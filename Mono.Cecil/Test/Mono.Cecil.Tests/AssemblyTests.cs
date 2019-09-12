using System;

using Mono.Cecil;
using Mono.Cecil.Cil;

using NUnit.Framework;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class AssemblyTests : BaseTestFixture {

		[Test]
		public void Name ()
		{
			TestModule ("hello.exe", module => {
				var name = module.Assembly.Name;

				Assert.IsNotNull (name);

				Assert.AreEqual ("hello", name.Name);
				Assert.AreEqual ("", name.Culture);
				Assert.AreEqual (new Version (0, 0, 0, 0), name.Version);
				Assert.AreEqual (AssemblyHashAlgorithm.SHA1, name.HashAlgorithm);
			});
		}

		[Test]
		public void ParseLowerCaseNameParts ()
		{
			var name = AssemblyNameReference.Parse ("Foo, version=2.0.0.0, culture=fr-FR");
			Assert.AreEqual ("Foo", name.Name);
			Assert.AreEqual (2, name.Version.Major);
			Assert.AreEqual (0, name.Version.Minor);
			Assert.AreEqual ("fr-FR", name.Culture);
		}

		[Test]
		public void ZeroVersion ()
		{
			var name = new AssemblyNameReference ("Foo", null);
			Assert.AreEqual ("0.0.0.0", name.Version.ToString (fieldCount: 4));
			Assert.AreEqual ("Foo, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", name.FullName);

			name = new AssemblyNameReference ("Foo", new Version (0, 0, 0, 0));
			Assert.AreEqual ("0.0.0.0", name.Version.ToString (fieldCount: 4));
			Assert.AreEqual ("Foo, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", name.FullName);
		}

		[Test]
		public void NoBuildOrMajor ()
		{
			var name = new AssemblyNameReference ("Foo", new Version (0, 0));
			Assert.AreEqual ("Foo, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", name.FullName);

			name = new AssemblyNameReference ("Foo", new Version (0, 0, 0));
			Assert.AreEqual ("Foo, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null", name.FullName);
		}

		[Test]
		public void Retargetable ()
		{
			if (Platform.OnCoreClr)
				return;

			TestModule ("RetargetableExample.dll", module => {
				var type = module.Types [1];
				var property = type.Properties [0];
				var attribute = property.CustomAttributes [0];

				var argumentType = ((CustomAttributeArgument) attribute.ConstructorArguments [0].Value).Type;
				var reference = (AssemblyNameReference) argumentType.Scope;

				Assert.AreEqual (
					"System.Data, Version=3.5.0.0, Culture=neutral, PublicKeyToken=969db8053d3322ac, Retargetable=Yes",
					reference.FullName);
			}, verify: !Platform.OnMono);
		}

		[Test]
		public void SystemRuntime ()
		{
			if (Platform.OnCoreClr)
				return;

			TestModule ("System.Runtime.dll", module => {
				Assert.AreEqual ("System.Runtime", module.Assembly.Name.Name);
				Assert.AreEqual (1, module.AssemblyReferences.Count);
				Assert.AreNotEqual (module, module.TypeSystem.CoreLibrary);
				Assert.AreEqual (module.AssemblyReferences [0], module.TypeSystem.CoreLibrary);
			}, verify: !Platform.OnMono);
		}

		[Test]
		public void MismatchedLibraryAndSymbols ()
		{
			// SQLite-net.dll (from nuget) shiped with mismatched symbol files, but throwIfNoSymbol did not prevent it from throwing
			var parameters = new ReaderParameters {
				ReadSymbols = true,
				SymbolReaderProvider = new DefaultSymbolReaderProvider (throwIfNoSymbol: false),
				ThrowIfSymbolsAreNotMatching = false,
			};

			using (var module = GetResourceModule ("SQLite-net.dll", parameters)) {
				Assert.Null (module.SymbolReader);
			}
		}
	}
}
