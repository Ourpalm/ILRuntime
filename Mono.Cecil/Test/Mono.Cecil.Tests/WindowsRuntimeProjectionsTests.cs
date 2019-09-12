#if !NET_CORE

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mono.Cecil.Tests {
	[TestFixture]
	public abstract class BaseWindowsRuntimeProjectionsTests : BaseTestFixture {
		protected abstract string ModuleName { get; }
		protected abstract MetadataKind ExpectedMetadataKind { get; }
		protected abstract string [] ManagedClassTypeNames { get; }
		protected abstract string [] CustomListTypeNames { get; }

		[Test]
		public void CanReadMetadataType ()
		{
			if (Platform.OnMono)
				return;

			TestModule (ModuleName, (module) => {
				Assert.AreEqual (ExpectedMetadataKind, module.MetadataKind);
			}, verify: false, assemblyResolver: WindowsRuntimeAssemblyResolver.CreateInstance (), applyWindowsRuntimeProjections: true);
		}

		[Test]
		public void CanProjectParametersAndReturnTypes ()
		{
			if (Platform.OnMono)
				return;

			TestModule (ModuleName, (module) => {
				var types = ManagedClassTypeNames.Select (typeName => module.Types.Single (t => t.Name == typeName));

				foreach (var type in types) {
					var listGetter = type.Properties.Single (p => p.Name == "List").GetMethod;
					var listSetter = type.Properties.Single (p => p.Name == "List").SetMethod;

					Assert.IsNotNull (listGetter);
					Assert.IsNotNull (listSetter);

					Assert.AreEqual (listGetter.ReturnType.FullName, "System.Collections.Generic.IList`1<System.Int32>");
					Assert.AreEqual (listSetter.Parameters.Count, 1);
					Assert.AreEqual (listSetter.Parameters [0].ParameterType.FullName, "System.Collections.Generic.IList`1<System.Int32>");
				}
			}, verify: false, assemblyResolver: WindowsRuntimeAssemblyResolver.CreateInstance (), applyWindowsRuntimeProjections: true);
		}

		[Test]
		public void CanProjectInterfaces ()
		{
			if (Platform.OnMono)
				return;

			TestModule (ModuleName, (module) => {
				var types = CustomListTypeNames.Select (typeName => module.Types.Single (t => t.Name == typeName));

				foreach (var type in types) {
					Assert.IsNotNull (type.Interfaces.SingleOrDefault (i => i.InterfaceType.FullName == "System.Collections.Generic.IList`1<System.Int32>"));
					Assert.IsNotNull (type.Interfaces.SingleOrDefault (i => i.InterfaceType.FullName == "System.Collections.Generic.IEnumerable`1<System.Int32>"));
				}
			}, verify: false, assemblyResolver: WindowsRuntimeAssemblyResolver.CreateInstance (), applyWindowsRuntimeProjections: true);
		}

		[Test]
		public void CanStripType ()
		{
			if (Platform.OnMono)
				return;

			var assemblyResolver = WindowsRuntimeAssemblyResolver.CreateInstance ();

			TestModule (ModuleName, (originalModule) => {
				var types = CustomListTypeNames.Select (typeName => originalModule.Types.Single (t => t.Name == typeName)).ToArray ();

				foreach (var type in types)
					originalModule.Types.Remove (type);

				var tmpPath = Path.GetTempFileName ();
				originalModule.Write (tmpPath);

				try {
					TestModule (tmpPath, (modifiedModule) => {
						foreach (var type in types)
							Assert.IsTrue (!modifiedModule.Types.Any (t => t.FullName == type.FullName));
					}, verify: false, assemblyResolver: assemblyResolver, applyWindowsRuntimeProjections: true);
				} finally {
					File.Delete (tmpPath);
				}
			}, readOnly: true, verify: false, assemblyResolver: assemblyResolver, applyWindowsRuntimeProjections: true);
		}
	}

	[TestFixture]
	public class ManagedWindowsRuntimeProjectionsTests : BaseWindowsRuntimeProjectionsTests {
		protected override string ModuleName { get { return "ManagedWinmd.winmd"; } }

		protected override MetadataKind ExpectedMetadataKind { get { return MetadataKind.ManagedWindowsMetadata; } }

		protected override string [] ManagedClassTypeNames { get { return new [] { "ManagedClass", "<WinRT>ManagedClass" }; } }

		protected override string [] CustomListTypeNames { get { return new [] { "CustomList", "<WinRT>CustomList" }; } }

		[Test]
		public void CanProjectClasses ()
		{
			if (Platform.OnMono)
				return;

			TestModule (ModuleName, (module) => {
				var managedClassType = module.Types.Single (t => t.Name == "ManagedClass");
				Assert.AreEqual ("<CLR>ManagedClass", managedClassType.WindowsRuntimeProjection.Name);
				Assert.AreEqual (TypeDefinitionTreatment.UnmangleWindowsRuntimeName, managedClassType.WindowsRuntimeProjection.Treatment);

				var someOtherClassType = module.Types.Single (t => t.Name == "SomeOtherClass");
				Assert.AreEqual ("<CLR>SomeOtherClass", someOtherClassType.WindowsRuntimeProjection.Name);
				Assert.AreEqual (TypeDefinitionTreatment.UnmangleWindowsRuntimeName, someOtherClassType.WindowsRuntimeProjection.Treatment);

				var winrtManagedClassType = module.Types.Single (t => t.Name == "<WinRT>ManagedClass");
				Assert.AreEqual ("ManagedClass", winrtManagedClassType.WindowsRuntimeProjection.Name);
				Assert.AreEqual (TypeDefinitionTreatment.PrefixWindowsRuntimeName, winrtManagedClassType.WindowsRuntimeProjection.Treatment);

				var winrtSomeOtherClassType = module.Types.Single (t => t.Name == "<WinRT>SomeOtherClass");
				Assert.AreEqual ("SomeOtherClass", winrtSomeOtherClassType.WindowsRuntimeProjection.Name);
				Assert.AreEqual (TypeDefinitionTreatment.PrefixWindowsRuntimeName, winrtSomeOtherClassType.WindowsRuntimeProjection.Treatment);
			}, verify: false, assemblyResolver: WindowsRuntimeAssemblyResolver.CreateInstance (), applyWindowsRuntimeProjections: true);
		}
	}

	[TestFixture]
	public class NativeWindowsRuntimeProjectionsTests : BaseWindowsRuntimeProjectionsTests {
		protected override string ModuleName { get { return "NativeWinmd.winmd"; } }

		protected override MetadataKind ExpectedMetadataKind { get { return MetadataKind.WindowsMetadata; } }

		protected override string [] ManagedClassTypeNames { get { return new [] { "ManagedClass" }; } }

		protected override string [] CustomListTypeNames { get { return new [] { "CustomList" }; } }
	}
}
#endif
