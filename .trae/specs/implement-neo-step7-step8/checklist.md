# Checklist

## Step7 — 引用类型基础
- [ ] Neo 帧入口会为 `TotalRefSize` 批量预留 mStack 槽
- [ ] 引用类型 local/temp 初始化后帧内值为 `-1`
- [ ] Neo 帧正常退出后 `mStack.Count` 恢复到 `frameRefBase`
- [ ] `Ldnull` 写入 `-1`
- [ ] `Ldstr` 把字符串对象写入目标 ref slot 对应的 mStack 槽
- [ ] 引用类型 `Move` 同步更新目标帧 slot 与目标 mStack ref slot
- [ ] 堆 IL 对象 primitive 字段 `Ldfld_*` / `Stfld_*` 正确读写 `ILTypeInstance.Primitives`
- [ ] 堆 IL 对象引用字段 `Ldfld_*` / `Stfld_*` 正确读写 `ILTypeInstance.ManagedObjects`
- [ ] Step6 中 Neo 模式跳过静态构造函数调用的临时分支已删除
- [ ] 静态字段初始化用例读取 `class C { static int X = 42; }` 得到 42

## Step8 — IL 方法调用
- [ ] Caller 将 primitive 参数按 callee param offset 写入 `esp`
- [ ] Caller 将 reference 参数的 mStack index 按 callee param offset 写入 `esp`
- [ ] `ExecuteNeo` 调用后 caller 的 `esp` 不被 callee 破坏
- [ ] primitive 返回值由 callee `Ret` 直接写入 caller `retDst`
- [ ] reference 返回值由 callee `Ret` 写入 caller `retRefBase`
- [ ] void 方法以 `retDst = null`、`retRefBase = 0` 调用且 `Ret` 不写返回
- [ ] 递归调用不会复用错误帧，也不会泄漏 mStack 引用槽

## Step8b — IL 引用类型 newobj
- [ ] `Newobj` 可分配 IL 引用类型 `ILTypeInstance`
- [ ] 新对象写入 caller 预分配目标 ref slot
- [ ] 构造函数 `this` 作为 param0 传入
- [ ] 带参数构造函数参数按 Step8 Call 约定写入 callee 帧
- [ ] 构造函数中的字段赋值可被后续字段读取验证
- [ ] CLR 引用类型 `newobj` 未被误实现为不完整路径，仍明确推迟到后续 Step

## 编译与测试
- [ ] `dotnet build -c Debug` 通过，0 错误
- [ ] `dotnet build -c Debug_Neo` 通过，0 错误
- [ ] Neo 模式 `NeoStep6Test` 在恢复 cctor 后保持通过
- [ ] Neo Step7-Step8 新增测试全部通过
- [ ] 测试执行发生在 Step8/8b 完成之后，没有在 Step7 中途运行端到端测试
