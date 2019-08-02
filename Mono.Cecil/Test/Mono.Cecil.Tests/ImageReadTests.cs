using System;
using System.IO;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.PE;
using Mono.Cecil.Metadata;

using NUnit.Framework;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class ImageReadTests : BaseTestFixture {

		[Test]
		public void ImageSections ()
		{
			using (var image = GetResourceImage ("hello.exe")) {
				Assert.AreEqual (3, image.Sections.Length);
				Assert.AreEqual (".text", image.Sections [0].Name);
				Assert.AreEqual (".rsrc", image.Sections [1].Name);
				Assert.AreEqual (".reloc", image.Sections [2].Name);
			}
		}

		[Test]
		public void ImageMetadataVersion ()
		{
			using (var image = GetResourceImage ("hello.exe"))
				Assert.AreEqual (TargetRuntime.Net_2_0, image.RuntimeVersion.ParseRuntime ());

			using (var image = GetResourceImage ("hello1.exe"))
				Assert.AreEqual (TargetRuntime.Net_1_1, image.RuntimeVersion.ParseRuntime ());
		}

		[Test]
		public void ImageModuleKind ()
		{
			using (var image = GetResourceImage ("hello.exe"))
				Assert.AreEqual (ModuleKind.Console, image.Kind);

			using (var image = GetResourceImage ("libhello.dll"))
				Assert.AreEqual (ModuleKind.Dll, image.Kind);

			using (var image = GetResourceImage ("hellow.exe"))
				Assert.AreEqual (ModuleKind.Windows, image.Kind);
		}

		[Test]
		public void MetadataHeaps ()
		{
			using (var image = GetResourceImage ("hello.exe")) {
				Assert.IsNotNull (image.TableHeap);

				Assert.IsNotNull (image.StringHeap);
				Assert.AreEqual (string.Empty, image.StringHeap.Read (0));
				Assert.AreEqual ("<Module>", image.StringHeap.Read (1));

				Assert.IsNotNull (image.UserStringHeap);
				Assert.AreEqual (string.Empty, image.UserStringHeap.Read (0));
				Assert.AreEqual ("Hello Cecil World !", image.UserStringHeap.Read (1));

				Assert.IsNotNull (image.GuidHeap);
				Assert.AreEqual (new Guid (), image.GuidHeap.Read (0));
				Assert.AreEqual (new Guid ("C3BC2BD3-2576-4D00-A80E-465B5632415F"), image.GuidHeap.Read (1));

				Assert.IsNotNull (image.BlobHeap);
				Assert.AreEqual (new byte [0], image.BlobHeap.Read (0));
			}
		}

		[Test]
		public void TablesHeap ()
		{
			using (var image = GetResourceImage ("hello.exe")) {
				var heap = image.TableHeap;

				Assert.IsNotNull (heap);

				Assert.AreEqual (1, heap [Table.Module].Length);
				Assert.AreEqual (4, heap [Table.TypeRef].Length);
				Assert.AreEqual (2, heap [Table.TypeDef].Length);
				Assert.AreEqual (0, heap [Table.Field].Length);
				Assert.AreEqual (2, heap [Table.Method].Length);
				Assert.AreEqual (4, heap [Table.MemberRef].Length);
				Assert.AreEqual (2, heap [Table.CustomAttribute].Length);
				Assert.AreEqual (1, heap [Table.Assembly].Length);
				Assert.AreEqual (1, heap [Table.AssemblyRef].Length);
			}
		}

		[Test]
		public void X64Module ()
		{
			TestModule ("hello.x64.exe", module => {
				Assert.AreEqual (TargetArchitecture.AMD64, module.Image.Architecture);
				Assert.AreEqual (ModuleAttributes.ILOnly, module.Image.Attributes);
			}, verify: !Platform.OnMono);
		}

		[Test]
		public void X64ModuleTextOnlySection ()
		{
			TestModule ("hello.textonly.x64.exe", module => {
				Assert.AreEqual (TargetArchitecture.AMD64, module.Image.Architecture);
				Assert.AreEqual (ModuleAttributes.ILOnly, module.Image.Attributes);
			}, verify: !Platform.OnMono);
		}

		[Test]
		public void IA64Module ()
		{
			TestModule ("hello.ia64.exe", module => {
				Assert.AreEqual (TargetArchitecture.IA64, module.Image.Architecture);
				Assert.AreEqual (ModuleAttributes.ILOnly, module.Image.Attributes);
			}, verify: !Platform.OnMono);
		}

		[Test]
		public void X86Module ()
		{
			TestModule ("hello.x86.exe", module => {
				Assert.AreEqual (TargetArchitecture.I386, module.Image.Architecture);
				Assert.AreEqual (ModuleAttributes.ILOnly | ModuleAttributes.Required32Bit, module.Image.Attributes);
			});
		}

		[Test]
		public void AnyCpuModule ()
		{
			TestModule ("hello.anycpu.exe", module => {
				Assert.AreEqual (TargetArchitecture.I386, module.Image.Architecture);
				Assert.AreEqual (ModuleAttributes.ILOnly, module.Image.Attributes);
			});
		}

		[Test]
		public void DelaySignedAssembly ()
		{
			TestModule ("delay-signed.dll", module => {
				Assert.IsNotNull (module.Assembly.Name.PublicKey);
				Assert.AreNotEqual (0, module.Assembly.Name.PublicKey.Length);
				Assert.AreNotEqual (ModuleAttributes.StrongNameSigned, module.Attributes & ModuleAttributes.StrongNameSigned);
				Assert.AreNotEqual (0, module.Image.StrongName.VirtualAddress);
				Assert.AreNotEqual (0, module.Image.StrongName.Size);
			});
		}

		[Test]
		public void WindowsPhoneNonSignedAssembly ()
		{
			TestModule ("wp7.dll", module => {
				Assert.AreEqual (0, module.Assembly.Name.PublicKey.Length);
				Assert.AreNotEqual (ModuleAttributes.StrongNameSigned, module.Attributes & ModuleAttributes.StrongNameSigned);
				Assert.AreEqual (0, module.Image.StrongName.VirtualAddress);
				Assert.AreEqual (0, module.Image.StrongName.Size);
			}, verify: false);
		}

		[Test]
		public void MetroAssembly ()
		{
			if (Platform.OnMono)
				return;

			TestModule ("metro.exe", module => {
				Assert.AreEqual (ModuleCharacteristics.AppContainer, module.Characteristics & ModuleCharacteristics.AppContainer);
			}, verify: false);
		}

#if !NET_CORE
		[Test]
		public void WindowsRuntimeComponentAssembly ()
		{
			var resolver = WindowsRuntimeAssemblyResolver.CreateInstance ();
			if (resolver == null)
				return;

			TestModule("winrtcomp.winmd", module => {
				Assert.IsTrue (module.Assembly.Name.IsWindowsRuntime);
				Assert.AreEqual (6, module.Image.SubSystemMajor);
				Assert.AreEqual (2, module.Image.SubSystemMinor);
			}, verify: false, assemblyResolver: resolver);
		}
#endif
		[Test]
		public void DeterministicAssembly ()
		{
			TestModule ("Deterministic.dll", module => {
				Assert.IsTrue (module.HasDebugHeader);

				var header = module.GetDebugHeader ();

				Assert.AreEqual (1, header.Entries.Length);
				Assert.IsTrue (header.Entries.Any (e => e.Directory.Type == ImageDebugType.Deterministic));
			});
		}

		[Test]
		public void Net471TargetingAssembly ()
		{
			TestModule ("net471.exe", module => {
				Assert.AreEqual (6, module.Image.SubSystemMajor);
				Assert.AreEqual (0, module.Image.SubSystemMinor);
			});
		}

		[Test]
		public void ExternalPdbDeterministicAssembly ()
		{
			TestModule ("ExternalPdbDeterministic.dll", module => {
				Assert.IsTrue (module.HasDebugHeader);

				var header = module.GetDebugHeader ();

				Assert.AreEqual (2, header.Entries.Length);
				Assert.IsTrue (header.Entries.Any (e => e.Directory.Type == ImageDebugType.CodeView));
				Assert.IsTrue (header.Entries.Any (e => e.Directory.Type == ImageDebugType.Deterministic));
			}, symbolReaderProvider: typeof (PortablePdbReaderProvider), symbolWriterProvider: typeof (PortablePdbWriterProvider));
		}

		[Test]
		public void EmbeddedPdbDeterministicAssembly ()
		{
			TestModule ("EmbeddedPdbDeterministic.dll", module => {
				Assert.IsTrue (module.HasDebugHeader);

				var header = module.GetDebugHeader ();

				Assert.AreEqual (3, header.Entries.Length);
				Assert.IsTrue (header.Entries.Any (e => e.Directory.Type == ImageDebugType.CodeView));
				Assert.IsTrue (header.Entries.Any (e => e.Directory.Type == ImageDebugType.Deterministic));
				Assert.IsTrue (header.Entries.Any (e => e.Directory.Type == ImageDebugType.EmbeddedPortablePdb));
			}, symbolReaderProvider: typeof (EmbeddedPortablePdbReaderProvider), symbolWriterProvider: typeof (EmbeddedPortablePdbWriterProvider));
		}
	}
}
