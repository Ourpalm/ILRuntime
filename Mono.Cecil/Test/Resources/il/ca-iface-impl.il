.assembly extern mscorlib {}

.assembly CA {}

.module CA.dll

.class public auto ansi beforefieldinit FooAttribute
	extends [mscorlib]System.Attribute
{
	.method public hidebysig specialname rtspecialname instance void .ctor ()
	{
		ldarg.0
		call instance void [mscorlib]System.Attribute::.ctor ()
		ret
	}
}

.class interface public auto ansi abstract IFoo
{
}

.class public auto ansi beforefieldinit FooType
	extends [mscorlib]System.Object
	implements IFoo
{
	.interfaceimpl type IFoo
	.custom instance void FooAttribute::.ctor ()

	.method public hidebysig specialname rtspecialname instance void .ctor ()
	{
		ldarg.0
		call instance void [mscorlib]System.Object::.ctor ()
		ret
	}
}
