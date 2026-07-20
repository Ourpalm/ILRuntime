# ILRuntime Neo Step 12b - Implementation Plan

## 工程约束(每个 Task 必须遵守)
- **局部变量复用**:ExecuteNeo case 块内严禁声明新的同类型局部变量;临时变量统一在方法头部共享区([ILIntepreter.Neo.cs L412-L418](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L412-L418))声明
- **零运行时查表**:所有偏移编译期填入 OpCodeR 字段,handler 不访问 localInfos/fieldOffsets
- **无 fallback**:未实现能力直接 throw NotImplementedException 标注归属 step,不回退 Legacy/VTOR
- **不改 NeoStep12Test 用例**:10 个用例是验收基准,只改运行时/JIT
- **Ref Slot 编码按 design §15.2**,不重新设计

## Task Dependencies
```
Task 0 (设计文档回填 D1/D2)               [前置,独立]
Task 1 (Ref Slot slot 分配 + stackBase 基址)
  └─→ Task 2 (Ref Slot 生产指令 Ldloca/Ldarga/Ldflda/Ldsflda)
       └─→ Task 3 (Ref Slot 消费指令 Stind_*/Ldind_* 三分派 + sub-int 扩展)
            └─→ Task 4 (Ldfld_*/Stfld_* 新增 Ref Slot receiver 路径)
                 └─→ Task 5 (struct-this ABI 改 Ref Slot by-ref)
       └─→ Task 6 (Ldsfld_*/Stsfld_* handler + cctor 闭环)   [依赖 Task 1]
       └─→ Task 7 (sub-int 扩展审计 Ldfld_*/Ldsfld_*)          [依赖 Task 2/6]
Task 8 (默认分支拆分显式 case)              [贯穿 Task 2/3/6]
Task 9 (可选:Ldfld/Stfld inline fold pass) [依赖 Task 4,可延后]
Task 10 (全量回归 + handoff + D3 回填)      [最后]
```

---

