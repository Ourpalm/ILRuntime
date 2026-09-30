# Step17 设计补充

Ref Slot ABI 保持设计 §2.5 的 8 字节 `(objectIndex, offset)`。Step17 实现对 §15.6 与 §25 作如下补充：

1. 引用字段位置根据持有对象/活动帧的类型布局找到引用区，不增加 -2 marker。跨帧写入必须使用地址所属帧的引用槽。
2. CLR 嵌套值类型字段不能只把中间 box 放进 mStack 后单层回写；需要保留父位置，逐层提交到原字段或数组。实现以 mStack 中的 CLRFieldAddressFallback 保存这条链，不扩大 Ref Slot。
3. ldflda/ldelema 和 byref Move 需要稳定 owner 锚点；原引用变量或循环临时槽的复用不能改变已经取出的地址。
4. ref return 可以指向仍存活的 caller 帧；禁止的是当前 retiring frame 的地址。堆引用返回时将 owner 重新锚定到 caller。
5. 只读 verification、CLR reflection 的 ref alias / 异常写回，以及 CLR 方法直接返回 ref 的限制详见 handoff，不隐式回退为错误的值拷贝。
