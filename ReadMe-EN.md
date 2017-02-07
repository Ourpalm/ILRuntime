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

WARNING!
========
Please mainly use this library for bug fixing!! ADDING/HIDING/CHANING features in your APP may violate some platform's guideline(for example iOS), your APP may get REJECTED or even BANNED if you do so.

This libray should not be used for evading Apple's review system, if you do this, it will be on your own risk!

Get Started
========
Unity
----------
If you want to use ILRuntime in Unity, you need to copy the following source code into your project's Assets folder:
* Mono.Cecil.20
* Mono.Cecil.Pdb for VS compiled assembly or Mono.Cecil.Mdb for mono compiled assembly
* ILRuntime

The bin, obj, Properties sub folder should not be copied into unity folder, otherwise it may cause problem. The project files(.csproj) are not needed either, but it shoudn't cause any problem either.

ILRuntime uses unsafe code, so you must enable unsafe mode to use ILRuntime, you can enable it like this:
* Create a file named:smcs.rsp in Assets folder
* Open smcs.rsp with text editor and add "-unsafe" to it.

Visual Studio
----------
For Visual Studio you only need to reference Mono.Cecil.20,ILRuntime,Mono.Cecil.Pdb or Mono.Cecil.Mdb's assembly.

Usage
----------
For start using ILRuntime, you may follow these instructions:
* Reference or copy the needed dependencies like described above
* Make a instance of ILRuntime.Runtime.Enviorment.AppDomain, this class is the entry point of ILRuntime
* Use appDomain.LoadAssembly to load a dll file, and the corresponding symbol file. You should specify the symbol reader if you want to use symbol file(for example, Mono.Cecil.Pdb.PdbReaderProvider for .pdb symbol)
* Use appDomain.Invoke to run a static method of specified type
* You can get all loaded types via appDomain.LoadedTypes property. All types in ILRuntime are represented with IType interface. You can instantiate a ILRuntime type by using: ((ILType)type).Instantiate()

Delegates
----------
In order to support platforms, where JIT are not allowed, we can't use reflection to create delegate types. So you need to register the delegate type, before you can use it.

You only need to register delegate types with different method signature, different delegate types with the same parameters and return value are only needed to register once.

To register a delegate type, you need to call appDomain.DelegateManager.RegisterMethodDelegate<ParamType1,ParamType2...>() for methods, appDomain.DelegateManager.RegisterFunctionDelegate<ParamType1, ParamType2, ..., ReturnType>() for functions

If you want to use an delegate instance created in ILRuntime outside ILRuntime, then you need to make a Delegate Converter for it. ILRuntime uses Action<T> and Func<T> internal for delegates, so such delegate types have builtin converter, and you don't need to write converter for such types.

A typical Delegate Converter should look like this:
```C#
app.DelegateManager.RegisterDelegateConvertor<DelegateType>((action) =>
{
    return new DelegateType((a) =>
    {
       ((Action<ParamType1>)action)(a);
    });
});
```

Inheritance
----------
Before you can inherit a type declared outside ILRuntime, you need to define a Adaptor for it.  A typical Adaptor should look like this:
```C#
    //All adaptors should inherit CrossBindingAdaptor
    public class ClassInheritanceAdaptor : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(ClassInheritanceTest);//This is the type to be inherited
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adaptor);//This is the actual Adaptor class for it
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);//Creating a new instance
        }

		//The Adaptor class should inherit the type you want to inherit from ILRuntime, and implement the CrossBindingAdaptorType interface
        class Adaptor : ClassInheritanceTest, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;
            IMethod mTestAbstract;
            IMethod mTestVirtual;
            bool isTestVirtualInvoking = false;

            public Adaptor()
            {

            }

            public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }
            
			//The adaptor class should override all virtual and abstract methods declared in the base type, and redirect the call to the ILRuntime instance
            public override void TestAbstract()
            {
                if(mTestAbstract == null)
                {
                    mTestAbstract = instance.Type.GetMethod("TestAbstract", 0);
                }
                if (mTestAbstract != null)
                    appdomain.Invoke(mTestAbstract, instance);
            }

            public override void TestVirtual()
            {
                if (mTestVirtual == null)
                {
                    mTestVirtual = instance.Type.GetMethod("TestVirtual", 0);
                }
				//For virtual method, you must add a bool variable to determine if it's already invoking, otherwise it will cause stackoverflow if you call base.TestVirtual() inside ILRuntime
                if (mTestVirtual != null && !isTestVirtualInvoking)
                {
                    isTestVirtualInvoking = true;
                    appdomain.Invoke(mTestVirtual, instance);
                    isTestVirtualInvoking = false;
                }
                else
                    base.TestVirtual();
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
```

Running Test Project
-----------
Running the test project is quite simple. 
* Compile the whole solution
* Run the ILRuntimeTest project
* Select the TestCases.dll
* The tests are run automatically and output the results in Console and Window

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
* Delegates
* Inheritance of classes outside ILRuntime

##Planned
* Multi-dimensional Arrays
* Reflection support
* All IL instructions
* Implementation of interfaces outside ILRuntime

##Experimental, timeline uncertain
* Debugger support
* Visual Studio integration
* Debugging in Visual Studio
* Remote debugging on other device

Known Issues and Limitations
==============================
* ILRuntime is still been develeoped, you should not use it in production enviorment yet!
* Code like below is not supported
```C#
//The following code is decleared in ILRuntime:
class SubType : BaseType
{

}

class Test
{
    void foo()
    {
        SubType instance = BaseType.Make<SubType>();
        //This instance is invalid, because it is not able to create ILRuntime instance in CLR like this
    }
}

//=====================================
//This type is decleared in native CLR
class BaseType
{
    public static T Make<T>()
        where T:new()
    {
        return new T();
    }
}
```
