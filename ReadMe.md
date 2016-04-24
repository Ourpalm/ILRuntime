ILRuntime
==========

This project is inpired by L# intepreter, which is designed for the same purpose as we do, which is to provide a IL runtime to run c# code on enviorments without JIT. For example on iOS.

L# is a great project which created a good code base to accomplish the task, but it also has some limitations.
* L# doesn't support Generic types inside the runtime
* L# doesn't support inheritance of types outside the runtime
* The arthmetic operation on L# has relative poor performance compared to highly optimized runtime like luajit

So this project aims to develop a solid IL runtime to support as many features of IL as possible, and with highly optimized performance which is competitable to luajit.

Apporach
========
The basic part of the runtime, like resolving PE header, gathering meta information of types, and disassembling of IL instructions, we will take the same solution as L#, to use the Mono.Cecil library. 

The intepreter part, we will try to take the similar approach as luagit, to utilize the native stack and register as much as possible. 
