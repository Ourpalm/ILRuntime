using System;
using System.Collections.Generic;
using System.Linq;

namespace Mono.Cecil {

	public static class TypeDefinitionUtils {

		public static TypeReference TypeDefinitionForGeneric (
			Type genericType, AssemblyDefinition genericAssemblyDefinition,
			Type paramterType, AssemblyDefinition parameterAssemblyDefinition)
		{
			var paramDefinition = TypeDefinitionUtils.TypeDefinitionFor (paramterType, parameterAssemblyDefinition);
			var genericDefinition = TypeDefinitionUtils.TypeDefinitionFor (genericType, genericAssemblyDefinition);
			var genericInstance = new GenericInstanceType (genericDefinition);
			genericInstance.GenericArguments.Add (paramDefinition);
			return genericInstance;
		}

		public static TypeDefinition TypeDefinitionFor (Type type, AssemblyDefinition assemblyDefinition)
		{
			var stack = new Stack<string> ();
			var currentType = type;
			while  (currentType != null) {
				stack.Push ( (currentType.DeclaringType == null ? currentType.Namespace + "." : "") + currentType.Name);
				currentType = currentType.DeclaringType;
			}

			var typeDefinition = assemblyDefinition.MainModule.GetType (stack.Pop ());
			if  (typeDefinition == null)
				return null;

			while  (stack.Count > 0) {
				var name = stack.Pop ();
				typeDefinition = typeDefinition.NestedTypes.Single (t => t.Name == name);
			}

			return typeDefinition;
		}
	}
}
