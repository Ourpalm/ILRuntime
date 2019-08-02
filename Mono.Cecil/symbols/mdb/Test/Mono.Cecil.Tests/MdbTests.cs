using Mono.Cecil.Mdb;

using NUnit.Framework;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class MdbTests : BaseTestFixture {

		[Test]
		public void MdbWithJustLineInfo ()
		{
			TestModule ("simplemdb.exe", module => {
				var type = module.GetType ("Program");
				var main = type.GetMethod ("Main");

				AssertCode (@"
	.locals init (System.Int32 i)
	.line 6,-1:-1,-1 'C:\sources\cecil\symbols\Mono.Cecil.Mdb\Test\Resources\assemblies\hello.cs'
	IL_0000: ldc.i4.0
	IL_0001: stloc.0
	.line 7,-1:-1,-1 'C:\sources\cecil\symbols\Mono.Cecil.Mdb\Test\Resources\assemblies\hello.cs'
	IL_0002: br IL_0013
	.line 8,-1:-1,-1 'C:\sources\cecil\symbols\Mono.Cecil.Mdb\Test\Resources\assemblies\hello.cs'
	IL_0007: ldarg.0
	IL_0008: ldloc.0
	IL_0009: ldelem.ref
	IL_000a: call System.Void Program::Print(System.String)
	.line 7,-1:-1,-1 'C:\sources\cecil\symbols\Mono.Cecil.Mdb\Test\Resources\assemblies\hello.cs'
	IL_000f: ldloc.0
	IL_0010: ldc.i4.1
	IL_0011: add
	IL_0012: stloc.0
	IL_0013: ldloc.0
	IL_0014: ldarg.0
	IL_0015: ldlen
	IL_0016: conv.i4
	IL_0017: blt IL_0007
	.line 10,-1:-1,-1 'C:\sources\cecil\symbols\Mono.Cecil.Mdb\Test\Resources\assemblies\hello.cs'
	IL_001c: ldc.i4.0
	IL_001d: ret
", main);
			}, symbolReaderProvider: typeof(MdbReaderProvider), symbolWriterProvider: typeof(MdbWriterProvider));
		}

		[Test]
		public void RoundTripCoreLib ()
		{
			TestModule ("mscorlib.dll", module => {
				var type = module.GetType ("System.IO.__Error");
				var method = type.GetMethod ("WinIOError");

				Assert.IsNotNull (method.Body);
			}, verify: !Platform.OnMono, symbolReaderProvider: typeof(MdbReaderProvider), symbolWriterProvider: typeof(MdbWriterProvider));
		}

		[Test]
		public void PartialClass ()
		{
			TestModule ("BreakpointTest.Portable.dll", module => {
				var type = module.GetType ("BreakpointTest.Portable.TestService/<MyAsyncAction1>c__async3");
				var method = type.GetMethod ("MoveNext");

				Assert.IsNotNull (method);

				var info = method.DebugInformation;
				Assert.AreEqual (5, info.SequencePoints.Count);
				foreach (var sp in info.SequencePoints)
					Assert.AreEqual(@"C:\tmp\repropartial\BreakpointTest.Portable\TestService.Actions.cs", sp.Document.Url);

				type = module.GetType("BreakpointTest.Portable.TestService/<MyAsyncAction2>c__async2");
				method = type.GetMethod("MoveNext");

				Assert.IsNotNull(method);

				info = method.DebugInformation;
				Assert.AreEqual(5, info.SequencePoints.Count);
				foreach (var sp in info.SequencePoints)
					Assert.AreEqual(@"C:\tmp\repropartial\BreakpointTest.Portable\TestService.cs", sp.Document.Url);

			}, symbolReaderProvider: typeof(MdbReaderProvider), symbolWriterProvider: typeof(MdbWriterProvider));
		}
	}
}
