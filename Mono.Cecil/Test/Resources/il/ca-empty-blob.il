.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89)
  .ver 2:0:0:0
}

.assembly CaEmptyBlob {}

.module CaEmptyBlob.dll

.class public auto ansi CustomAttribute extends [mscorlib]System.Attribute
{
	.custom instance void CustomAttribute::.ctor() = ()

	.method public hidebysig specialname rtspecialname instance void .ctor()
	{
		ldarg.0
		call instance void [mscorlib]System.Attribute::.ctor()
		ret
	}
}
