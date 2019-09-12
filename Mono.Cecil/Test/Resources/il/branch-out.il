.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89)
  .ver 2:0:0:0
}

.assembly BranchOut {}

.module BranchOut.dll

.class private auto ansi Foo {

	.method public specialname rtspecialname instance void .ctor () cil managed
	{
		ldarg.0
		call instance void [mscorlib]System.Object::.ctor ()
		ret
	}

	.method public static void BranchOutside ()
	{
		.maxstack 2
		.locals init ()
		.try
		{
			leave afterMethod
		}
		finally
		{
			endfinally
		}
	afterMethod:
	}
}
