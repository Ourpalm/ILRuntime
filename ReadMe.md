ILRuntime
==========

This project is inpired by L# intepreter([LSharp Project](https://github.com/lightszero/LSharp "A Pure C# IL Runner,Run DLL as a Script" )), which is designed for the same purpose as we do, which is to provide a IL runtime to run c# code on enviorments without JIT. For example on iOS.

L# is a great project which created a good code base to accomplish the task, but it also has some limitations.
* L# doesn't support Generic types inside the runtime
* L# doesn't support inheritance of types outside the runtime
* The arthmetic operation on L# has relative poor performance compared to highly optimized runtime like luajit

So this project aims to develop a solid IL runtime to support as many features of IL as possible, and with highly optimized performance which is  as competitive as luajit.

Apporach
========
The basic part of the runtime, like resolving PE header, gathering meta information of types, and disassembling of IL instructions, we will take the same solution as L#, to use the Mono.Cecil library. 

The intepreter part, we will try to take elimminate the memory allcation for arthmetic operations and method invokation as much as possible. Also we want to make use of the advantage of unsafe code to boost the arthmetic operations and stack operation.

A very simple test case shows drastical performance improvement compared to LSharp:
```C#
        public static int foo(int init)
        {
            int b = init;
            for (int i = 0; i < 10000; i++)
            {
                b += i;
            }

            return b;
        }
        public static int foo()
        {
            int b = 0;
            for (int i = 0; i < 50; i++)
            {
                b += foo(b);
            }

            return b;
        }
```

Invokation of foo() takes about 1300ms in LSharp, but only takes about 40ms in ILRuntime.
