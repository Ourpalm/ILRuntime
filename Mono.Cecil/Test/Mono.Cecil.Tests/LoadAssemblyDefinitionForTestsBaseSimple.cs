using System.Reflection;

namespace Mono.Cecil.Tests {

	public class LoadAssemblyDefinitionForTestsBaseSimple {

		protected AssemblyDefinition _assembly;
		protected AssemblyDefinition _mscorlib;

		public void SetupAssemblyDefinitions (Assembly testAssembly)
		{
			_assembly = AssemblyDefinition.ReadAssembly (testAssembly.Location);
			_mscorlib = _assembly.MainModule.TypeSystem.Object.Resolve ().Module.Assembly;
		}
	}
}
