using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

using Mono.Cecil.Cil;
using Mono.Cecil.PE;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class PortablePdbTests : BaseTestFixture {

		[Test]
		public void SequencePoints ()
		{
			TestPortablePdbModule (module => {
				var type = module.GetType ("PdbTarget.Program");
				var main = type.GetMethod ("Main");

				AssertCode (@"
	.locals init (System.Int32 a, System.String[] V_1, System.Int32 V_2, System.String arg)
	.line 21,21:3,4 'C:\sources\PdbTarget\Program.cs'
	IL_0000: nop
	.line 22,22:4,11 'C:\sources\PdbTarget\Program.cs'
	IL_0001: nop
	.line 22,22:24,28 'C:\sources\PdbTarget\Program.cs'
	IL_0002: ldarg.0
	IL_0003: stloc.1
	IL_0004: ldc.i4.0
	IL_0005: stloc.2
	.line hidden 'C:\sources\PdbTarget\Program.cs'
	IL_0006: br.s IL_0017
	.line 22,22:13,20 'C:\sources\PdbTarget\Program.cs'
	IL_0008: ldloc.1
	IL_0009: ldloc.2
	IL_000a: ldelem.ref
	IL_000b: stloc.3
	.line 23,23:5,20 'C:\sources\PdbTarget\Program.cs'
	IL_000c: ldloc.3
	IL_000d: call System.Void System.Console::WriteLine(System.String)
	IL_0012: nop
	.line hidden 'C:\sources\PdbTarget\Program.cs'
	IL_0013: ldloc.2
	IL_0014: ldc.i4.1
	IL_0015: add
	IL_0016: stloc.2
	.line 22,22:21,23 'C:\sources\PdbTarget\Program.cs'
	IL_0017: ldloc.2
	IL_0018: ldloc.1
	IL_0019: ldlen
	IL_001a: conv.i4
	IL_001b: blt.s IL_0008
	.line 25,25:4,22 'C:\sources\PdbTarget\Program.cs'
	IL_001d: ldc.i4.1
	IL_001e: ldc.i4.2
	IL_001f: call System.Int32 System.Math::Min(System.Int32,System.Int32)
	IL_0024: stloc.0
	.line 26,26:3,4 'C:\sources\PdbTarget\Program.cs'
	IL_0025: ret
", main);
			});
		}

		[Test]
		public void SequencePointsMultipleDocument ()
		{
			TestPortablePdbModule (module => {
				var type = module.GetType ("PdbTarget.B");
				var main = type.GetMethod (".ctor");

				AssertCode (@"
	.locals ()
	.line 7,7:3,25 'C:\sources\PdbTarget\B.cs'
	IL_0000: ldarg.0
	IL_0001: ldstr """"
	IL_0006: stfld System.String PdbTarget.B::s
	.line 110,110:3,21 'C:\sources\PdbTarget\Program.cs'
	IL_000b: ldarg.0
	IL_000c: ldc.i4.2
	IL_000d: stfld System.Int32 PdbTarget.B::a
	.line 111,111:3,21 'C:\sources\PdbTarget\Program.cs'
	IL_0012: ldarg.0
	IL_0013: ldc.i4.3
	IL_0014: stfld System.Int32 PdbTarget.B::b
	.line 9,9:3,13 'C:\sources\PdbTarget\B.cs'
	IL_0019: ldarg.0
	IL_001a: call System.Void System.Object::.ctor()
	IL_001f: nop
	.line 10,10:3,4 'C:\sources\PdbTarget\B.cs'
	IL_0020: nop
	.line 11,11:4,19 'C:\sources\PdbTarget\B.cs'
	IL_0021: ldstr ""B""
	IL_0026: call System.Void System.Console::WriteLine(System.String)
	IL_002b: nop
	.line 12,12:3,4 'C:\sources\PdbTarget\B.cs'
	IL_002c: ret
", main);
			});
		}

		[Test]
		public void LocalVariables ()
		{
			TestPortablePdbModule (module => {
				var type = module.GetType ("PdbTarget.Program");
				var method = type.GetMethod ("Bar");
				var debug_info = method.DebugInformation;

				Assert.IsNotNull (debug_info.Scope);
				Assert.IsTrue (debug_info.Scope.HasScopes);
				Assert.AreEqual (2, debug_info.Scope.Scopes.Count);

				var scope = debug_info.Scope.Scopes [0];

				Assert.IsNotNull (scope);
				Assert.IsTrue (scope.HasVariables);
				Assert.AreEqual (1, scope.Variables.Count);

				var variable = scope.Variables [0];

				Assert.AreEqual ("s", variable.Name);
				Assert.IsFalse (variable.IsDebuggerHidden);
				Assert.AreEqual (2, variable.Index);

				scope = debug_info.Scope.Scopes [1];

				Assert.IsNotNull (scope);
				Assert.IsTrue (scope.HasVariables);
				Assert.AreEqual (1, scope.Variables.Count);

				variable = scope.Variables [0];

				Assert.AreEqual ("s", variable.Name);
				Assert.IsFalse (variable.IsDebuggerHidden);
				Assert.AreEqual (3, variable.Index);

				Assert.IsTrue (scope.HasScopes);
				Assert.AreEqual (1, scope.Scopes.Count);

				scope = scope.Scopes [0];

				Assert.IsNotNull (scope);
				Assert.IsTrue (scope.HasVariables);
				Assert.AreEqual (1, scope.Variables.Count);

				variable = scope.Variables [0];

				Assert.AreEqual ("u", variable.Name);
				Assert.IsFalse (variable.IsDebuggerHidden);
				Assert.AreEqual (5, variable.Index);
			});
		}

		[Test]
		public void LocalConstants ()
		{
			TestPortablePdbModule (module => {
				var type = module.GetType ("PdbTarget.Program");
				var method = type.GetMethod ("Bar");
				var debug_info = method.DebugInformation;

				Assert.IsNotNull (debug_info.Scope);
				Assert.IsTrue (debug_info.Scope.HasScopes);
				Assert.AreEqual (2, debug_info.Scope.Scopes.Count);

				var scope = debug_info.Scope.Scopes [1];

				Assert.IsNotNull (scope);
				Assert.IsTrue (scope.HasConstants);
				Assert.AreEqual (2, scope.Constants.Count);

				var constant = scope.Constants [0];

				Assert.AreEqual ("b", constant.Name);
				Assert.AreEqual (12, constant.Value);
				Assert.AreEqual (MetadataType.Int32, constant.ConstantType.MetadataType);

				constant = scope.Constants [1];
				Assert.AreEqual ("c", constant.Name);
				Assert.AreEqual ((decimal) 74, constant.Value);
				Assert.AreEqual (MetadataType.ValueType, constant.ConstantType.MetadataType);

				method = type.GetMethod ("Foo");
				debug_info = method.DebugInformation;

				Assert.IsNotNull (debug_info.Scope);
				Assert.IsTrue (debug_info.Scope.HasConstants);
				Assert.AreEqual (4, debug_info.Scope.Constants.Count);

				constant = debug_info.Scope.Constants [0];
				Assert.AreEqual ("s", constant.Name);
				Assert.AreEqual ("const string", constant.Value);
				Assert.AreEqual (MetadataType.String, constant.ConstantType.MetadataType);

				constant = debug_info.Scope.Constants [1];
				Assert.AreEqual ("f", constant.Name);
				Assert.AreEqual (1, constant.Value);
				Assert.AreEqual (MetadataType.Int32, constant.ConstantType.MetadataType);

				constant = debug_info.Scope.Constants [2];
				Assert.AreEqual ("o", constant.Name);
				Assert.AreEqual (null, constant.Value);
				Assert.AreEqual (MetadataType.Object, constant.ConstantType.MetadataType);

				constant = debug_info.Scope.Constants [3];
				Assert.AreEqual ("u", constant.Name);
				Assert.AreEqual (null, constant.Value);
				Assert.AreEqual (MetadataType.String, constant.ConstantType.MetadataType);
			});
		}

		[Test]
		public void ImportScope ()
		{
			TestPortablePdbModule (module => {
				var type = module.GetType ("PdbTarget.Program");
				var method = type.GetMethod ("Bar");
				var debug_info = method.DebugInformation;

				Assert.IsNotNull (debug_info.Scope);

				var import = debug_info.Scope.Import;
				Assert.IsNotNull (import);

				Assert.IsFalse (import.HasTargets);
				Assert.IsNotNull (import.Parent);

				import = import.Parent;

				Assert.IsTrue (import.HasTargets);
				Assert.AreEqual (9, import.Targets.Count);
				var target = import.Targets [0];

				Assert.AreEqual (ImportTargetKind.ImportAlias, target.Kind);
				Assert.AreEqual ("XML", target.Alias);

				target = import.Targets [1];

				Assert.AreEqual (ImportTargetKind.ImportNamespace, target.Kind);
				Assert.AreEqual ("System", target.Namespace);

				target = import.Targets [2];

				Assert.AreEqual (ImportTargetKind.ImportNamespace, target.Kind);
				Assert.AreEqual ("System.Collections.Generic", target.Namespace);

				target = import.Targets [3];

				Assert.AreEqual (ImportTargetKind.ImportNamespace, target.Kind);
				Assert.AreEqual ("System.IO", target.Namespace);

				target = import.Targets [4];

				Assert.AreEqual (ImportTargetKind.ImportNamespace, target.Kind);
				Assert.AreEqual ("System.Threading.Tasks", target.Namespace);

				target = import.Targets [5];

				Assert.AreEqual (ImportTargetKind.ImportNamespaceInAssembly, target.Kind);
				Assert.AreEqual ("System.Xml.Resolvers", target.Namespace);
				Assert.AreEqual ("System.Xml", target.AssemblyReference.Name);


				target = import.Targets [6];

				Assert.AreEqual (ImportTargetKind.ImportType, target.Kind);
				Assert.AreEqual ("System.Console", target.Type.FullName);

				target = import.Targets [7];

				Assert.AreEqual (ImportTargetKind.ImportType, target.Kind);
				Assert.AreEqual ("System.Math", target.Type.FullName);

				target = import.Targets [8];

				Assert.AreEqual (ImportTargetKind.DefineTypeAlias, target.Kind);
				Assert.AreEqual ("Foo", target.Alias);
				Assert.AreEqual ("System.Xml.XmlDocumentType", target.Type.FullName);

				Assert.IsNotNull (import.Parent);

				import = import.Parent;

				Assert.IsTrue (import.HasTargets);
				Assert.AreEqual (1, import.Targets.Count);
				Assert.IsNull (import.Parent);

				target = import.Targets [0];

				Assert.AreEqual (ImportTargetKind.DefineAssemblyAlias, target.Kind);
				Assert.AreEqual ("XML", target.Alias);
				Assert.AreEqual ("System.Xml", target.AssemblyReference.Name);
			});
		}

		[Test]
		public void StateMachineKickOff ()
		{
			TestPortablePdbModule (module => {
				var state_machine = module.GetType ("PdbTarget.Program/<Baz>d__7");
				var main = state_machine.GetMethod ("MoveNext");
				var symbol = main.DebugInformation;

				Assert.IsNotNull (symbol);
				Assert.IsNotNull (symbol.StateMachineKickOffMethod);
				Assert.AreEqual ("System.Threading.Tasks.Task PdbTarget.Program::Baz(System.IO.StreamReader)", symbol.StateMachineKickOffMethod.FullName);
			});
		}

		[Test]
		public void StateMachineCustomDebugInformation ()
		{
			TestPortablePdbModule (module => {
				var state_machine = module.GetType ("PdbTarget.Program/<Baz>d__7");
				var move_next = state_machine.GetMethod ("MoveNext");

				Assert.IsTrue (move_next.HasCustomDebugInformations);

				var state_machine_scope = move_next.CustomDebugInformations.OfType<StateMachineScopeDebugInformation> ().FirstOrDefault ();
				Assert.IsNotNull (state_machine_scope);
				Assert.AreEqual (3, state_machine_scope.Scopes.Count);
				Assert.AreEqual (0, state_machine_scope.Scopes [0].Start.Offset);
				Assert.IsTrue (state_machine_scope.Scopes [0].End.IsEndOfMethod);

				Assert.AreEqual (0, state_machine_scope.Scopes [1].Start.Offset);
				Assert.AreEqual (0, state_machine_scope.Scopes [1].End.Offset);

				Assert.AreEqual (184, state_machine_scope.Scopes [2].Start.Offset);
				Assert.AreEqual (343, state_machine_scope.Scopes [2].End.Offset);

				var async_body = move_next.CustomDebugInformations.OfType<AsyncMethodBodyDebugInformation> ().FirstOrDefault ();
				Assert.IsNotNull (async_body);
				Assert.AreEqual (-1, async_body.CatchHandler.Offset);

				Assert.AreEqual (2, async_body.Yields.Count);
				Assert.AreEqual (61, async_body.Yields [0].Offset);
				Assert.AreEqual (221, async_body.Yields [1].Offset);

				Assert.AreEqual (2, async_body.Resumes.Count);
				Assert.AreEqual (91, async_body.Resumes [0].Offset);
				Assert.AreEqual (252, async_body.Resumes [1].Offset);

				Assert.AreEqual (move_next, async_body.ResumeMethods [0]);
				Assert.AreEqual (move_next, async_body.ResumeMethods [1]);
			});
		}

		[Test]
		public void EmbeddedCompressedPortablePdb ()
		{
			TestModule("EmbeddedCompressedPdbTarget.exe", module => {
				Assert.IsTrue (module.HasDebugHeader);

				var header = module.GetDebugHeader ();

				Assert.IsNotNull (header);
				Assert.AreEqual (2, header.Entries.Length);

				var cv = header.Entries [0];
				Assert.AreEqual (ImageDebugType.CodeView, cv.Directory.Type);

				var eppdb = header.Entries [1];
				Assert.AreEqual (ImageDebugType.EmbeddedPortablePdb, eppdb.Directory.Type);
				Assert.AreEqual (0x0100, eppdb.Directory.MajorVersion);
				Assert.AreEqual (0x0100, eppdb.Directory.MinorVersion);
			}, symbolReaderProvider: typeof (EmbeddedPortablePdbReaderProvider), symbolWriterProvider: typeof (EmbeddedPortablePdbWriterProvider));
		}

		void TestPortablePdbModule (Action<ModuleDefinition> test)
		{
			TestModule ("PdbTarget.exe", test, symbolReaderProvider: typeof (PortablePdbReaderProvider), symbolWriterProvider: typeof (PortablePdbWriterProvider));
			TestModule ("EmbeddedPdbTarget.exe", test, verify: !Platform.OnMono);
			TestModule ("EmbeddedCompressedPdbTarget.exe", test, symbolReaderProvider: typeof(EmbeddedPortablePdbReaderProvider), symbolWriterProvider: typeof(EmbeddedPortablePdbWriterProvider));
		}

		[Test]
		public void RoundTripCecilPortablePdb ()
		{
			TestModule ("cecil.dll", module => {
				Assert.IsTrue (module.HasSymbols);
			}, symbolReaderProvider: typeof (PortablePdbReaderProvider), symbolWriterProvider: typeof (PortablePdbWriterProvider));
		}

		[Test]
		public void RoundTripLargePortablePdb ()
		{
			TestModule ("Mono.Android.dll", module => {
				Assert.IsTrue (module.HasSymbols);
			}, verify: false, symbolReaderProvider: typeof (PortablePdbReaderProvider), symbolWriterProvider: typeof (PortablePdbWriterProvider));
		}

		[Test]
		public void EmptyPortablePdb ()
		{
			TestModule ("EmptyPdb.dll", module => {
				Assert.IsTrue (module.HasSymbols);
			}, symbolReaderProvider: typeof (PortablePdbReaderProvider), symbolWriterProvider: typeof (PortablePdbWriterProvider));
		}

		[Test]
		public void NullClassConstant ()
		{
			TestModule ("xattr.dll", module => {
				var type = module.GetType ("Library");
				var method = type.GetMethod ("NullXAttributeConstant");
				var symbol = method.DebugInformation;

				Assert.IsNotNull (symbol);
				Assert.AreEqual (1, symbol.Scope.Constants.Count);

				var a = symbol.Scope.Constants [0];
				Assert.AreEqual ("a", a.Name);
			}, symbolReaderProvider: typeof (PortablePdbReaderProvider), symbolWriterProvider: typeof (PortablePdbWriterProvider));
		}

		[Test]
		public void InvalidConstantRecord ()
		{
			using (var module = GetResourceModule ("mylib.dll", new ReaderParameters { SymbolReaderProvider = new PortablePdbReaderProvider () })) {
				var type = module.GetType ("mylib.Say");
				var method = type.GetMethod ("hello");
				var symbol = method.DebugInformation;

				Assert.IsNotNull (symbol);
				Assert.AreEqual (0, symbol.Scope.Constants.Count);
			}
		}

		[Test]
		public void SourceLink ()
		{
			TestModule ("TargetLib.dll", module => {
				Assert.IsTrue (module.HasCustomDebugInformations);
				Assert.AreEqual (1, module.CustomDebugInformations.Count);

				var source_link = module.CustomDebugInformations [0] as SourceLinkDebugInformation;
				Assert.IsNotNull (source_link);
				Assert.AreEqual ("{\"documents\":{\"C:\\\\tmp\\\\SourceLinkProblem\\\\*\":\"https://raw.githubusercontent.com/bording/SourceLinkProblem/197d965ee7f1e7f8bd3cea55b5f904aeeb8cd51e/*\"}}", source_link.Content);
			}, symbolReaderProvider: typeof (PortablePdbReaderProvider), symbolWriterProvider: typeof (PortablePdbWriterProvider));
		}

		[Test]
		public void EmbeddedSource ()
		{
			TestModule ("embedcs.exe", module => {
				var program = GetDocument (module.GetType ("Program"));
				var program_src = GetSourceDebugInfo (program);
				Assert.IsTrue (program_src.compress);
				var program_src_content = Encoding.UTF8.GetString (program_src.Content);
				Assert.AreEqual (Normalize (@"using System;

class Program
{
    static void Main()
    {
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        // Hello hello hello hello hello hello
        Console.WriteLine(B.Do());
        Console.WriteLine(A.Do());
    }
}
"), Normalize (program_src_content));

				var a = GetDocument (module.GetType ("A"));
				var a_src = GetSourceDebugInfo (a);
				Assert.IsFalse (a_src.compress);
				var a_src_content = Encoding.UTF8.GetString (a_src.Content);
				Assert.AreEqual (Normalize (@"class A
{
    public static string Do()
    {
        return ""A::Do"";
    }
}"), Normalize (a_src_content));

				var b = GetDocument(module.GetType ("B"));
				var b_src = GetSourceDebugInfo (b);
				Assert.IsFalse (b_src.compress);
				var b_src_content = Encoding.UTF8.GetString (b_src.Content);
				Assert.AreEqual (Normalize (@"class B
{
    public static string Do()
    {
        return ""B::Do"";
    }
}"), Normalize (b_src_content));
			}, symbolReaderProvider: typeof (PortablePdbReaderProvider), symbolWriterProvider: typeof (PortablePdbWriterProvider));
		}

		static Document GetDocument (TypeDefinition type)
		{
			foreach (var method in type.Methods) {
				if (!method.HasBody)
					continue;

				foreach (var instruction in method.Body.Instructions) {
					var sp = method.DebugInformation.GetSequencePoint (instruction);
					if (sp != null && sp.Document != null)
						return sp.Document;
				}
			}

			return null;
		}

		static EmbeddedSourceDebugInformation GetSourceDebugInfo (Document document)
		{
			Assert.IsTrue (document.HasCustomDebugInformations);
			Assert.AreEqual (1, document.CustomDebugInformations.Count);

			var source = document.CustomDebugInformations [0] as EmbeddedSourceDebugInformation;
			Assert.IsNotNull (source);
			return source;
		}

		[Test]
		public void PortablePdbLineInfo  ()
		{
			TestModule ("line.exe", module => {
				var type = module.GetType ("Tests");
				var main = type.GetMethod ("Main");

				AssertCode (@"
	.locals ()
	.line 4,4:42,43 '/foo/bar.cs'
	IL_0000: nop
	.line 5,5:2,3 '/foo/bar.cs'
	IL_0001: ret", main);
			}, symbolReaderProvider: typeof (PortablePdbReaderProvider), symbolWriterProvider: typeof (PortablePdbWriterProvider));
		}

		public sealed class SymbolWriterProvider : ISymbolWriterProvider {

			readonly DefaultSymbolWriterProvider writer_provider = new DefaultSymbolWriterProvider ();

			public ISymbolWriter GetSymbolWriter (ModuleDefinition module, string fileName)
			{
				return new SymbolWriter (writer_provider.GetSymbolWriter (module, fileName));
			}

			public ISymbolWriter GetSymbolWriter (ModuleDefinition module, Stream symbolStream)
			{
				return new SymbolWriter (writer_provider.GetSymbolWriter (module, symbolStream));
			}
		}

		public sealed class SymbolWriter : ISymbolWriter {

			readonly ISymbolWriter symbol_writer;

			public SymbolWriter (ISymbolWriter symbolWriter)
			{
				this.symbol_writer = symbolWriter;
			}

			public ImageDebugHeader GetDebugHeader ()
			{
				var header = symbol_writer.GetDebugHeader ();
				if (!header.HasEntries)
					return header;

				for (int i = 0; i < header.Entries.Length; i++) {
					header.Entries [i] = ProcessEntry (header.Entries [i]);
				}

				return header;
			}

			private static ImageDebugHeaderEntry ProcessEntry (ImageDebugHeaderEntry entry)
			{
				if (entry.Directory.Type != ImageDebugType.CodeView)
					return entry;

				var reader = new ByteBuffer (entry.Data);
				var writer = new ByteBuffer ();

				var sig = reader.ReadUInt32 ();
				if (sig != 0x53445352)
					return entry;

				writer.WriteUInt32 (sig); // RSDS
				writer.WriteBytes (reader.ReadBytes (16)); // MVID
				writer.WriteUInt32 (reader.ReadUInt32 ()); // Age

				var length = Array.IndexOf (entry.Data, (byte) 0, reader.position) - reader.position;

				var fullPath = Encoding.UTF8.GetString (reader.ReadBytes (length));

				writer.WriteBytes (Encoding.UTF8.GetBytes (Path.GetFileName (fullPath)));
				writer.WriteByte (0);

				var newData = new byte [writer.length];
				Buffer.BlockCopy (writer.buffer, 0, newData, 0, writer.length);

				var directory = entry.Directory;
				directory.SizeOfData = newData.Length;

				return new ImageDebugHeaderEntry (directory, newData);
			}

			public ISymbolReaderProvider GetReaderProvider ()
			{
				return symbol_writer.GetReaderProvider ();
			}

			public void Write (MethodDebugInformation info)
			{
				symbol_writer.Write (info);
			}

			public void Dispose ()
			{
				symbol_writer.Dispose ();
			}
		}

		static string GetDebugHeaderPdbPath (ModuleDefinition module)
		{
			var header = module.GetDebugHeader ();
			var cv = Mixin.GetCodeViewEntry (header);
			Assert.IsNotNull (cv);
			var length = Array.IndexOf (cv.Data, (byte)0, 24) - 24;
			var bytes = new byte [length];
			Buffer.BlockCopy (cv.Data, 24, bytes, 0, length);
			return Encoding.UTF8.GetString (bytes);
		}

		[Test]
		public void UseCustomSymbolWriterToChangeDebugHeaderPdbPath ()
		{
			const string resource = "mylib.dll";

			string debug_header_pdb_path;
			string dest = Path.Combine (Path.GetTempPath (), resource);

			using (var module = GetResourceModule (resource, new ReaderParameters { SymbolReaderProvider = new PortablePdbReaderProvider () })) {
				debug_header_pdb_path = GetDebugHeaderPdbPath (module);
				Assert.IsTrue (Path.IsPathRooted (debug_header_pdb_path));
				module.Write (dest, new WriterParameters { SymbolWriterProvider = new SymbolWriterProvider () });
			}

			using (var module = ModuleDefinition.ReadModule (dest, new ReaderParameters { SymbolReaderProvider = new PortablePdbReaderProvider () })) {
				var pdb_path = GetDebugHeaderPdbPath (module);
				Assert.IsFalse (Path.IsPathRooted (pdb_path));
				Assert.AreEqual (Path.GetFileName (debug_header_pdb_path), pdb_path);
			}
		}

		[Test]
		public void WriteAndReadAgainModuleWithDeterministicMvid ()
		{
			const string resource = "mylib.dll";
			string destination = Path.GetTempFileName ();

			using (var module = GetResourceModule (resource, new ReaderParameters { SymbolReaderProvider = new PortablePdbReaderProvider () })) {
				module.Write (destination, new WriterParameters { DeterministicMvid = true, SymbolWriterProvider = new SymbolWriterProvider () });
			}

			using (var module = ModuleDefinition.ReadModule (destination, new ReaderParameters { SymbolReaderProvider = new PortablePdbReaderProvider () })) {
			}
		}
	}
}
