using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;

using NUnit.Framework;

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class CustomAttributesTests : BaseTestFixture {

		[Test]
		public void StringArgumentOnType ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var hamster = module.GetType ("Hamster");

				Assert.IsTrue (hamster.HasCustomAttributes);
				Assert.AreEqual (1, hamster.CustomAttributes.Count);

				var attribute = hamster.CustomAttributes [0];
				Assert.AreEqual ("System.Void FooAttribute::.ctor(System.String)",
					attribute.Constructor.FullName);

				Assert.IsTrue (attribute.HasConstructorArguments);
				Assert.AreEqual (1, attribute.ConstructorArguments.Count);

				AssertArgument ("bar", attribute.ConstructorArguments [0]);
			});
		}

		[Test]
		public void NullString ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var dentist = module.GetType ("Dentist");

				var attribute = GetAttribute (dentist, "Foo");
				Assert.IsNotNull (attribute);

				AssertArgument<string> (null, attribute.ConstructorArguments [0]);
			});
		}

		[Test]
		public void Primitives1 ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var steven = module.GetType ("Steven");

				var attribute = GetAttribute (steven, "Foo");
				Assert.IsNotNull (attribute);

				AssertArgument<sbyte> (-12, attribute.ConstructorArguments [0]);
				AssertArgument<byte> (242, attribute.ConstructorArguments [1]);
				AssertArgument<bool> (true, attribute.ConstructorArguments [2]);
				AssertArgument<bool> (false, attribute.ConstructorArguments [3]);
				AssertArgument<ushort> (4242, attribute.ConstructorArguments [4]);
				AssertArgument<short> (-1983, attribute.ConstructorArguments [5]);
				AssertArgument<char> ('c', attribute.ConstructorArguments [6]);
			});
		}

		[Test]
		public void Primitives2 ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var seagull = module.GetType ("Seagull");

				var attribute = GetAttribute (seagull, "Foo");
				Assert.IsNotNull (attribute);

				AssertArgument<int> (-100000, attribute.ConstructorArguments [0]);
				AssertArgument<uint> (200000, attribute.ConstructorArguments [1]);
				AssertArgument<float> (12.12f, attribute.ConstructorArguments [2]);
				AssertArgument<long> (long.MaxValue, attribute.ConstructorArguments [3]);
				AssertArgument<ulong> (ulong.MaxValue, attribute.ConstructorArguments [4]);
				AssertArgument<double> (64.646464, attribute.ConstructorArguments [5]);
			});
		}

		[Test]
		public void StringArgumentOnAssembly ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var assembly = module.Assembly;

				var attribute = GetAttribute (assembly, "Foo");
				Assert.IsNotNull (attribute);

				AssertArgument ("bingo", attribute.ConstructorArguments [0]);
			});
		}

		[Test]
		public void CharArray ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var rifle = module.GetType ("Rifle");

				var attribute = GetAttribute (rifle, "Foo");
				Assert.IsNotNull (attribute);

				var argument = attribute.ConstructorArguments [0];

				Assert.AreEqual ("System.Char[]", argument.Type.FullName);

				var array = argument.Value as CustomAttributeArgument [];
				Assert.IsNotNull (array);

				var str = "cecil";

				Assert.AreEqual (array.Length, str.Length);

				for (int i = 0; i < str.Length; i++)
				AssertArgument (str [i], array [i]);
			});
		}

		[Test]
		public void BoxedArguments ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var worm = module.GetType ("Worm");

				var attribute = GetAttribute (worm, "Foo");
				Assert.IsNotNull (attribute);

				Assert.AreEqual (".ctor ((Object:(String:\"2\")), (Object:(I4:2)))", PrettyPrint (attribute));
			});
		}

		[Test]
		public void BoxedArraysArguments ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var sheep = module.GetType ("Sheep");

				var attribute = GetAttribute (sheep, "Foo");
				Assert.IsNotNull (attribute);

				// [Foo (new object [] { "2", 2, 'c' }, new object [] { new object [] { 1, 2, 3}, null })]
				AssertCustomAttribute (".ctor ((Object:(Object[]:{(Object:(String:\"2\")), (Object:(I4:2)), (Object:(Char:'c'))})), (Object:(Object[]:{(Object:(Object[]:{(Object:(I4:1)), (Object:(I4:2)), (Object:(I4:3))})), (Object:(String:null))})))", attribute);
			});
		}

		[Test]
		public void FieldsAndProperties ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var angola = module.GetType ("Angola");

				var attribute = GetAttribute (angola, "Foo");
				Assert.IsNotNull (attribute);

				Assert.AreEqual (2, attribute.Fields.Count);

				var argument = attribute.Fields.Where (a => a.Name == "Pan").First ();
				AssertCustomAttributeArgument ("(Object:(Object[]:{(Object:(I4:1)), (Object:(String:\"2\")), (Object:(Char:'3'))}))", argument);

				argument = attribute.Fields.Where (a => a.Name == "PanPan").First ();
				AssertCustomAttributeArgument ("(String[]:{(String:\"yo\"), (String:\"yo\")})", argument);

				Assert.AreEqual (2, attribute.Properties.Count);

				argument = attribute.Properties.Where (a => a.Name == "Bang").First ();
				AssertArgument (42, argument);

				argument = attribute.Properties.Where (a => a.Name == "Fiou").First ();
				AssertArgument<string> (null, argument);
			});
		}

		[Test]
		public void BoxedStringField ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var type = module.GetType ("BoxedStringField");

				var attribute = GetAttribute (type, "Foo");
				Assert.IsNotNull (attribute);

				Assert.AreEqual (1, attribute.Fields.Count);

				var argument = attribute.Fields.Where (a => a.Name == "Pan").First ();
				AssertCustomAttributeArgument ("(Object:(String:\"fiouuu\"))", argument);
			});
		}

		[Test]
		public void TypeDefinitionEnum ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var zero = module.GetType ("Zero");

				var attribute = GetAttribute (zero, "Foo");
				Assert.IsNotNull (attribute);

				Assert.AreEqual (1, attribute.ConstructorArguments.Count);

				Assert.AreEqual ((short) 2, attribute.ConstructorArguments [0].Value);
				Assert.AreEqual ("Bingo", attribute.ConstructorArguments [0].Type.FullName);
			});
		}

		[Test]
		public void TypeReferenceEnum ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var ace = module.GetType ("Ace");

				var attribute = GetAttribute (ace, "Foo");
				Assert.IsNotNull (attribute);

				Assert.AreEqual (1, attribute.ConstructorArguments.Count);

				Assert.AreEqual ((byte) 0x04, attribute.ConstructorArguments [0].Value);
				Assert.AreEqual ("System.Security.AccessControl.AceFlags", attribute.ConstructorArguments [0].Type.FullName);
				Assert.AreEqual (module, attribute.ConstructorArguments [0].Type.Module);
			});
		}

		[Test]
		public void BoxedEnumReference ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var bzzz = module.GetType ("Bzzz");

				var attribute = GetAttribute (bzzz, "Foo");
				Assert.IsNotNull (attribute);

				// [Foo (new object [] { Bingo.Fuel, Bingo.Binga }, null, Pan = System.Security.AccessControl.AceFlags.NoPropagateInherit)]

				Assert.AreEqual (2, attribute.ConstructorArguments.Count);

				var argument = attribute.ConstructorArguments [0];

				AssertCustomAttributeArgument ("(Object:(Object[]:{(Object:(Bingo:2)), (Object:(Bingo:4))}))", argument);

				argument = attribute.ConstructorArguments [1];

				AssertCustomAttributeArgument ("(Object:(String:null))", argument);

				argument = attribute.Fields.Where (a => a.Name == "Pan").First ().Argument;

				AssertCustomAttributeArgument ("(Object:(System.Security.AccessControl.AceFlags:4))", argument);
			});
		}

		[Test]
		public void TypeOfTypeDefinition ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var typed = module.GetType ("Typed");

				var attribute = GetAttribute (typed, "Foo");
				Assert.IsNotNull (attribute);

				Assert.AreEqual (1, attribute.ConstructorArguments.Count);

				var argument = attribute.ConstructorArguments [0];

				Assert.AreEqual ("System.Type", argument.Type.FullName);

				var type = argument.Value as TypeDefinition;
				Assert.IsNotNull (type);

				Assert.AreEqual ("Bingo", type.FullName);
			});
		}

		[Test]
		public void TypeOfNestedTypeDefinition ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var typed = module.GetType ("NestedTyped");

				var attribute = GetAttribute (typed, "Foo");
				Assert.IsNotNull (attribute);

				Assert.AreEqual (1, attribute.ConstructorArguments.Count);

				var argument = attribute.ConstructorArguments [0];

				Assert.AreEqual ("System.Type", argument.Type.FullName);

				var type = argument.Value as TypeDefinition;
				Assert.IsNotNull (type);

				Assert.AreEqual ("FooAttribute/Token", type.FullName);
			});
		}

		[Test]
		public void FieldTypeOf ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var truc = module.GetType ("Truc");

				var attribute = GetAttribute (truc, "Foo");
				Assert.IsNotNull (attribute);

				var argument = attribute.Fields.Where (a => a.Name == "Chose").First ().Argument;

				Assert.AreEqual ("System.Type", argument.Type.FullName);

				var type = argument.Value as TypeDefinition;
				Assert.IsNotNull (type);

				Assert.AreEqual ("Typed", type.FullName);
			});
		}

		[Test]
		public void EscapedTypeName ()
		{
			TestModule ("bug-185.dll", module => {
				var foo = module.GetType ("Foo");
				var foo_do = foo.Methods.Where (m => !m.IsConstructor).First ();

				var attribute = foo_do.CustomAttributes.Where (ca => ca.AttributeType.Name == "AsyncStateMachineAttribute").First ();

				Assert.AreEqual (foo.NestedTypes [0], attribute.ConstructorArguments [0].Value);

				var function = module.GetType ("Function`1");

				var apply = function.Methods.Where(m => !m.IsConstructor).First ();

				attribute = apply.CustomAttributes.Where (ca => ca.AttributeType.Name == "AsyncStateMachineAttribute").First ();

				Assert.AreEqual (function.NestedTypes [0], attribute.ConstructorArguments [0].Value);
			});
		}

		[Test]
		public void FieldNullTypeOf ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var truc = module.GetType ("Machin");

				var attribute = GetAttribute (truc, "Foo");
				Assert.IsNotNull (attribute);

				var argument = attribute.Fields.Where (a => a.Name == "Chose").First ().Argument;

				Assert.AreEqual ("System.Type", argument.Type.FullName);

				Assert.IsNull (argument.Value);
			});
		}

		[Test]
		public void OpenGenericTypeOf ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var open_generic = module.GetType ("OpenGeneric`2");
				Assert.IsNotNull (open_generic);

				var attribute = GetAttribute (open_generic, "Foo");
				Assert.IsNotNull (attribute);

				Assert.AreEqual (1, attribute.ConstructorArguments.Count);

				var argument = attribute.ConstructorArguments [0];

				Assert.AreEqual ("System.Type", argument.Type.FullName);

				var type = argument.Value as TypeReference;
				Assert.IsNotNull (type);

				Assert.AreEqual ("System.Collections.Generic.Dictionary`2", type.FullName);
			});
		}

		[Test]
		public void ClosedGenericTypeOf ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var closed_generic = module.GetType ("ClosedGeneric");
				Assert.IsNotNull (closed_generic);

				var attribute = GetAttribute (closed_generic, "Foo");
				Assert.IsNotNull (attribute);

				Assert.AreEqual (1, attribute.ConstructorArguments.Count);

				var argument = attribute.ConstructorArguments [0];

				Assert.AreEqual ("System.Type", argument.Type.FullName);

				var type = argument.Value as TypeReference;
				Assert.IsNotNull (type);

				Assert.AreEqual ("System.Collections.Generic.Dictionary`2<System.String,OpenGeneric`2<Machin,System.Int32>[,]>", type.FullName);
			});
		}

		[Test]
		public void TypeOfArrayOfNestedClass ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var parent = module.GetType ("Parent");
				Assert.IsNotNull (parent);

				var attribute = GetAttribute (parent, "Foo");
				Assert.IsNotNull (attribute);

				Assert.AreEqual (1, attribute.ConstructorArguments.Count);

				var argument = attribute.ConstructorArguments [0];

				Assert.AreEqual ("System.Type", argument.Type.FullName);

				var type = argument.Value as TypeReference;
				Assert.IsNotNull (type);

				Assert.AreEqual ("Parent/Child[]", type.FullName);
			});
		}

		[Test]
		public void EmptyBlob ()
		{
			TestIL ("ca-empty-blob.il", module => {
				var attribute = module.GetType ("CustomAttribute");
				Assert.AreEqual (1, attribute.CustomAttributes.Count);
				Assert.AreEqual (0, attribute.CustomAttributes [0].ConstructorArguments.Count);
			}, verify: !Platform.OnMono);
		}

		[Test]
		public void InterfaceImplementation ()
		{
			OnlyOnWindows (); // Mono's ilasm doesn't support .interfaceimpl

			TestIL ("ca-iface-impl.il", module => {
				var type = module.GetType ("FooType");
				var iface = type.Interfaces.Single (i => i.InterfaceType.FullName == "IFoo");
				Assert.IsTrue (iface.HasCustomAttributes);
				var attributes = iface.CustomAttributes;
				Assert.AreEqual (1, attributes.Count);
				Assert.AreEqual ("FooAttribute", attributes [0].AttributeType.FullName);
			});
		}

		[Test]
		public void GenericParameterConstraint ()
		{
			TestModule ("GenericParameterConstraintAttributes.dll", module => {
				var type = module.GetType ("Foo.Library`1");
				var gp = type.GenericParameters.Single ();
				var constraint = gp.Constraints.Single ();

				Assert.IsTrue (constraint.HasCustomAttributes);
				var attributes = constraint.CustomAttributes;
				Assert.AreEqual (1, attributes.Count);
				Assert.AreEqual ("System.Runtime.CompilerServices.NullableAttribute", attributes [0].AttributeType.FullName);
			}, verify: !Platform.OnMono);
		}

		[Test]
		public void NullCharInString ()
		{
			TestCSharp ("CustomAttributes.cs", module => {
				var type = module.GetType ("NullCharInString");
				var attributes = type.CustomAttributes;
				Assert.AreEqual (1, attributes.Count);
				var attribute = attributes [0];
				Assert.AreEqual (1, attribute.ConstructorArguments.Count);
				var value = (string) attribute.ConstructorArguments [0].Value;
				Assert.AreEqual (8, value.Length);
				Assert.AreEqual ('\0', value [7]);
			});
		}

		[Test]
		public void OrderedAttributes ()
		{
			TestModule ("ordered-attrs.exe", module => {
				var type = module.GetType ("Program");
				var method = type.GetMethod ("Main");
				var attributes = method.CustomAttributes;
				Assert.AreEqual (6, attributes.Count);

				Assert.AreEqual ("AAttribute", attributes [0].AttributeType.Name);
				Assert.AreEqual ("Main.A1", attributes [0].Fields [0].Argument.Value as string);

				Assert.AreEqual ("AAttribute", attributes [1].AttributeType.Name);
				Assert.AreEqual ("Main.A2", attributes [1].Fields [0].Argument.Value as string);

				Assert.AreEqual ("BAttribute", attributes [2].AttributeType.Name);
				Assert.AreEqual ("Main.B1", attributes [2].Fields [0].Argument.Value as string);

				Assert.AreEqual ("AAttribute", attributes [3].AttributeType.Name);
				Assert.AreEqual ("Main.A3", attributes [3].Fields [0].Argument.Value as string);

				Assert.AreEqual ("BAttribute", attributes [4].AttributeType.Name);
				Assert.AreEqual ("Main.B2", attributes [4].Fields [0].Argument.Value as string);

				Assert.AreEqual ("BAttribute", attributes [5].AttributeType.Name);
				Assert.AreEqual ("Main.B3", attributes [5].Fields [0].Argument.Value as string);
			});
		}

		[Test]
		public void DefineCustomAttributeFromBlob ()
		{
			var file = Path.Combine (Path.GetTempPath (), "CaBlob.dll");

			var module = ModuleDefinition.CreateModule ("CaBlob.dll", new ModuleParameters { Kind = ModuleKind.Dll, Runtime = TargetRuntime.Net_2_0 });
			var assembly_title_ctor = module.ImportReference (typeof (System.Reflection.AssemblyTitleAttribute).GetConstructor (new [] {typeof (string)}));

			Assert.IsNotNull (assembly_title_ctor);

			var buffer = new ByteBuffer ();
			buffer.WriteUInt16 (1); // ca signature

			var title = Encoding.UTF8.GetBytes ("CaBlob");

			buffer.WriteCompressedUInt32 ((uint) title.Length);
			buffer.WriteBytes (title);

			buffer.WriteUInt16 (0); // named arguments

			var blob = new byte [buffer.length];
			Buffer.BlockCopy (buffer.buffer, 0, blob, 0, buffer.length);

			var attribute = new CustomAttribute (assembly_title_ctor, blob);
			module.Assembly.CustomAttributes.Add (attribute);

			module.Write (file);

			module = ModuleDefinition.ReadModule (file);

			attribute = GetAttribute (module.Assembly, "AssemblyTitle");

			Assert.IsNotNull (attribute);
			Assert.AreEqual ("CaBlob", (string) attribute.ConstructorArguments [0].Value);

			module.Dispose ();
		}

		static void AssertCustomAttribute (string expected, CustomAttribute attribute)
		{
			Assert.AreEqual (expected, PrettyPrint (attribute));
		}

		static void AssertCustomAttributeArgument (string expected, CustomAttributeNamedArgument named_argument)
		{
			AssertCustomAttributeArgument (expected, named_argument.Argument);
		}

		static void AssertCustomAttributeArgument (string expected, CustomAttributeArgument argument)
		{
			var result = new StringBuilder ();
			PrettyPrint (argument, result);

			Assert.AreEqual (expected, result.ToString ());
		}

		static string PrettyPrint (CustomAttribute attribute)
		{
			var signature = new StringBuilder ();
			signature.Append (".ctor (");

			for (int i = 0; i < attribute.ConstructorArguments.Count; i++) {
				if (i > 0)
					signature.Append (", ");

				PrettyPrint (attribute.ConstructorArguments [i], signature);
			}

			signature.Append (")");
			return signature.ToString ();
		}

		static void PrettyPrint (CustomAttributeArgument argument, StringBuilder signature)
		{
			var value = argument.Value;

			signature.Append ("(");

			PrettyPrint (argument.Type, signature);

			signature.Append (":");

			PrettyPrintValue (argument.Value, signature);

			signature.Append (")");
		}

		static void PrettyPrintValue (object value, StringBuilder signature)
		{
			if (value == null) {
				signature.Append ("null");
				return;
			}

			var arguments = value as CustomAttributeArgument [];
			if (arguments != null) {
				signature.Append ("{");
				for (int i = 0; i < arguments.Length; i++) {
					if (i > 0)
						signature.Append (", ");

					PrettyPrint (arguments [i], signature);
				}
				signature.Append ("}");

				return;
			}

			switch (Type.GetTypeCode (value.GetType ())) {
			case System.TypeCode.String:
				signature.AppendFormat ("\"{0}\"", value);
				break;
			case System.TypeCode.Char:
				signature.AppendFormat ("'{0}'", (char) value);
				break;
			default:
				var formattable = value as IFormattable;
				if (formattable != null) {
					signature.Append (formattable.ToString (null, CultureInfo.InvariantCulture));
					return;
				}

				if (value is CustomAttributeArgument) {
					PrettyPrint ((CustomAttributeArgument) value, signature);
					return;
				}
				break;
			}
		}

		static void PrettyPrint (TypeReference type, StringBuilder signature)
		{
			if (type.IsArray) {
				ArrayType array = (ArrayType) type;
				signature.AppendFormat ("{0}[]", array.ElementType.etype.ToString ());
			} else if (type.etype == ElementType.None) {
				signature.Append (type.FullName);
			} else
				signature.Append (type.etype.ToString ());
		}

		static void AssertArgument<T> (T value, CustomAttributeNamedArgument named_argument)
		{
			AssertArgument (value, named_argument.Argument);
		}

		static void AssertArgument<T> (T value, CustomAttributeArgument argument)
		{
			AssertArgument (typeof (T).FullName, (object) value, argument);
		}

		static void AssertArgument (string type, object value, CustomAttributeArgument argument)
		{
			Assert.AreEqual (type, argument.Type.FullName);
			Assert.AreEqual (value, argument.Value);
		}

		static CustomAttribute GetAttribute (ICustomAttributeProvider owner, string type)
		{
			Assert.IsTrue (owner.HasCustomAttributes);

			foreach (var attribute in owner.CustomAttributes)
				if (attribute.Constructor.DeclaringType.Name.StartsWith (type))
					return attribute;

			return null;
		}
	}
}