## [x] Task 0: 设计文档回填(D1 / D2)
- **Priority**: high
- **Depends On**: None
- **Description**:
  - **D1**:在 [object-model-neo-design.md](file:///f:/SVN/ILRuntime/.trae/documents/object-model-neo-design.md) §4.3 新增 "两种宽度规则并存" 段落:
    - 独立帧内 primitive slot(local + temp)按 CIL evaluation-stack 宽度(sub-int widen 到 4 字节)
    - struct 内部字段(堆/帧)按 CLR StructLayout 自然对齐,sub-int 占实际字节数
    - `Ldfld_*/Ldsfld_*/Ldind_*` 加载 sub-int 到独立 slot 时做符号/零扩展;反向 `Stfld_*/Stsfld_*/Stind_*` 按字段宽度截断
    - `Move/Move_Vt` 独立 slot 间按 slot 宽度整体拷贝,不涉及扩展/截断
  - **D2**:§2 之后新增 "§2.5 基础抽象:Ref Slot(managed pointer 表示)",把 §15.2 的 8 字节 `(objectIndex, offset)` 定义前置提炼;§15 保留 Ldloca/Ldflda/Ldsflda/Stind/Ldind 具体指令语义,不重复编码定义
- **AC**: FR-9, AC-11
- **Test**: 人工审阅文档,§4.3 / §2.5 内容准确

---

## [x] Task 1: Ref Slot slot 类型分配 + stackBase 绝对基址
- **Priority**: high
- **Depends On**: Task 0
- **Description**:
  - `StackSlotInfo` / `AllocateSlotForType` 支持 byref slot:`Size=8, RefCount=0`
  - `AllocateLocalStackSpaces` 为 byref 类型的 local/temp/param 分配 8 字节 slot;冲突取 max size(design §15.5)
  - ExecuteNeo 入口缓存 runtime stack 绝对基址 `byte* stackBase`(`nativePointer`/`StackBase`),供 `objectIndex==-1` 时 `stackBase + offset` 还原(Open Question 确认用哪个)
  - 在共享变量区追加所需新变量(如 `int objIndex; byte* refTarget;`),不在 case 内声明
- **AC**: FR-1
- **Test**:
  - `-c Debug_Neo` 编译 0 错误
  - byref local slot 分配 8 字节,`(frameBase - stackBase)` 计算正确(断点/单测验证)

---

## [x] Task 2: Ref Slot 生产指令 handler
- **Priority**: high
- **Depends On**: Task 1
- **Description**:
  - `Ldloca / Ldloca_S / Ldarga / Ldarga_S` → 写 `(-1, (frameBase - stackBase) + slotOffset)` 到 8 字节目标 slot
  - `Ldflda`:
    - 堆 IL 对象 → `(objMStackIndex, fieldPrimitiveOffset)`
    - 帧内值类型 / receiver 为 Ref Slot → `(-1, baseAbsOffset + fieldPrimitiveOffset)` 累加
    - CLR 对象 → `(objMStackIndex, fieldHash)`
  - `Ldsflda` → 触发 `type.StaticInstance`(cctor),产出 `(staticInstanceMStackIndex, fieldPrimitiveOffset)`
  - JIT Translate:为上述指令目标寄存器分配 byref slot,字段偏移编码进 Operand;LowerNeoOffsets 下降偏移
- **AC**: FR-2, AC-1
- **Test**:
  - `ref int x = ref local` 场景 Ldloca 产出 `(-1, abs)`
  - `ref int f = ref obj.field` 场景 Ldflda 堆对象产出 `(objIdx, fieldOff)`

---

## [x] Task 3: Ref Slot 消费指令 Stind_*/Ldind_* 三分派
- **Priority**: high
- **Depends On**: Task 2
- **Description**:
  - `Stind_I/I1/I2/I4/I8/R4/R8/Ref` 与 `Ldind_I/I1/U1/I2/U2/I4/U4/I8/R4/R8/Ref` 按 Ref Slot `objectIndex` 分派:
    - `-1` → `*(T*)(stackBase + offset)` 直接读写
    - `>=0 && ILTypeInstance` → 重取 `Primitives` managed ref 后按 offset 读写(GC safe)
    - `>=0 && CLR 对象` → `CLRType.GetFieldValue/SetFieldValue(fieldHash)` + 值类型回写 `mStack[objectIndex]=obj`
    - Array → 抛 NotImpl("Step 16")
  - `Ldind_U1/I1/U2/I2` 做零/符号扩展到 4 字节 dst(见 Task 7 规则)
  - `Stind_*` 按目标宽度截断写入
  - handler 复用共享变量,不声明 case-scope 局部变量
- **AC**: FR-3, FR-7(部分), AC-1, AC-4(部分)
- **Test**:
  - 帧内 ref 写回:`void Inc(ref int x){x++}` 调用后 caller local 被修改
  - IL 对象字段 ref 写回正确

---

## [x] Task 4: Ldfld_*/Stfld_* 新增 Ref Slot receiver 路径
- **Priority**: high
- **Depends On**: Task 3
- **Description**:
  - 现有两路径(heap mStack index / 帧内 struct direct)基础上,新增第三路径:receiver 寄存器持 8 字节 Ref Slot
  - 读出 `(objIndex, off)` 后按 objIndex 三分派(帧内 stackBase+off / ILTypeInstance Primitives / CLR),再加字段 `Operand2` 定位读写
  - **不产生 struct 拷贝,不 copyback**
  - JIT 侧根据 receiver 寄存器 slot 是否为 byref/struct-this 选择 receiver 变体;确定 Operand4(或新字段)的三态编码(见 Open Question)
  - 重审 [JITCompiler.cs L1211/L1244](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1211) 的 `Operand4=(type.IsValueType && !type.IsEnum)?1:0`:改为基于 receiver 寄存器是否持 Ref Slot
  - handler 复用共享变量
- **AC**: FR-4, AC-2(部分)
- **Test**:
  - receiver 为 Ref Slot 时 Ldfld/Stfld 读写命中正确内存
  - 原 heap 路径(NeoStep7-11)回归绿

---

## [x] Task 5: struct-this ABI 改为 Ref Slot by-ref
- **Priority**: high
- **Depends On**: Task 4
- **Description**:
  - [JITCompiler.cs L460-L469](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L460-L469):struct this 参数 slot 从整块 struct 改为 8 字节 Ref Slot(Size=8, RefCount=0)
  - Caller emit `Ldloca_S t` 得到 `(-1, absoluteFrameOffset)`,写入 callee 参数区
  - `CopyNeoCallArguments` / `NeoCallParamMap` 对 struct-this 参数改为拷贝 8 字节 Ref Slot(非整块 struct)
  - Callee `ldarg.0` 加载 Ref Slot,`this.field` 走 Task 4 的 Ref Slot receiver 路径
  - 移除隐含的 by-value copyback 逻辑(确认无回退隐患)
- **AC**: FR-5, AC-2
- **Test**:
  - `NeoStep12ThisTarget::Bump` 类用例:struct 实例方法修改字段写回 caller 原始 struct
  - Move this→local(如有)无 DEBUG layout mismatch

---

## [x] Task 6: Ldsfld_*/Stsfld_* handler + cctor 闭环
- **Priority**: high
- **Depends On**: Task 1
- **Description**:
  - `Ldsfld_*` / `Stsfld_*` 覆盖 primitive + reference + Value(struct copy)变体
  - IL 类型:`type.StaticInstance` 触发 cctor,`field.PrimitiveOffset` 读写 `Primitives`,引用字段走 `staticInstance.ManagedObjects[refOffset]`
  - CLR 类型:`CLRType.GetStaticFieldValue/SetStaticFieldValue`
  - JIT:静态字段偏移用正确 fieldOffsets 编码进指令(修正 [Ldflda L1218-L1221](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/JITCompiler.cs#L1218) 现有 GetStaticFieldIndex 硬编码,与本 step Ref Slot 语义对齐)
  - handler 复用共享变量
- **AC**: FR-6, AC-3
- **Test**:
  - `class C{static int X=42}` 首次 Ldsfld 触发 cctor,读到 42
  - Stsfld 写入后 Ldsfld 读回一致

---

## [x] Task 7: sub-int 符号/零扩展审计
- **Priority**: high
- **Depends On**: Task 2, Task 6
- **Description**:
  - 审计 `Ldfld_U1/I1/U2/I2/Boolean`(现有,见 [L1687-L1726](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L1687)):确认无符号补0、有符号符号扩展
  - 新增/修正 `Ldsfld_*` 对应 sub-int 变体(Task 6 产物)
  - 新增 `Ldind_U1/I1/U2/I2` 对应变体(Task 3 产物)
  - `Stfld_*/Stsfld_*/Stind_*` 按字段宽度截断即可,无需额外处理
- **AC**: FR-7, AC-4
- **Test**:
  - sbyte/byte/short/ushort/bool 字段读到 int slot 后与常量比较正确(高位无残值)

---

## [x] Task 8: 默认分支拆分为显式 case
- **Priority**: medium
- **Depends On**: Task 2, Task 3, Task 6(随实现推进)
- **Description**:
  - 把 `Ldloca_S/Ldarga/Ldflda/Ldsflda/Ldsfld/Stsfld/Stind_*/Ldind_*` 从 ExecuteNeo 默认 `not yet implemented (Step 6)`([L2009](file:///f:/SVN/ILRuntime/ILRuntime/Runtime/Intepreter/RegisterVM/ILIntepreter.Neo.cs#L2009))拆为显式 case
  - 尚未实现的分支(Array ldind/stind 等)显式 NotImpl 标注归属 step
- **AC**: FR-8, AC-8
- **Test**: grep 验证上述指令不再落默认兜底

---

## [x] Task 9: (可选)Ldfld/Stfld inline fold pass
- **Priority**: low
- **Depends On**: Task 4
- **Description**:
  - **实现方案(替代 fold)**:选择在 JIT AllocateLocalStackSpaces 后加一个 `PropagateByRefReferentOffsets` pass,把 byref slot 的 `StackSlotInfo.RefOffset` 字段语义重定义为"referent struct 的 RefOffset"。这样 Ref Slot 编码 `Operand4 = -1 - localInfos[reg].RefOffset` 在 Ldloca/Ldarga 产生的 byref slot 上就直接携带了 receiver struct 的 RefOffset,不再依赖 fold
  - byref slot `RefCount=0`,分配时 `RefOffset` 字段本来不参与引用区计数,重定义为 referent RefOffset 是自洽的
  - `Ldloca/Ldarga`:复制源 local/parameter slot 的 RefOffset;`Ldsflda`:0;`Ldflda`:留 0 + 注释(receiver+field.ReferenceOffset 组合待 Step 17 扩展 Ldflda 编码时启用)
  - 效果:同帧 `ldloca V + stfld_ref f`、`ldloca V + initobj f` 在**不 fold** 的情况下即可正确解引用引用字段;避免让 fold 成为正确性依赖
- **AC**: FR-10
- **Test**: NeoStep12StructWithRef 不依赖 fold 通过

---

## [ ] Task 10: 全量回归 + handoff + D3 回填
- **Priority**: high
- **Depends On**: Task 5, Task 6, Task 7, Task 8
- **Description**:
  - Neo:NeoStep6/7/8/10/11(31 个)+ NeoStep12(10 个)全绿
  - `-c Debug` 与 `-c Debug_Neo` 双配置 0 错误 0 警告
  - Legacy 回归(useRegister=false)无回归
  - **D3**:回填 [neo-implementation-steps.md](file:///f:/SVN/ILRuntime/.trae/documents/neo-implementation-steps.md) 依赖关系:Step 12b 是 Step 13/16/17/18 前置;Step 16 ldelema 依赖 Step 12b;Step 17 精简为跨帧 ref/out 封送;Step 18 依赖 Step 12b
  - 撰写 handoff.md,附 "ECMA-335 合规状态" 一节:列 A1-A5 闭环情况 + 向后续 step 转移的未闭环项;更新 project_memory
  - 复审 audit §1 全部违规项无回退
- **AC**: AC-5, AC-6, AC-7, AC-8
- **Test**: 全部测试通过;audit §1 复审通过

---

# Task Dependencies(汇总)
- Task 1 depends on Task 0
- Task 2 depends on Task 1
- Task 3 depends on Task 2
- Task 4 depends on Task 3
- Task 5 depends on Task 4
- Task 6 depends on Task 1
- Task 7 depends on Task 2, Task 6
- Task 8 depends on Task 2, Task 3, Task 6
- Task 9 depends on Task 4(可选)
- Task 10 depends on Task 5, Task 6, Task 7, Task 8
