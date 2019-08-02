using System;
using System.Collections.Generic;
using System.IO;
using SR = System.Reflection;
using System.Runtime.CompilerServices;

using Mono.Cecil.Cil;

using NUnit.Framework;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class ImportReflectionTests : BaseTestFixture {

		[Test]
		public void ImportString ()
		{
			var get_string = Compile<Func<string>> ((_, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldstr, "yo dawg!");
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual ("yo dawg!", get_string ());
		}

		[Test]
		public void ImportInt ()
		{
			var add = Compile<Func<int, int, int>> ((_, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Add);
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual (42, add (40, 2));
		}

		[Test]
		public void ImportStringByRef ()
		{
			var get_string = Compile<Func<string, string>> ((module, body) => {
				var type = module.Types [1];

				var method_by_ref = new MethodDefinition {
					Name = "ModifyString",
					IsPrivate = true,
					IsStatic = true,
				};

				type.Methods.Add (method_by_ref);

				method_by_ref.MethodReturnType.ReturnType = module.ImportReference (typeof (void));

				method_by_ref.Parameters.Add (new ParameterDefinition (module.ImportReference (typeof (string))));
				method_by_ref.Parameters.Add (new ParameterDefinition (module.ImportReference (typeof (string).MakeByRefType ())));

				var m_il = method_by_ref.Body.GetILProcessor ();
				m_il.Emit (OpCodes.Ldarg_1);
				m_il.Emit (OpCodes.Ldarg_0);
				m_il.Emit (OpCodes.Stind_Ref);
				m_il.Emit (OpCodes.Ret);

				var v_0 = new VariableDefinition (module.ImportReference (typeof (string)));
				body.Variables.Add (v_0);

				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldnull);
				il.Emit (OpCodes.Stloc, v_0);
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldloca, v_0);
				il.Emit (OpCodes.Call, method_by_ref);
				il.Emit (OpCodes.Ldloc_0);
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual ("foo", get_string ("foo"));
		}

		[Test]
		public void ImportStringArray ()
		{
			var identity = Compile<Func<string [,], string [,]>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ret);
			});

			var array = new string [2, 2];

			Assert.AreEqual (array, identity (array));
		}

		[Test]
		public void ImportFieldStringEmpty ()
		{
			var get_empty = Compile<Func<string>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldsfld, module.ImportReference (typeof (string).GetField ("Empty")));
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual ("", get_empty ());
		}

		[Test]
		public void ImportStringConcat ()
		{
			var concat = Compile<Func<string, string, string>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Call, module.ImportReference (typeof (string).GetMethod ("Concat", new [] { typeof (string), typeof (string) })));
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual ("FooBar", concat ("Foo", "Bar"));
		}

		[Test]
		public void GeneratedAssemblyCulture ()
		{
			var id = Compile<Func<int, int>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual ("", id.Method.DeclaringType.Assembly.GetName ().CultureInfo.Name);
		}

		public class Generic<T> {
			public T Field;

			public T Method (T t)
			{
				return t;
			}

			public TS GenericMethod<TS> (T t, TS s)
			{
				return s;
			}

			public Generic<TS> ComplexGenericMethod<TS> (T t, TS s)
			{
				return new Generic<TS> { Field = s };
			}
		}

		[Test]
		public void ImportGenericField ()
		{
			if (Platform.OnCoreClr)
				return;

			var get_field = Compile<Func<Generic<string>, string>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldfld, module.ImportReference (typeof (Generic<string>).GetField ("Field")));
				il.Emit (OpCodes.Ret);
			});

			var generic = new Generic<string> {
				Field = "foo",
			};

			Assert.AreEqual ("foo", get_field (generic));
		}

		[Test]
		public void ImportGenericMethod ()
		{
			if (Platform.OnCoreClr)
				return;

			var generic_identity = Compile<Func<Generic<int>, int, int>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Callvirt, module.ImportReference (typeof (Generic<int>).GetMethod ("Method")));
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual (42, generic_identity (new Generic<int> (), 42));
		}

		[Test]
		public void ImportGenericMethodSpec ()
		{
			if (Platform.OnCoreClr)
				return;

			var gen_spec_id = Compile<Func<Generic<string>, int, int>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldnull);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Callvirt, module.ImportReference (typeof (Generic<string>).GetMethod ("GenericMethod").MakeGenericMethod (typeof (int))));
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual (42, gen_spec_id (new Generic<string> (), 42));
		}

		[Test]
		public void ImportComplexGenericMethodSpec ()
		{
			if (Platform.OnCoreClr)
				return;

			var gen_spec_id = Compile<Func<Generic<string>, int, int>> ((module, body) => {
				var il = body.GetILProcessor ();
				il.Emit (OpCodes.Ldarg_0);
				il.Emit (OpCodes.Ldnull);
				il.Emit (OpCodes.Ldarg_1);
				il.Emit (OpCodes.Callvirt, module.ImportReference (typeof (Generic<string>).GetMethod ("ComplexGenericMethod").MakeGenericMethod (typeof (int))));
				il.Emit (OpCodes.Ldfld, module.ImportReference (typeof (Generic<int>).GetField ("Field")));
				il.Emit (OpCodes.Ret);
			});

			Assert.AreEqual (42, gen_spec_id (new Generic<string> (), 42));
		}

		public class Foo<TFoo> {
			public List<TFoo> list;
		}

		[Test]
		public void ImportGenericTypeDefOrOpen ()
		{
			using (var module = typeof (Foo<>).ToDefinition ().Module) {
				var foo_def = module.ImportReference (typeof (Foo<>));
				var foo_open = module.ImportReference (typeof (Foo<>), foo_def);

				Assert.AreEqual ("Mono.Cecil.Tests.ImportReflectionTests/Foo`1", foo_def.FullName);
				Assert.AreEqual ("Mono.Cecil.Tests.ImportReflectionTests/Foo`1<TFoo>", foo_open.FullName);
			}
		}

		[Test]
		public void ImportGenericTypeFromContext ()
		{
			var list_foo = typeof (Foo<>).GetField ("list").FieldType;
			var generic_list_foo_open = typeof (Generic<>).MakeGenericType (list_foo);

			var foo_def = typeof (Foo<>).ToDefinition ();
			using (var module = foo_def.Module) {
				var generic_foo = module.ImportReference (generic_list_foo_open, foo_def);

				Assert.AreEqual ("Mono.Cecil.Tests.ImportReflectionTests/Generic`1<System.Collections.Generic.List`1<TFoo>>",
					generic_foo.FullName);
			}
		}

		[Test]
		public void ImportGenericTypeDefFromContext ()
		{
			var foo_open = typeof (Foo<>).MakeGenericType (typeof (Foo<>).GetGenericArguments () [0]);
			var generic_foo_open = typeof (Generic<>).MakeGenericType (foo_open);

			var foo_def = typeof (Foo<>).ToDefinition ();
			using (var module = foo_def.Module) {
				var generic_foo = module.ImportReference (generic_foo_open, foo_def);

				Assert.AreEqual ("Mono.Cecil.Tests.ImportReflectionTests/Generic`1<Mono.Cecil.Tests.ImportReflectionTests/Foo`1<TFoo>>",
					generic_foo.FullName);
			}
		}

		[Test]
		public void ImportArrayTypeDefFromContext ()
		{
			var foo_open = typeof (Foo<>).MakeGenericType (typeof (Foo<>).GetGenericArguments () [0]);
			var foo_open_array = foo_open.MakeArrayType ();

			var foo_def = typeof (Foo<>).ToDefinition ();
			using (var module = foo_def.Module) {
				var array_foo = module.ImportReference (foo_open_array, foo_def);

				Assert.AreEqual ("Mono.Cecil.Tests.ImportReflectionTests/Foo`1<TFoo>[]",
					array_foo.FullName);
			}
		}

		[Test]
		public void ImportGenericFieldFromContext ()
		{
			if (Platform.OnCoreClr)
				return;

			var list_foo = typeof (Foo<>).GetField ("list").FieldType;
			var generic_list_foo_open = typeof (Generic<>).MakeGenericType (list_foo);
			var generic_list_foo_open_field = generic_list_foo_open.GetField ("Field");

			var foo_def = typeof (Foo<>).ToDefinition ();
			using (var module = foo_def.Module) {
				var generic_field = module.ImportReference (generic_list_foo_open_field, foo_def);

				Assert.AreEqual ("T Mono.Cecil.Tests.ImportReflectionTests/Generic`1<System.Collections.Generic.List`1<TFoo>>::Field",
					generic_field.FullName);
			}
		}

		[Test]
		public void ImportGenericMethodFromContext ()
		{
			if (Platform.OnCoreClr)
				return;

			var list_foo = typeof (Foo<>).GetField ("list").FieldType;
			var generic_list_foo_open = typeof (Generic<>).MakeGenericType (list_foo);
			var generic_list_foo_open_method = generic_list_foo_open.GetMethod ("Method");

			var foo_def = typeof (Foo<>).ToDefinition ();
			using (var module = foo_def.Module) {
				var generic_method = module.ImportReference (generic_list_foo_open_method, foo_def);

				Assert.AreEqual ("T Mono.Cecil.Tests.ImportReflectionTests/Generic`1<System.Collections.Generic.List`1<TFoo>>::Method(T)",
					generic_method.FullName);
			}
		}

		[Test]
		public void ImportMethodOnOpenGenericType ()
		{
			using (var module = typeof (Generic<>).ToDefinition ().Module) {
				var method = module.ImportReference (typeof (Generic<>).GetMethod ("Method"));

				Assert.AreEqual ("T Mono.Cecil.Tests.ImportReflectionTests/Generic`1<T>::Method(T)", method.FullName);
			}
		}

		[Test]
		public void ImportGenericMethodOnOpenGenericType ()
		{
			using (var module = typeof (Generic<>).ToDefinition ().Module) {
				var generic_method = module.ImportReference (typeof (Generic<>).GetMethod ("GenericMethod"));

				Assert.AreEqual ("TS Mono.Cecil.Tests.ImportReflectionTests/Generic`1<T>::GenericMethod(T,TS)", generic_method.FullName);

				generic_method = module.ImportReference (typeof (Generic<>).GetMethod ("GenericMethod"), generic_method);

				Assert.AreEqual ("TS Mono.Cecil.Tests.ImportReflectionTests/Generic`1<T>::GenericMethod<TS>(T,TS)", generic_method.FullName);
			}
		}

		delegate void Emitter (ModuleDefinition module, MethodBody body);

		static TDelegate Compile<TDelegate> (Emitter emitter, [CallerMemberName] string testMethodName = null)
			where TDelegate : class
		{
			var name = "ImportReflection_" + testMethodName;

			var module = CreateTestModule<TDelegate> (name, emitter);
			var assembly = LoadTestModule (module);

			return CreateRunDelegate<TDelegate> (GetTestCase (name, assembly));
		}

		static TDelegate CreateRunDelegate<TDelegate> (Type type)
			where TDelegate : class
		{
			return (TDelegate) (object) Delegate.CreateDelegate (typeof (TDelegate), type.GetMethod ("Run"));
		}

		static Type GetTestCase (string name, SR.Assembly assembly)
		{
			return assembly.GetType (name);
		}

		static SR.Assembly LoadTestModule (ModuleDefinition module)
		{
			using (var stream = new MemoryStream ()) {
				module.Write (stream);
				File.WriteAllBytes (Path.Combine (Path.Combine (Path.GetTempPath (), "cecil"), module.Name + ".dll"), stream.ToArray ());
				return SR.Assembly.Load (stream.ToArray ());
			}
		}

		static ModuleDefinition CreateTestModule<TDelegate> (string name, Emitter emitter)
		{
			var module = CreateModule (name);

			var type = new TypeDefinition (
				"",
				name,
				TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Abstract,
				module.ImportReference (typeof (object)));

			module.Types.Add (type);

			var method = CreateMethod (type, typeof (TDelegate).GetMethod ("Invoke"));

			emitter (module, method.Body);

			return module;
		}

		static MethodDefinition CreateMethod (TypeDefinition type, SR.MethodInfo pattern)
		{
			var module = type.Module;

			var method = new MethodDefinition {
				Name = "Run",
				IsPublic = true,
				IsStatic = true,
			};

			type.Methods.Add (method);

			method.MethodReturnType.ReturnType = module.ImportReference (pattern.ReturnType);

			foreach (var parameter_pattern in pattern.GetParameters ())
				method.Parameters.Add (new ParameterDefinition (module.ImportReference (parameter_pattern.ParameterType)));

			return method;
		}

		static ModuleDefinition CreateModule (string name)
		{
			return ModuleDefinition.CreateModule (name, ModuleKind.Dll);
		}
	}
}
