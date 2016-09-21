ILRuntime
==========

This project is inpired by L# intepreter([LSharp Project](https://github.com/lightszero/LSharp "A Pure C# IL Runner,Run DLL as a Script" )), which is designed for the same purpose as we do, which is to provide a IL runtime to run c# code on enviorments without JIT. For example on iOS.

L# is a great project which created a good code base to accomplish the task, but it also has some limitations.
* L# doesn't support Generic types inside the runtime
* L# doesn't support inheritance of types outside the runtime
* The arthmetic operation on L# has relative poor performance compared to highly optimized runtime like luajit

So this project aims to develop a solid IL runtime to support as many features of IL as possible, and with highly optimized performance which is  as competitive as luajit.

Our Vision
========
Our vision is to create a reliable high performance IL runtime which is also as productive as possible. The entire runtime will include the following features

* Full support for Generics, both for types inside and outside the ILRuntime
* Full support for type Inheritance from ILRuntime to CLR
* Complete framework for Debugging, which supports break points, and common debugger actions like Step in, Step out, Step through.
* Framework for inspecting stack and object information
* Either Visual Studio integration or Standalone Debugger with GUI.

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

StackFrame and Managed Object Stack
---------------------
In order to make the arthmetic operation fast enough, we must use unsafe code and manage the stack ourselfs. To accomplish this task, we've designed a ValueType to represent every slot in managed Stack.
```C#
    struct StackObject
    {
        public ObjectTypes ObjectType;
        public int Value;
        public int ValueLow;
    }
    enum ObjectTypes
    {
        Null,
        Integer,
        Long,
        Float,
        Double,
        StackObjectReference,//Value = pointer, 
        StaticFieldReference,
        Object,
        FieldReference,//Value = objIdx, ValueLow = fieldIdx
        ArrayReference,//Value = objIdx, ValueLow = elemIdx
    }
```
With this structure we can represent all kinds of objects on a managed Stack. It has a field indicates, which type of object it is. And it has 2 32bits Value fields, which can represent values from byte to long, it is also capable to represent a field reference.

This structure is not sufficient for storing a reference type value. Therefore we have to instantiate a List<object> to store object references. In such case, the Value field stores the index of the object reference in the List.

Managed Call Stack
-----------------------------
In order to make function call, we need to push all parameters onto the stack, and we need to allocate memory for all local variables, and we need to pass the return value to the caller. The Stack manipulation of entering and leaving the method body is illustrated like below:

```
EnterFrame:                            LeaveFrame:
|---------------|                     |---------------|
|   Argument1   |     |-------------->|  [ReturnVal]  |
|---------------|     |               |---------------|
|      ...      |     |               |     NULL      |
|---------------|     |               |---------------|
|   ArgumentN   |     |               |      ...      |
|---------------|     |
|   LocalVar1   |     |
|---------------|     |
|      ...      |     |
|---------------|     |
|   LocalVarN   |     |
|---------------|     |
|   FrameBase   |     |
|---------------|     |
|  [ReturnVal]  |------
|---------------|
```
After entering the Call stack frame, the stack pointer(to simplify the whole thing we just call it 'esp') to the FrameBase in the illustration. The local variables are stored in esp - n to esp -1. And the arguments are stored before local varialbes, which are pushed onto stack by the caller.

After executing the ret instruction, we copy the return value onto the position, where the first argument was, and zero out all the memories behind it, the esp is then pointed to the return value.

Roadmaps
==============================================

##Implemented
* Basic Stack operations
* Majorities of IL instructions
* Type Systems
* Value types and Reference Types
* Enums
* Virtual Methods
* Inheritance of classes inside the Runtime
* Implementation of interfaces insided the Runtime
* Generics for both inside the Runtime and for CLR types
* Exception handling
* CLR Method redirections
* Call stack and local variable dumper

##Planned
* Delegates
* Inheritance of classes and interfaces from 
* All IL instructions

##Experimental, timeline uncertain
* Debugger support
* Visual Studio integration
* Debugging in Visual Studio
* Remote debugging on other device
