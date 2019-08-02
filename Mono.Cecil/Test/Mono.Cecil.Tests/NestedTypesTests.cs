using System;

using Mono.Cecil;

using NUnit.Framework;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class NestedTypesTests : BaseTestFixture {

		[Test]
		public void NestedTypes ()
		{
			TestCSharp ("NestedTypes.cs", module => {
				var foo = module.GetType ("Foo");

				Assert.AreEqual ("Foo", foo.Name);
				Assert.AreEqual ("Foo", foo.FullName);
				Assert.AreEqual (module, foo.Module);
				Assert.AreEqual (1, foo.NestedTypes.Count);

				var bar = foo.NestedTypes [0];

				Assert.AreEqual ("Bar", bar.Name);
				Assert.AreEqual ("Foo/Bar", bar.FullName);
				Assert.AreEqual (module, bar.Module);
				Assert.AreEqual (1, bar.NestedTypes.Count);

				var baz = bar.NestedTypes [0];

				Assert.AreEqual ("Baz", baz.Name);
				Assert.AreEqual ("Foo/Bar/Baz", baz.FullName);
				Assert.AreEqual (module, baz.Module);
			});
		}

		[Test]
		public void DirectNestedType ()
		{
			TestCSharp ("NestedTypes.cs", module => {
				var bingo = module.GetType ("Bingo");
				var get_fuel = bingo.GetMethod ("GetFuel");

				Assert.AreEqual ("Bingo/Fuel", get_fuel.ReturnType.FullName);
			});
		}

		[Test]
		public void NestedTypeWithOwnNamespace ()
		{
			TestModule ("bug-185.dll", module => {
				var foo = module.GetType ("Foo");
				var foo_child = foo.NestedTypes [0];

				Assert.AreEqual ("<IFoo<System.Byte[]>", foo_child.Namespace);
				Assert.AreEqual ("Do>d__0", foo_child.Name);

				Assert.AreEqual ("Foo/<IFoo<System.Byte[]>.Do>d__0", foo_child.FullName);
			});
		}

		[Test]
		public void NestedTypeFullName ()
		{
			var foo = new TypeDefinition (null, "Foo", TypeAttributes.Class);
			var bar = new TypeDefinition (null, "Bar", TypeAttributes.Class);
			var baz = new TypeDefinition (null, "Baz", TypeAttributes.Class);

			foo.NestedTypes.Add (bar);
			bar.NestedTypes.Add (baz);

			Assert.AreEqual ("Foo/Bar/Baz", baz.FullName);

			foo.Namespace = "Change";

			Assert.AreEqual ("Change.Foo/Bar", bar.FullName);
			Assert.AreEqual ("Change.Foo/Bar/Baz", baz.FullName);

			bar.Namespace = "AnotherChange";

			Assert.AreEqual ("Change.Foo/AnotherChange.Bar", bar.FullName);
			Assert.AreEqual ("Change.Foo/AnotherChange.Bar/Baz", baz.FullName);

			foo.Name = "FooFoo";

			Assert.AreEqual ("Change.FooFoo/AnotherChange.Bar", bar.FullName);
			Assert.AreEqual ("Change.FooFoo/AnotherChange.Bar/Baz", baz.FullName);
		}
	}
}
