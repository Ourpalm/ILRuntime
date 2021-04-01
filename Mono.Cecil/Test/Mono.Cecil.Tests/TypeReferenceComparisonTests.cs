using System;
using System.Collections.Generic;
using Mono.Cecil;
using NUnit.Framework;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class TypeReferenceComparisonTests : LoadAssemblyDefinitionForTestsBaseSimple {

		[SetUp]
		public void SetUp ()
		{
			SetupAssemblyDefinitions (typeof (TypeReferenceComparisonTests).Assembly);
		}

		[Test]
		public void TypeReferenceEqualsTypeDefinition ()
		{
			var typeDefinition = TypeDefinitionUtils.TypeDefinitionFor (typeof (Int32), _mscorlib);
			var typeReference = new TypeReference (typeDefinition.Namespace, typeDefinition.Name, typeDefinition.Module, typeDefinition.Scope);

			Assert.That (TypeReferenceEqualityComparer.AreEqual (typeDefinition, typeReference), Is.True);
		}

		[Test]
		public void GenericParametersFromTwoTypesAreNotEqual ()
		{
			var listDefinition = TypeDefinitionUtils.TypeDefinitionFor (typeof (List<>), _mscorlib);
			var stackDefinition = TypeDefinitionUtils.TypeDefinitionFor (typeof (Comparer<>), _mscorlib);

			Assert.That (TypeReferenceEqualityComparer.AreEqual (listDefinition.GenericParameters[0], stackDefinition.GenericParameters[0]), Is.False);
		}

		[Test]
		public void ArrayTypesDoNotMatchIfRankIsDifferent ()
		{
			var elementType = TypeDefinitionUtils.TypeDefinitionFor (typeof (Int32), _mscorlib);

			Assert.That (TypeReferenceEqualityComparer.AreEqual (new ArrayType (elementType, 1), new ArrayType (elementType, 2)), Is.False, "Two array types with different ranks match, which is not expected.");
		}

		[Test]
		public void ArrayTypesDoNotMatchIfElementTypeIsDifferent ()
		{
			Assert.That (TypeReferenceEqualityComparer.AreEqual (new ArrayType (TypeDefinitionUtils.TypeDefinitionFor (typeof (Int32), _mscorlib), 1), new ArrayType (TypeDefinitionUtils.TypeDefinitionFor (typeof (Int64), _mscorlib), 1)), Is.False, "Two array types with different element types match, which is not expected.");
		}

		[Test]
		public void ArrayTypesWithDifferentRanksToNotMatch ()
		{
			var elementType = TypeDefinitionUtils.TypeDefinitionFor (typeof (Int32), _mscorlib);

			Assert.That (TypeReferenceEqualityComparer.AreEqual ( (TypeSpecification) new ArrayType (elementType, 1),  (TypeSpecification) new ArrayType (elementType, 2)), Is.False, "Two type specifications that are array types with different ranks match, which is not expected.");
		}

		[Test]
		public void GenericInstanceTypeFromTwoTypesAreNotEqual ()
		{
			var int32Definition = TypeDefinitionUtils.TypeDefinitionFor (typeof (Int32), _mscorlib);
			var listDefinition = TypeDefinitionUtils.TypeDefinitionFor (typeof (List<>), _mscorlib);
			var listGenericInstance = new GenericInstanceType (listDefinition);
			listGenericInstance.GenericArguments.Add (int32Definition);
			var stackDefinition = TypeDefinitionUtils.TypeDefinitionFor (typeof (Comparer<>), _mscorlib);
			var stackGenericInstance = new GenericInstanceType (stackDefinition);
			stackGenericInstance.GenericArguments.Add (int32Definition);

			Assert.That (TypeReferenceEqualityComparer.AreEqual (listGenericInstance, stackGenericInstance), Is.False);
		}

		[Test]
		public void GenericInstanceTypeForSameTypeIsEqual ()
		{
			var int32Definition = TypeDefinitionUtils.TypeDefinitionFor (typeof (Int32), _mscorlib);
			var listDefinition = TypeDefinitionUtils.TypeDefinitionFor (typeof (List<>), _mscorlib);
			var listGenericInstance = new GenericInstanceType (listDefinition);
			listGenericInstance.GenericArguments.Add (int32Definition);
			var listGenericInstance2 = new GenericInstanceType (listDefinition);
			listGenericInstance2.GenericArguments.Add (int32Definition);

			Assert.That (TypeReferenceEqualityComparer.AreEqual (listGenericInstance, listGenericInstance2), Is.True);
		}
	}
}
