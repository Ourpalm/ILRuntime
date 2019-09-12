using System.IO;
using System.Linq;

using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;

using NUnit.Framework;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class PdbTests : BaseTestFixture {

		[Test]
		public void Main ()
		{
			TestModule ("test.exe", module => {
				var type = module.GetType ("Program");
				var main = type.GetMethod ("Main");

				AssertCode (@"
	.locals init (System.Int32 i, System.Int32 CS$1$0000, System.Boolean CS$4$0001)
	.line 6,6:2,3 'c:\sources\cecil\symbols\Mono.Cecil.Pdb\Test\Resources\assemblies\test.cs'
	IL_0000: nop
	.line 7,7:8,18 'c:\sources\cecil\symbols\Mono.Cecil.Pdb\Test\Resources\assemblies\test.cs'
	IL_0001: ldc.i4.0
	IL_0002: stloc.0
	.line hidden 'c:\sources\cecil\symbols\Mono.Cecil.Pdb\Test\Resources\assemblies\test.cs'
	IL_0003: br.s IL_0012
	.line 8,8:4,21 'c:\sources\cecil\symbols\Mono.Cecil.Pdb\Test\Resources\assemblies\test.cs'
	IL_0005: ldarg.0
	IL_0006: ldloc.0
	IL_0007: ldelem.ref
	IL_0008: call System.Void Program::Print(System.String)
	IL_000d: nop
	.line 7,7:36,39 'c:\sources\cecil\symbols\Mono.Cecil.Pdb\Test\Resources\assemblies\test.cs'
	IL_000e: ldloc.0
	IL_000f: ldc.i4.1
	IL_0010: add
	IL_0011: stloc.0
	.line 7,7:19,34 'c:\sources\cecil\symbols\Mono.Cecil.Pdb\Test\Resources\assemblies\test.cs'
	IL_0012: ldloc.0
	IL_0013: ldarg.0
	IL_0014: ldlen
	IL_0015: conv.i4
	IL_0016: clt
	IL_0018: stloc.2
	.line hidden 'c:\sources\cecil\symbols\Mono.Cecil.Pdb\Test\Resources\assemblies\test.cs'
	IL_0019: ldloc.2
	IL_001a: brtrue.s IL_0005
	.line 10,10:3,12 'c:\sources\cecil\symbols\Mono.Cecil.Pdb\Test\Resources\assemblies\test.cs'
	IL_001c: ldc.i4.0
	IL_001d: stloc.1
	IL_001e: br.s IL_0020
	.line 11,11:2,3 'c:\sources\cecil\symbols\Mono.Cecil.Pdb\Test\Resources\assemblies\test.cs'
	IL_0020: ldloc.1
	IL_0021: ret
", main);
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof(PdbReaderProvider), symbolWriterProvider: typeof(PdbWriterProvider));
		}

		[Test]
		public void DebuggerHiddenVariable ()
		{
			TestModule ("test.exe", module => {
				var type = module.GetType ("Program");
				var method = type.GetMethod ("Main");

				var scope = method.DebugInformation.Scope;

				Assert.IsTrue (scope.HasVariables);
				var variables = scope.Variables;

				Assert.AreEqual ("CS$1$0000", variables [0].Name);
				Assert.IsTrue (variables [0].IsDebuggerHidden);
				Assert.AreEqual ("CS$4$0001", variables [1].Name);
				Assert.IsTrue (variables [1].IsDebuggerHidden);

				Assert.AreEqual (1, scope.Scopes.Count);
				scope = scope.Scopes [0];
				variables = scope.Variables;

				Assert.AreEqual ("i", variables [0].Name);
				Assert.IsFalse (variables [0].IsDebuggerHidden);
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof(PdbReaderProvider), symbolWriterProvider: typeof(PdbWriterProvider));
		}

		[Test]
		public void Document ()
		{
			TestModule ("test.exe", module => {
				var type = module.GetType ("Program");
				var method = type.GetMethod ("Main");

				var sequence_point = method.DebugInformation.SequencePoints.First (sp => sp != null);
				var document = sequence_point.Document;

				Assert.IsNotNull (document);

				Assert.AreEqual (@"c:\sources\cecil\symbols\Mono.Cecil.Pdb\Test\Resources\assemblies\test.cs", document.Url);
				Assert.AreEqual (DocumentType.Text, document.Type);
				Assert.AreEqual (DocumentHashAlgorithm.None, document.HashAlgorithm);
				Assert.AreEqual (DocumentLanguage.CSharp, document.Language);
				Assert.AreEqual (DocumentLanguageVendor.Microsoft, document.LanguageVendor);
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof(PdbReaderProvider), symbolWriterProvider: typeof(PdbWriterProvider));
		}

		[Test]
		public void BasicDocument ()
		{
			TestModule ("VBConsApp.exe", module => {
				var type = module.GetType ("VBConsApp.Program");
				var method = type.GetMethod ("Main");

				var sequence_point = method.DebugInformation.SequencePoints.First (sp => sp != null);
				var document = sequence_point.Document;

				Assert.IsNotNull (document);

				Assert.AreEqual (@"c:\tmp\VBConsApp\Program.vb", document.Url);
				Assert.AreEqual (DocumentType.Text, document.Type);
				Assert.AreEqual (DocumentHashAlgorithm.None, document.HashAlgorithm);
				Assert.AreEqual (DocumentLanguage.Basic, document.Language);
				Assert.AreEqual (DocumentLanguageVendor.Microsoft, document.LanguageVendor);
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof(PdbReaderProvider), symbolWriterProvider: typeof(PdbWriterProvider));
		}

		[Test]
		public void FSharpDocument ()
		{
			TestModule ("fsapp.exe", module => {
				var type = module.GetType ("Program");
				var method = type.GetMethod ("fact");

				var sequence_point = method.DebugInformation.SequencePoints.First (sp => sp != null);
				var document = sequence_point.Document;

				Assert.IsNotNull (document);

				Assert.AreEqual (@"c:\tmp\fsapp\Program.fs", document.Url);
				Assert.AreEqual (DocumentType.Text, document.Type);
				Assert.AreEqual (DocumentHashAlgorithm.None, document.HashAlgorithm);
				Assert.AreEqual (DocumentLanguage.FSharp, document.Language);
				Assert.AreEqual (DocumentLanguageVendor.Microsoft, document.LanguageVendor);
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof (PdbReaderProvider), symbolWriterProvider: typeof (PdbWriterProvider));
		}

		[Test]
		public void EmptyEnumerable ()
		{
			TestModule ("empty-iterator.dll", module => {
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof (PdbReaderProvider), symbolWriterProvider: typeof (PdbWriterProvider));
		}

		[Test]
		public void EmptyRootNamespace ()
		{
			TestModule ("EmptyRootNamespace.dll", module => {
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof (PdbReaderProvider), symbolWriterProvider: typeof (PdbWriterProvider));
		}

		[Test]
		public void VisualBasicNamespace ()
		{
			TestModule ("AVbTest.exe", module => {
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof (PdbReaderProvider), symbolWriterProvider: typeof (PdbWriterProvider));

		}

		[Test]
		public void LocalVariables ()
		{
			TestModule ("ComplexPdb.dll", module => {
				var type = module.GetType ("ComplexPdb.Program");
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
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof (PdbReaderProvider), symbolWriterProvider: typeof (PdbWriterProvider));
		}

		[Test]
		public void LocalConstants ()
		{
			TestModule ("ComplexPdb.dll", module => {
				var type = module.GetType ("ComplexPdb.Program");
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
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof (PdbReaderProvider), symbolWriterProvider: typeof (PdbWriterProvider));
		}

		[Test]
		public void ImportScope ()
		{
			TestModule ("ComplexPdb.dll", module => {
				var type = module.GetType ("ComplexPdb.Program");
				var method = type.GetMethod ("Bar");
				var debug_info = method.DebugInformation;

				Assert.IsNotNull (debug_info.Scope);

				var import = debug_info.Scope.Import;
				Assert.IsNotNull (import);

				Assert.IsTrue (import.HasTargets);
				Assert.AreEqual (6, import.Targets.Count);
				var target = import.Targets [0];

				Assert.AreEqual (ImportTargetKind.ImportNamespace, target.Kind);
				Assert.AreEqual ("System", target.Namespace);

				target = import.Targets [1];

				Assert.AreEqual (ImportTargetKind.ImportNamespace, target.Kind);
				Assert.AreEqual ("System.Collections.Generic", target.Namespace);

				target = import.Targets [2];

				Assert.AreEqual (ImportTargetKind.ImportNamespace, target.Kind);
				Assert.AreEqual ("System.Threading.Tasks", target.Namespace);

				target = import.Targets [3];

				Assert.AreEqual (ImportTargetKind.ImportType, target.Kind);
				Assert.AreEqual ("System.Console", target.Type.FullName);

				target = import.Targets [4];

				Assert.AreEqual (ImportTargetKind.DefineTypeAlias, target.Kind);
				Assert.AreEqual ("Foo1", target.Alias);
				Assert.AreEqual ("System.Console", target.Type.FullName);

				target = import.Targets [5];

				Assert.AreEqual (ImportTargetKind.DefineNamespaceAlias, target.Kind);
				Assert.AreEqual ("Foo2", target.Alias);
				Assert.AreEqual ("System.Reflection", target.Namespace);
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof (PdbReaderProvider), symbolWriterProvider: typeof (PdbWriterProvider));
		}

		[Test]
		public void StateMachineKickOff ()
		{
			TestModule ("ComplexPdb.dll", module => {
				var state_machine = module.GetType ("ComplexPdb.Program/<TestAsync>d__2");
				var move_next = state_machine.GetMethod ("MoveNext");
				var symbol = move_next.DebugInformation;

				Assert.IsNotNull (symbol);
				Assert.IsNotNull (symbol.StateMachineKickOffMethod);
				Assert.AreEqual ("System.Threading.Tasks.Task ComplexPdb.Program::TestAsync()", symbol.StateMachineKickOffMethod.FullName);
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof (PdbReaderProvider), symbolWriterProvider: typeof (PdbWriterProvider));
		}

		[Test]
		public void Iterators ()
		{
			TestModule ("ComplexPdb.dll", module => {
				var state_machine = module.GetType ("ComplexPdb.Program/<TestAsync>d__2");
				var move_next = state_machine.GetMethod ("MoveNext");

				Assert.IsTrue (move_next.DebugInformation.HasCustomDebugInformations);
				Assert.AreEqual (2, move_next.DebugInformation.CustomDebugInformations.Count);

				var state_machine_scope = move_next.DebugInformation.CustomDebugInformations [0] as StateMachineScopeDebugInformation;
				Assert.IsNotNull (state_machine_scope);
				Assert.AreEqual (1, state_machine_scope.Scopes.Count);
				Assert.AreEqual (142, state_machine_scope.Scopes [0].Start.Offset);
				Assert.AreEqual (319, state_machine_scope.Scopes [0].End.Offset);

				var async_body = move_next.DebugInformation.CustomDebugInformations [1] as AsyncMethodBodyDebugInformation;
				Assert.IsNotNull (async_body);
				Assert.AreEqual (-1, async_body.CatchHandler.Offset);

				Assert.AreEqual (2, async_body.Yields.Count);
				Assert.AreEqual (68, async_body.Yields [0].Offset);
				Assert.AreEqual (197, async_body.Yields [1].Offset);

				Assert.AreEqual (2, async_body.Resumes.Count);
				Assert.AreEqual (98, async_body.Resumes [0].Offset);
				Assert.AreEqual (227, async_body.Resumes [1].Offset);

				Assert.AreEqual (move_next, async_body.ResumeMethods [0]);
				Assert.AreEqual (move_next, async_body.ResumeMethods [1]);
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof (PdbReaderProvider), symbolWriterProvider: typeof (PdbWriterProvider));
		}

		[Test]
		public void ImportsForFirstMethod ()
		{
			TestModule ("CecilTest.exe", module => {
				var type = module.GetType ("CecilTest.Program");
				var method = type.GetMethod ("Main");

				var debug = method.DebugInformation;
				var scope = debug.Scope;

				Assert.IsTrue (scope.End.IsEndOfMethod);

				var import = scope.Import;

				Assert.IsNotNull (import);
				Assert.AreEqual (5, import.Targets.Count);

				var ns = new [] {
					"System",
					"System.Collections.Generic",
					"System.Linq",
					"System.Text",
					"System.Threading.Tasks",
				};

				for (int i = 0; i < import.Targets.Count; i++) {
					var target = import.Targets [i];

					Assert.AreEqual (ImportTargetKind.ImportNamespace, target.Kind);
					Assert.AreEqual (ns [i], target.Namespace);
				}

				Assert.AreEqual ("System", import.Targets [0].Namespace);
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof (PdbReaderProvider), symbolWriterProvider: typeof (PdbWriterProvider));
		}

		[Test]
		public void CreateMethodFromScratch ()
		{
			if (!Platform.HasNativePdbSupport)
				Assert.Ignore ();

			var module = ModuleDefinition.CreateModule ("Pan", ModuleKind.Dll);
			var type = new TypeDefinition ("Pin", "Pon", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed, module.ImportReference (typeof (object)));
			module.Types.Add (type);

			var method = new MethodDefinition ("Pang", MethodAttributes.Public | MethodAttributes.Static, module.ImportReference (typeof (string)));
			type.Methods.Add (method);

			var body = method.Body;

			body.InitLocals = true;

			var il = body.GetILProcessor ();
			var temp = new VariableDefinition (module.ImportReference (typeof (string)));
			body.Variables.Add (temp);

			il.Emit (OpCodes.Nop);
			il.Emit (OpCodes.Ldstr, "hello");
			il.Emit (OpCodes.Stloc, temp);
			il.Emit (OpCodes.Ldloc, temp);
			il.Emit (OpCodes.Ret);

			var sequence_point = new SequencePoint (body.Instructions [0], new Document (@"C:\test.cs")) {
				StartLine = 0,
				StartColumn = 0,
				EndLine = 0,
				EndColumn = 4,
			};

			method.DebugInformation.SequencePoints.Add (sequence_point);

			method.DebugInformation.Scope = new ScopeDebugInformation  (body.Instructions [0], null) {
				Variables = { new VariableDebugInformation (temp, "temp") }
			};

			var file = Path.Combine (Path.GetTempPath (), "Pan.dll");
			module.Write (file, new WriterParameters {
				SymbolWriterProvider = new PdbWriterProvider (),
			});

			module = ModuleDefinition.ReadModule (file, new ReaderParameters {
				SymbolReaderProvider = new PdbReaderProvider (),
			});

			method = module.GetType ("Pin.Pon").GetMethod ("Pang");

			Assert.AreEqual ("temp", method.DebugInformation.Scope.Variables [0].Name);
		}
	}
}
