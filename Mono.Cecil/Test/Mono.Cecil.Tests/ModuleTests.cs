using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Mono.Cecil;

using NUnit.Framework;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class ModuleTests : BaseTestFixture {

		[Test]
		public void CreateModuleEscapesAssemblyName ()
		{
			var module = ModuleDefinition.CreateModule ("Test.dll", ModuleKind.Dll);
			Assert.AreEqual ("Test", module.Assembly.Name.Name);

			module = ModuleDefinition.CreateModule ("Test.exe", ModuleKind.Console);
			Assert.AreEqual ("Test", module.Assembly.Name.Name);
		}

		[Test]
		public void SingleModule ()
		{
			TestModule ("hello.exe", module => {
				var assembly = module.Assembly;

				Assert.AreEqual (1, assembly.Modules.Count);
				Assert.IsNotNull (assembly.MainModule);
			});
		}

		[Test]
		public void EntryPoint ()
		{
			TestModule ("hello.exe", module => {
				var entry_point = module.EntryPoint;
				Assert.IsNotNull (entry_point);

				Assert.AreEqual ("System.Void Program::Main()", entry_point.ToString ());
			});
		}

		[Test]
		public void MultiModules ()
		{
			IgnoreOnCoreClr ();

			TestModule("mma.exe", module => {
				var assembly = module.Assembly;

				Assert.AreEqual (3, assembly.Modules.Count);

				Assert.AreEqual ("mma.exe", assembly.Modules [0].Name);
				Assert.AreEqual (ModuleKind.Console, assembly.Modules [0].Kind);

				Assert.AreEqual ("moda.netmodule", assembly.Modules [1].Name);
				Assert.AreEqual ("eedb4721-6c3e-4d9a-be30-49021121dd92", assembly.Modules [1].Mvid.ToString ());
				Assert.AreEqual (ModuleKind.NetModule, assembly.Modules [1].Kind);

				Assert.AreEqual ("modb.netmodule", assembly.Modules [2].Name);
				Assert.AreEqual ("46c5c577-11b2-4ea0-bb3c-3c71f1331dd0", assembly.Modules [2].Mvid.ToString ());
				Assert.AreEqual (ModuleKind.NetModule, assembly.Modules [2].Kind);
			});
		}

		[Test]
		public void ModuleInformation ()
		{
			TestModule ("hello.exe", module => {
				Assert.IsNotNull (module);

				Assert.AreEqual ("hello.exe", module.Name);
				Assert.AreEqual (new Guid ("C3BC2BD3-2576-4D00-A80E-465B5632415F"), module.Mvid);
			});
		}

		[Test]
		public void AssemblyReferences ()
		{
			TestModule ("hello.exe", module => {
				Assert.AreEqual (1, module.AssemblyReferences.Count);

				var reference = module.AssemblyReferences [0];

				Assert.AreEqual ("mscorlib", reference.Name);
				Assert.AreEqual (new Version (2, 0, 0, 0), reference.Version);
				Assert.AreEqual (new byte [] { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 }, reference.PublicKeyToken);
			});
		}

		[Test]
		public void ModuleReferences ()
		{
			TestModule ("pinvoke.exe", module => {
				Assert.AreEqual (2, module.ModuleReferences.Count);
				Assert.AreEqual ("kernel32.dll", module.ModuleReferences [0].Name);
				Assert.AreEqual ("shell32.dll", module.ModuleReferences [1].Name);
			});
		}

		[Test]
		public void Types ()
		{
			TestModule ("hello.exe", module => {
				Assert.AreEqual (2, module.Types.Count);
				Assert.AreEqual ("<Module>", module.Types [0].FullName);
				Assert.AreEqual ("<Module>", module.GetType ("<Module>").FullName);
				Assert.AreEqual ("Program", module.Types [1].FullName);
				Assert.AreEqual ("Program", module.GetType ("Program").FullName);
			});
		}

		[Test]
		public void LinkedResource ()
		{
			TestModule ("libres.dll", module => {
				var resource = module.Resources.Where (res => res.Name == "linked.txt").First () as LinkedResource;
				Assert.IsNotNull (resource);

				Assert.AreEqual ("linked.txt", resource.Name);
				Assert.AreEqual ("linked.txt", resource.File);
				Assert.AreEqual (ResourceType.Linked, resource.ResourceType);
				Assert.IsTrue (resource.IsPublic);
			});
		}

		[Test]
		public void EmbeddedResource ()
		{
			TestModule ("libres.dll", module => {
				var resource = module.Resources.Where (res => res.Name == "embedded1.txt").First () as EmbeddedResource;
				Assert.IsNotNull (resource);

				Assert.AreEqual ("embedded1.txt", resource.Name);
				Assert.AreEqual (ResourceType.Embedded, resource.ResourceType);
				Assert.IsTrue (resource.IsPublic);

				using (var reader = new StreamReader (resource.GetResourceStream ()))
				Assert.AreEqual ("Hello", reader.ReadToEnd ());

				resource = module.Resources.Where (res => res.Name == "embedded2.txt").First () as EmbeddedResource;
				Assert.IsNotNull (resource);

				Assert.AreEqual ("embedded2.txt", resource.Name);
				Assert.AreEqual (ResourceType.Embedded, resource.ResourceType);
				Assert.IsTrue (resource.IsPublic);

				using (var reader = new StreamReader (resource.GetResourceStream ()))
				Assert.AreEqual ("World", reader.ReadToEnd ());
			});
		}

		[Test]
		public void ExportedTypeFromNetModule ()
		{
			IgnoreOnCoreClr ();

			TestModule ("mma.exe", module => {
				Assert.IsTrue (module.HasExportedTypes);
				Assert.AreEqual (2, module.ExportedTypes.Count);

				var exported_type = module.ExportedTypes [0];

				Assert.AreEqual ("Module.A.Foo", exported_type.FullName);
				Assert.AreEqual ("moda.netmodule", exported_type.Scope.Name);

				exported_type = module.ExportedTypes [1];

				Assert.AreEqual ("Module.B.Baz", exported_type.FullName);
				Assert.AreEqual ("modb.netmodule", exported_type.Scope.Name);
			});
		}

		[Test]
		public void NestedTypeForwarder ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				Assert.IsTrue (module.HasExportedTypes);
				Assert.AreEqual (2, module.ExportedTypes.Count);

				var exported_type = module.ExportedTypes [0];

				Assert.AreEqual ("System.Diagnostics.DebuggableAttribute", exported_type.FullName);
				Assert.AreEqual (Platform.OnCoreClr ? "System.Private.CoreLib" : "mscorlib", exported_type.Scope.Name);
				Assert.IsTrue (exported_type.IsForwarder);

				var nested_exported_type = module.ExportedTypes [1];

				Assert.AreEqual ("System.Diagnostics.DebuggableAttribute/DebuggingModes", nested_exported_type.FullName);
				Assert.AreEqual (exported_type, nested_exported_type.DeclaringType);
				Assert.AreEqual (Platform.OnCoreClr ? "System.Private.CoreLib" : "mscorlib", nested_exported_type.Scope.Name);
			});
		}

		[Test]
		public void HasTypeReference ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				Assert.IsTrue (module.HasTypeReference ("System.Attribute"));
				Assert.IsTrue (module.HasTypeReference (Platform.OnCoreClr ? "System.Private.CoreLib" : "mscorlib", "System.Attribute"));

				Assert.IsFalse (module.HasTypeReference ("System.Core", "System.Attribute"));
				Assert.IsFalse (module.HasTypeReference ("System.Linq.Enumerable"));
			});
		}

		[Test]
		public void Win32FileVersion ()
		{
			IgnoreOnCoreClr ();

			TestModule ("libhello.dll", module => {
				var version = FileVersionInfo.GetVersionInfo (module.FileName);

				Assert.AreEqual ("0.0.0.0", version.FileVersion);
			});
		}

		[Test]
		public void ModuleWithoutBlob ()
		{
			TestModule ("noblob.dll", module => {
				Assert.IsNull (module.Image.BlobHeap);
			});
		}

		[Test]
		public void MixedModeModule ()
		{
			using (var module = GetResourceModule ("cppcli.dll")) {
				Assert.AreEqual (1, module.ModuleReferences.Count);
				Assert.AreEqual (string.Empty, module.ModuleReferences [0].Name);
			}
		}

		[Test]
		public void OpenIrrelevantFile ()
		{
			Assert.Throws<BadImageFormatException> (() => GetResourceModule ("text_file.txt"));
		}

		[Test]
		public void GetTypeNamespacePlusName ()
		{
			using (var module = GetResourceModule ("moda.netmodule")) {
				var type = module.GetType ("Module.A", "Foo");
				Assert.IsNotNull (type);
			}
		}

		[Test]
		public void GetNonExistentTypeRuntimeName ()
		{
			using (var module = GetResourceModule ("libhello.dll")) {
				var type = module.GetType ("DoesNotExist", runtimeName: true);
				Assert.IsNull (type);
			}
		}

		[Test]
		public void OpenModuleImmediate ()
		{
			using (var module = GetResourceModule ("hello.exe", ReadingMode.Immediate)) {
				Assert.AreEqual (ReadingMode.Immediate, module.ReadingMode);
			}
		}

		[Test]
		public void OpenModuleDeferred ()
		{
			using (var module = GetResourceModule ("hello.exe", ReadingMode.Deferred)) {
				Assert.AreEqual (ReadingMode.Deferred, module.ReadingMode);
			}
		}

		[Test]
		public void OwnedStreamModuleFileName ()
		{
			var path = GetAssemblyResourcePath ("hello.exe");
			using (var file = File.Open (path, FileMode.Open))
			{
				using (var module = ModuleDefinition.ReadModule (file))
				{
					Assert.IsNotNull (module.FileName);
					Assert.IsNotEmpty (module.FileName);
					Assert.AreEqual (path, module.FileName);
				}
			}
		}

		[Test]
		public void ReadAndWriteFile ()
		{
			var path = Path.GetTempFileName ();

			var original = ModuleDefinition.CreateModule ("FooFoo", ModuleKind.Dll);
			var type = new TypeDefinition ("Foo", "Foo", TypeAttributes.Abstract | TypeAttributes.Sealed);
			original.Types.Add (type);
			original.Write (path);

			using (var module = ModuleDefinition.ReadModule (path, new ReaderParameters { ReadWrite = true })) {
				module.Write ();
			}

			using (var module = ModuleDefinition.ReadModule (path))
				Assert.AreEqual ("Foo.Foo", module.Types [1].FullName);
		}

		[Test]
		public void ExceptionInWriteDoesNotKeepLockOnFile ()
		{
			var path = Path.GetTempFileName ();

			var module = ModuleDefinition.CreateModule ("FooFoo", ModuleKind.Dll);
			// Mixed mode module that Cecil can not write
			module.Attributes = (ModuleAttributes) 0;

			Assert.Throws<NotSupportedException>(() => module.Write (path));

			// Ensure you can still delete the file
			File.Delete (path);
		}
	}
}
