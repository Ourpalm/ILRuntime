using NUnit.Framework;
using System;
using System.Linq;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class MethodReferenceComparerTests : LoadAssemblyDefinitionForTestsBaseSimple {

		private TypeDefinition _class1;
		private TypeDefinition _class2;

		[SetUp]
		public void SetUp ()
		{
			SetupAssemblyDefinitions (typeof (MethodReferenceComparerTests).Assembly);
			_class1 = TypeDefinitionUtils.TypeDefinitionFor (typeof (Class1), _assembly);
			_class2 = TypeDefinitionUtils.TypeDefinitionFor (typeof (Class2), _assembly);
		}

		[Test]
		public void MethodReferenceEqualsMethodDefinition ()
		{
			var typeDefinition = TypeDefinitionUtils.TypeDefinitionFor (typeof (Int32), _mscorlib);
			var method = typeDefinition.Methods.Single (m => m.Name == "GetHashCode");
			var methodReference = new MethodReference (method.Name, method.ReturnType, method.DeclaringType);
			methodReference.HasThis = method.HasThis;

			Assert.That (MethodReferenceComparer.AreEqual (method, methodReference), Is.True);
		}

		[Test]
		public void VerifyMethodSignatureMatches ()
		{
			Assert.IsTrue (CompareSignatures ("MethodWithNoParametersOrReturn"));
			Assert.IsTrue (CompareSignatures ("GenericMethodWithNoParametersOrReturn"));
			Assert.IsFalse (CompareSignatures ("MethodWithNoParametersOrReturn", "GenericMethodWithNoParametersOrReturn"));

			Assert.IsTrue (CompareSignatures ("MethodWithIntParameterAndVoidReturn"));
		}

		[Test]
		public void VerifySignatureComparisonConsidersStatic ()
		{
			Assert.IsTrue (CompareSignatures ("StaticMethodWithNoParametersOrReturn"));
			Assert.IsTrue (CompareSignatures ("StaticMethodWithNoParametersOrReturn"));
			Assert.IsFalse (CompareSignatures ("MethodWithNoParametersOrReturn", "StaticMethodWithNoParametersOrReturn"));
			Assert.IsFalse (CompareSignatures ("GenericMethodWithNoParametersOrReturn", "GenericStaticMethodWithNoParametersOrReturn"));
		}

		[Test]
		public void VerifyMethodSignatureWithGenericParameters ()
		{
			Assert.IsTrue (CompareSignatures ("GenericMethodWithGenericParameter"));
			Assert.IsTrue (CompareSignatures ("GenericMethodWithGenericParameterArray"));
			Assert.IsTrue (CompareSignatures ("GenericMethodWithByReferenceGenericParameter"));
			Assert.IsTrue (CompareSignatures ("GenericMethodWithGenericInstanceGenericParameter"));
		}

		[Test]
		public void VerifyNonResolvableMethodReferencesWithDifferentParameterTypesAreNotEqual ()
		{
			var method1 = new MethodReference ("TestMethod", _class1.Module.TypeSystem.Void, _class1);
			method1.Parameters.Add (new ParameterDefinition (new ByReferenceType (_class1.Module.TypeSystem.Int16)));

			var method2 = new MethodReference ("TestMethod", _class1.Module.TypeSystem.Void, _class1);
			method2.Parameters.Add (new ParameterDefinition (new ByReferenceType (_class1.Module.TypeSystem.Char)));

			Assert.IsFalse (MethodReferenceComparer.AreEqual (method1, method2));
		}

		[Test]
		public void VerifyNonResolvableRecursiveMethodsDontStackOverflow ()
		{
			var method1 = new MethodReference ("TestMethod", _class1.Module.TypeSystem.Void, _class1);
			method1.GenericParameters.Add (new GenericParameter (method1));
			method1.Parameters.Add (new ParameterDefinition (method1.GenericParameters[0]));

			var method2 = new MethodReference ("TestMethod", _class1.Module.TypeSystem.Void, _class1);
			method2.GenericParameters.Add (new GenericParameter (method2));
			method2.Parameters.Add (new ParameterDefinition (method2.GenericParameters[0]));

			Assert.IsTrue (MethodReferenceComparer.AreEqual (method1, method2));
		}

		bool CompareSignatures (string name)
		{
			return CompareSignatures (name, name);
		}

		bool CompareSignatures (string name1, string name2)
		{
			return MethodReferenceComparer.AreSignaturesEqual (GetMethod (_class1, name1), GetMethod (_class2, name2), TypeComparisonMode.SignatureOnly);
		}

		static MethodDefinition GetMethod (TypeDefinition type, string name)
		{
			return type.Methods.Single (m => m.Name == name);
		}

		class GenericClass<T> {

		}

		class Class1 {

			void MethodWithNoParametersOrReturn () {}
			void GenericMethodWithNoParametersOrReturn<T> () {}
			static void StaticMethodWithNoParametersOrReturn () {}
			static void GenericStaticMethodWithNoParametersOrReturn<T> () {}

			void MethodWithIntParameterAndVoidReturn (int a) {}

			void GenericMethodWithGenericParameter<T> (T t) {}
			void GenericMethodWithGenericParameterArray<T> (T[] t) {}
			void GenericMethodWithByReferenceGenericParameter<T> (ref T a) {}
			void GenericMethodWithGenericInstanceGenericParameter<T> (GenericClass<T> a) {}
		}

		class Class2 {

			void MethodWithNoParametersOrReturn () {}
			void GenericMethodWithNoParametersOrReturn<T> () {}
			static void StaticMethodWithNoParametersOrReturn () {}
			static void GenericStaticMethodWithNoParametersOrReturn<T> () {}

			void MethodWithIntParameterAndVoidReturn (int a) {}

			void GenericMethodWithGenericParameter<T> (T t) {}
			void GenericMethodWithGenericParameterArray<T> (T[] t) {}
			void GenericMethodWithByReferenceGenericParameter<T> (ref T a) {}
			void GenericMethodWithGenericInstanceGenericParameter<T> (GenericClass<T> a) {}
		}
	}
}
