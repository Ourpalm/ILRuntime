using System;
using System.IO;

using NUnit.Framework;

using Mono.Cecil.Cil;
using Mono.Cecil.Mdb;
using Mono.Cecil.Pdb;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class SymbolTests : BaseTestFixture {

		[Test]
		public void DefaultPdb ()
		{
			TestModule ("libpdb.dll", module => {
				Assert.IsTrue (module.HasSymbols);
				Assert.AreEqual (typeof (NativePdbReader), module.SymbolReader.GetType ());
			}, readOnly: !Platform.HasNativePdbSupport, symbolReaderProvider: typeof (DefaultSymbolReaderProvider), symbolWriterProvider: typeof (DefaultSymbolWriterProvider));
		}

		[Test]
		public void DefaultMdb ()
		{
			TestModule ("libmdb.dll", module => {
				Assert.IsTrue (module.HasSymbols);
				Assert.AreEqual (typeof (MdbReader), module.SymbolReader.GetType ());
			}, symbolReaderProvider: typeof (DefaultSymbolReaderProvider), symbolWriterProvider: typeof (DefaultSymbolWriterProvider));
		}

		[Test]
		public void DefaultPortablePdb ()
		{
			TestModule ("PdbTarget.exe", module => {
				Assert.IsTrue (module.HasSymbols);
				Assert.AreEqual (typeof (PortablePdbReader), module.SymbolReader.GetType ());
			}, symbolReaderProvider: typeof (DefaultSymbolReaderProvider), symbolWriterProvider: typeof (DefaultSymbolWriterProvider));
		}

		[Test]
		public void DefaultEmbeddedPortablePdb ()
		{
			TestModule ("EmbeddedPdbTarget.exe", module => {
				Assert.IsTrue (module.HasSymbols);
				Assert.AreEqual (typeof (PortablePdbReader), module.SymbolReader.GetType ());
			}, symbolReaderProvider: typeof (DefaultSymbolReaderProvider), symbolWriterProvider: typeof (DefaultSymbolWriterProvider), verify: !Platform.OnMono);
		}

		[Test]
		public void MdbMismatch ()
		{
			Assert.Throws<SymbolsNotMatchingException> (() => GetResourceModule ("mdb-mismatch.dll", new ReaderParameters { SymbolReaderProvider = new MdbReaderProvider () }));
		}

		[Test]
		public void MdbIgnoreMismatch()
		{
			using (var module = GetResourceModule ("mdb-mismatch.dll", new ReaderParameters { SymbolReaderProvider = new MdbReaderProvider (), ThrowIfSymbolsAreNotMatching = false })) {
				Assert.IsNull (module.SymbolReader);
				Assert.IsFalse (module.HasSymbols);
			}
		}

		[Test]
		public void PortablePdbMismatch ()
		{
			Assert.Throws<SymbolsNotMatchingException> (() => GetResourceModule ("pdb-mismatch.dll", new ReaderParameters { SymbolReaderProvider = new PortablePdbReaderProvider () }));
		}

		[Test]
		public void PortablePdbIgnoreMismatch()
		{
			using (var module = GetResourceModule ("pdb-mismatch.dll", new ReaderParameters { SymbolReaderProvider = new PortablePdbReaderProvider (), ThrowIfSymbolsAreNotMatching = false })) {
				Assert.IsNull (module.SymbolReader);
				Assert.IsFalse (module.HasSymbols);
			}
		}

		[Test]
		public void DefaultPortablePdbStream ()
		{
			using (var symbolStream = GetResourceStream ("PdbTarget.pdb")) {
				var parameters = new ReaderParameters {
					SymbolReaderProvider = new PortablePdbReaderProvider (),
					SymbolStream = symbolStream,
				};

				using (var module = GetResourceModule ("PdbTarget.exe", parameters)) {
					Assert.IsNotNull (module.SymbolReader);
					Assert.IsTrue (module.HasSymbols);
					Assert.AreEqual (typeof (PortablePdbReader), module.SymbolReader.GetType ());
				}
			}
		}

		[Test]
		public void DefaultPdbStream ()
		{
			using (var symbolStream = GetResourceStream ("libpdb.pdb")) {
				var parameters = new ReaderParameters {
					SymbolReaderProvider = new NativePdbReaderProvider (),
					SymbolStream = symbolStream,
				};

				using (var module = GetResourceModule ("libpdb.dll", parameters)) {
					Assert.IsNotNull (module.SymbolReader);
					Assert.IsTrue (module.HasSymbols);
					Assert.AreEqual (typeof (NativePdbReader), module.SymbolReader.GetType ());
				}
			}
		}

		[Test]
		public void DefaultMdbStream ()
		{
			using (var symbolStream = GetResourceStream ("libmdb.dll.mdb")) {
				var parameters = new ReaderParameters {
					SymbolReaderProvider = new MdbReaderProvider (),
					SymbolStream = symbolStream,
				};

				using (var module = GetResourceModule ("libmdb.dll", parameters)) {
					Assert.IsNotNull (module.SymbolReader);
					Assert.IsTrue (module.HasSymbols);
					Assert.AreEqual (typeof (MdbReader), module.SymbolReader.GetType ());
				}
			}
		}
	}
}
