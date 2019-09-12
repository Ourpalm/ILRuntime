using System;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using NUnit.Framework;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class MethodBodyTests : BaseTestFixture {

		[Test]
		public void MultiplyMethod ()
		{
			TestIL ("hello.il", module => {
				var foo = module.GetType ("Foo");
				Assert.IsNotNull (foo);

				var bar = foo.GetMethod ("Bar");
				Assert.IsNotNull (bar);
				Assert.IsTrue (bar.IsIL);

				AssertCode (@"
	.locals init (System.Int32 V_0)
	IL_0000: ldarg.0
	IL_0001: ldarg.1
	IL_0002: mul
	IL_0003: stloc.0
	IL_0004: ldloc.0
	IL_0005: call System.Void Foo::Baz(System.Int32)
	IL_000a: ret
", bar);
			});
		}

		[Test]
		public void PrintStringEmpty ()
		{
			TestIL ("hello.il", module => {
				var foo = module.GetType ("Foo");
				Assert.IsNotNull (foo);

				var print_empty = foo.GetMethod ("PrintEmpty");
				Assert.IsNotNull (print_empty);

				AssertCode (@"
	.locals ()
	IL_0000: ldsfld System.String System.String::Empty
	IL_0005: call System.Void System.Console::WriteLine(System.String)
	IL_000a: ret
", print_empty);
			});
		}

		[Test]
		public void Branch ()
		{
			TestModule ("libhello.dll", module => {
				var lib = module.GetType ("Library");
				Assert.IsNotNull (lib);

				var method = lib.GetMethod ("GetHelloString");
				Assert.IsNotNull (method);

				AssertCode (@"
	.locals init (System.String V_0)
	IL_0000: nop
	IL_0001: ldstr ""hello world of tomorrow""
	IL_0006: stloc.0
	IL_0007: br.s IL_0009
	IL_0009: ldloc.0
	IL_000a: ret
", method);
			});
		}

		[Test]
		public void Switch ()
		{
			TestModule ("switch.exe", module => {
				var program = module.GetType ("Program");
				Assert.IsNotNull (program);

				var method = program.GetMethod ("Main");
				Assert.IsNotNull (method);

				AssertCode (@"
	.locals init (System.Int32 V_0)
	IL_0000: ldarg.0
	IL_0001: ldlen
	IL_0002: conv.i4
	IL_0003: stloc.0
	IL_0004: ldloc.0
	IL_0005: ldc.i4.8
	IL_0006: bgt.s IL_0026
	IL_0008: ldloc.0
	IL_0009: ldc.i4.1
	IL_000a: sub
	IL_000b: switch (IL_0032, IL_0034, IL_0038, IL_0034)
	IL_0020: ldloc.0
	IL_0021: ldc.i4.8
	IL_0022: beq.s IL_0036
	IL_0024: br.s IL_0038
	IL_0026: ldloc.0
	IL_0027: ldc.i4.s 16
	IL_0029: beq.s IL_0036
	IL_002b: ldloc.0
	IL_002c: ldc.i4.s 32
	IL_002e: beq.s IL_0036
	IL_0030: br.s IL_0038
	IL_0032: ldc.i4.0
	IL_0033: ret
	IL_0034: ldc.i4.1
	IL_0035: ret
	IL_0036: ldc.i4.2
	IL_0037: ret
	IL_0038: ldc.i4.s 42
	IL_003a: ret
", method);
			});
		}

		[Test]
		public void MethodSpec ()
		{
			TestIL ("methodspecs.il", module => {
				var tamtam = module.GetType ("Tamtam");

				var bar = tamtam.GetMethod ("Bar");
				Assert.IsNotNull (bar);

				AssertCode (@"
	.locals ()
	IL_0000: ldc.i4.2
	IL_0001: call System.Void Tamtam::Foo<System.Int32>(TFoo)
	IL_0006: ret
", bar);
			});
		}

		[Test]
		public void NestedTryCatchFinally ()
		{
			TestModule ("catch.exe", module => {
				var program = module.GetType ("Program");
				var main = program.GetMethod ("Main");
				Assert.IsNotNull (main);

				AssertCode (@"
	.locals ()
	IL_0000: call System.Void Program::Foo()
	IL_0005: leave.s IL_000d
	IL_0007: call System.Void Program::Baz()
	IL_000c: endfinally
	IL_000d: leave.s IL_001f
	IL_000f: pop
	IL_0010: call System.Void Program::Bar()
	IL_0015: leave.s IL_001f
	IL_0017: pop
	IL_0018: call System.Void Program::Bar()
	IL_001d: leave.s IL_001f
	IL_001f: leave.s IL_0027
	IL_0021: call System.Void Program::Baz()
	IL_0026: endfinally
	IL_0027: ret
	.try IL_0000 to IL_0007 finally handler IL_0007 to IL_000d
	.try IL_0000 to IL_000f catch System.ArgumentException handler IL_000f to IL_0017
	.try IL_0000 to IL_000f catch System.Exception handler IL_0017 to IL_001f
	.try IL_0000 to IL_0021 finally handler IL_0021 to IL_0027
", main);
			});
		}

		[Test]
		public void FunctionPointersAndCallSites ()
		{
			TestModule ("fptr.exe", module => {
				var type = module.Types [0];
				var start = type.GetMethod ("Start");
				Assert.IsNotNull (start);

				AssertCode (@"
	.locals init ()
	IL_0000: ldc.i4.1
	IL_0001: call method System.Int32 *(System.Int32) MakeDecision::Decide()
	IL_0006: calli System.Int32(System.Int32)
	IL_000b: call System.Void System.Console::WriteLine(System.Int32)
	IL_0010: ldc.i4.1
	IL_0011: call method System.Int32 *(System.Int32) MakeDecision::Decide()
	IL_0016: calli System.Int32(System.Int32)
	IL_001b: call System.Void System.Console::WriteLine(System.Int32)
	IL_0020: ldc.i4.1
	IL_0021: call method System.Int32 *(System.Int32) MakeDecision::Decide()
	IL_0026: calli System.Int32(System.Int32)
	IL_002b: call System.Void System.Console::WriteLine(System.Int32)
	IL_0030: ret
", start);
			}, verify: false);
		}

		[Test]
		public void ThisParameter ()
		{
			TestIL ("hello.il", module => {
				var type = module.GetType ("Foo");
				var method = type.GetMethod ("Gazonk");

				Assert.IsNotNull (method);

				AssertCode (@"
	.locals ()
	IL_0000: ldarg 0
	IL_0004: pop
	IL_0005: ret
", method);

				Assert.AreEqual (method.Body.ThisParameter.ParameterType, type);
				Assert.AreEqual (method.Body.ThisParameter, method.Body.Instructions [0].Operand);
			});
		}

		[Test]
		public void ThisParameterStaticMethod ()
		{
			var static_method = typeof (ModuleDefinition).ToDefinition ().Methods.Where (m => m.IsStatic).First ();
			Assert.IsNull (static_method.Body.ThisParameter);
		}

		[Test]
		public void ThisParameterPrimitive ()
		{
			var int32 = typeof (int).ToDefinition ();
			var int_to_string = int32.Methods.Where (m => m.Name == "ToString" && m.Parameters.Count == 0).First();
			Assert.IsNotNull (int_to_string);

			var this_parameter_type = int_to_string.Body.ThisParameter.ParameterType;
			Assert.IsTrue (this_parameter_type.IsByReference);

			var element_type = ((ByReferenceType) this_parameter_type).ElementType;
			Assert.AreEqual (int32, element_type);
		}

		[Test]
		public void ThisParameterValueType ()
		{
			var token = typeof (MetadataToken).ToDefinition ();
			var token_to_string = token.Methods.Where (m => m.Name == "ToString" && m.Parameters.Count == 0).First ();
			Assert.IsNotNull (token_to_string);

			var this_parameter_type = token_to_string.Body.ThisParameter.ParameterType;
			Assert.IsTrue (this_parameter_type.IsByReference);

			var element_type = ((ByReferenceType) this_parameter_type).ElementType;
			Assert.AreEqual (token, element_type);
		}

		[Test]
		public void ThisParameterObject ()
		{
			var module = typeof (MethodBodyTests).ToDefinition ().Module;
			var @object = module.TypeSystem.Object.Resolve ();
			var method = @object.Methods.Where (m => m.HasBody).First ();

			var type = method.Body.ThisParameter.ParameterType;
			Assert.IsFalse (type.IsValueType);
			Assert.IsFalse (type.IsPrimitive);
			Assert.IsFalse (type.IsPointer);
		}

		[Test]
		public void FilterMaxStack ()
		{
			TestIL ("hello.il", module => {
				var type = module.GetType ("Foo");
				var method = type.GetMethod ("TestFilter");

				Assert.IsNotNull (method);
				Assert.AreEqual (2, method.Body.MaxStackSize);
			});
		}

		[Test]
		public void BranchOutsideMethod ()
		{
			OnlyOnWindows (); // Mono's ilasm doesn't support branching outside of method

			TestIL ("branch-out.il", module => {
				var type = module.GetType ("Foo");
				var method = type.GetMethod ("BranchOutside");

				Assert.IsNotNull (method);
				Assert.IsNotNull (method.Body);

				var leave = method.Body.Instructions [0];
				Assert.AreEqual (OpCodes.Leave, leave.OpCode);
				Assert.IsNull (leave.Operand);
				Assert.AreEqual ("IL_0000: leave", leave.ToString ());
			}, verify: false);
		}

		[Test]
		public void Iterator ()
		{
			TestModule ("iterator.exe", module => {
				var method = module.GetType ("Program").GetMethod ("GetLittleArgs");
				Assert.IsNotNull (method.Body);
			});
		}

		[Test]
		public void LoadString ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var type = module.GetType ("FooAttribute");
				var get_fiou = type.GetMethod ("get_Fiou");
				Assert.IsNotNull (get_fiou);

				var ldstr = get_fiou.Body.Instructions.Where (i => i.OpCode == OpCodes.Ldstr).First ();
				Assert.AreEqual ("fiou", ldstr.Operand);
			});
		}

		[Test]
		public void UnattachedMethodBody ()
		{
			var system_void = typeof (void).ToDefinition ();
			var method = new MethodDefinition ("NewMethod", MethodAttributes.Assembly | MethodAttributes.Static, system_void);
			var body = new MethodBody (method);
			var il = body.GetILProcessor ();
			il.Emit (OpCodes.Ret);
			method.Body = body;

			Assert.AreEqual (body, method.Body);
		}

		[Test]
		public void AddInstruction ()
		{
			var object_ref = new TypeReference ("System", "Object", null, null, false);
			var method = new MethodDefinition ("foo", MethodAttributes.Static, object_ref);
			var body = new MethodBody (method);

			var il = body.GetILProcessor ();

			var first = il.Create (OpCodes.Nop);
			var second = il.Create (OpCodes.Nop);

			body.Instructions.Add (first);
			body.Instructions.Add (second);

			Assert.IsNull (first.Previous);
			Assert.AreEqual (second, first.Next);
			Assert.AreEqual (first, second.Previous);
			Assert.IsNull (second.Next);
		}

		[Test]
		public void InsertInstruction ()
		{
			var object_ref = new TypeReference ("System", "Object", null, null, false);
			var method = new MethodDefinition ("foo", MethodAttributes.Static, object_ref);
			var body = new MethodBody (method);

			var il = body.GetILProcessor ();

			var first = il.Create (OpCodes.Nop);
			var second = il.Create (OpCodes.Nop);
			var third = il.Create (OpCodes.Nop);

			body.Instructions.Add (first);
			body.Instructions.Add (third);

			Assert.IsNull (first.Previous);
			Assert.AreEqual (third, first.Next);
			Assert.AreEqual (first, third.Previous);
			Assert.IsNull (third.Next);

			body.Instructions.Insert (1, second);

			Assert.IsNull (first.Previous);
			Assert.AreEqual (second, first.Next);
			Assert.AreEqual (first, second.Previous);
			Assert.AreEqual (third, second.Next);
			Assert.AreEqual (second, third.Previous);
			Assert.IsNull (third.Next);
		}

		[Test]
		public void InsertAfterLastInstruction ()
		{
			var object_ref = new TypeReference ("System", "Object", null, null, false);
			var method = new MethodDefinition ("foo", MethodAttributes.Static, object_ref);
			var body = new MethodBody (method);

			var il = body.GetILProcessor ();

			var first = il.Create (OpCodes.Nop);
			var second = il.Create (OpCodes.Nop);
			var third = il.Create (OpCodes.Nop);

			body.Instructions.Add (first);
			body.Instructions.Add (second);

			Assert.IsNull (first.Previous);
			Assert.AreEqual (second, first.Next);
			Assert.AreEqual (first, second.Previous);
			Assert.IsNull (second.Next);

			body.Instructions.Insert (2, third);

			Assert.IsNull (first.Previous);
			Assert.AreEqual (second, first.Next);
			Assert.AreEqual (first, second.Previous);
			Assert.AreEqual (third, second.Next);
			Assert.AreEqual (second, third.Previous);
			Assert.IsNull (third.Next);
		}

		[Test]
		public void RemoveInstruction ()
		{
			var object_ref = new TypeReference ("System", "Object", null, null, false);
			var method = new MethodDefinition ("foo", MethodAttributes.Static, object_ref);
			var body = new MethodBody (method);

			var il = body.GetILProcessor ();

			var first = il.Create (OpCodes.Nop);
			var second = il.Create (OpCodes.Nop);
			var third = il.Create (OpCodes.Nop);

			body.Instructions.Add (first);
			body.Instructions.Add (second);
			body.Instructions.Add (third);

			Assert.IsNull (first.Previous);
			Assert.AreEqual (second, first.Next);
			Assert.AreEqual (first, second.Previous);
			Assert.AreEqual (third, second.Next);
			Assert.AreEqual (second, third.Previous);
			Assert.IsNull (third.Next);

			body.Instructions.Remove (second);

			Assert.IsNull (first.Previous);
			Assert.AreEqual (third, first.Next);
			Assert.AreEqual (first, third.Previous);
			Assert.IsNull (third.Next);
		}
	}
}
