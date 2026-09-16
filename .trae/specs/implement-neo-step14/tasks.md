# Step14 任务记录

- [x] 读取设计、step13c handoff 和当前代码；基线 NeoStep 67 项、2 项既有失败。
- [x] 确认范围：catch/finally + throw/rethrow/leave/endfinally；filter/fault 编译时拒绝。
- [x] catch 隐式输入接入复制传播、寄存器清理、类型重命名与槽位分配。
- [x] 内部 catch 入口指令最终命名为 `EnterCatch`，代码、lowering、执行器和设计文档保持一致。
- [x] EH 分块、最终指令范围、Push 删除后的地址同步与无末尾指令边界。
- [x] Neo 专用展开、嵌套 finally continuation、原异常身份和 EDI 重抛。
- [x] Frames / ManagedStack / ValueTypeStackPointer 清理；异常后同一 interpreter 复用。
- [x] newobj 保留 caller 暂存与失败回滚，扩大 try/finally 覆盖参数准备及构造调用。
- [x] CLR 反射 wrapper 解包、失败时清空参数缓存；异常诊断不解码 Legacy 栈。
- [x] EH 引用局部变量正确 null 初始化；采用对应局部变量类型 token。
- [x] 27 项测试：19 项普通异常控制流在 IL 方法中执行，8 项由 CLRHost 验证外部边界或特殊 IL。
- [x] Legacy Debug 构建通过，Test05：register=true / false 各 23 项全通过。
- [x] 改名及测试代码格式整理后重新验收：Debug_Neo、Release_Neo 的 Step14 均 27/27；完整 NeoStep 均为 94 项、仅保留 2 项基线失败。
- [x] 移除热路径上的 `ExecuteNeoCore` 额外调用层；profiler 的异常安全 `try/finally` 仅在 `DEBUG && !NO_PROFILER` 下编译。
- [x] 更新设计 §17 / §27.13、步骤状态、验收记录和 handoff。
