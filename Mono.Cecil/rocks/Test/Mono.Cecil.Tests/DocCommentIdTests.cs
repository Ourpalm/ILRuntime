using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Mono.Cecil.Rocks;

namespace N
{

    /// <summary>
    /// ID string generated is "T:N.X". 
    /// </summary>
    public class X : IX<KVP<string, int>>
    {
        /// <summary>
        /// ID string generated is "M:N.X.#ctor".
        /// </summary>
        public X() { }


        /// <summary>
        /// ID string generated is "M:N.X.#ctor(System.Int32)".
        /// </summary>
        /// <param name="i">Describe parameter.</param>
        public X(int i) { }


        /// <summary>
        /// ID string generated is "F:N.X.q".
        /// </summary>
        public string q;


        /// <summary>
        /// ID string generated is "F:N.X.PI".
        /// </summary>
        public const double PI = 3.14;


        /// <summary>
        /// ID string generated is "M:N.X.f".
        /// </summary>
        public int f() { return 1; }


        /// <summary>
        /// ID string generated is "M:N.X.bb(System.String,System.Int32@)".
        /// </summary>
        public int bb(string s, ref int y) { return 1; }


        /// <summary>
        /// ID string generated is "M:N.X.gg(System.Int16[],System.Int32[0:,0:])". 
        /// </summary>
        public int gg(short[] array1, int[,] array) { return 0; }


        /// <summary>
        /// ID string generated is "M:N.X.op_Addition(N.X,N.X)". 
        /// </summary>
        public static X operator +(X x, X xx) { return x; }


        /// <summary>
        /// ID string generated is "P:N.X.prop".
        /// </summary>
        public int prop { get { return 1; } set { } }


        /// <summary>
        /// ID string generated is "E:N.X.d".
        /// </summary>
#pragma warning disable 67
        public event D d;
#pragma warning restore 67


		/// <summary>
		/// ID string generated is "P:N.X.Item(System.String)".
		/// </summary>
		public int this[string s] { get { return 1; } }


        /// <summary>
        /// ID string generated is "T:N.X.Nested".
        /// </summary>
        public class Nested { }


        /// <summary>
        /// ID string generated is "T:N.X.D". 
        /// </summary>
        public delegate void D(int i);


        /// <summary>
        /// ID string generated is "M:N.X.op_Explicit(N.X)~System.Int32".
        /// </summary>
        public static explicit operator int(X x) { return 1; }

	    public static void Linq (IEnumerable<string> enumerable, Func<string> selector)
	    {
	    }

        /// <summary>
		/// ID string generated is "M:N.X.N#IX{N#KVP{System#String,System#Int32}}#IXA(N.KVP{System.String,System.Int32})"
        /// </summary>
        void IX<KVP<string, int>>.IXA (KVP<string, int> k) { }
    }

	public interface IX<K>
	{
		void IXA (K k);
	}

	public class KVP<K, T> { }
}

namespace Mono.Cecil.Tests {

	[TestFixture]
	public class DocCommentIdTests {

		[Test]
		public void TypeDef ()
		{
			AssertDocumentID ("T:N.X", GetTestType ());
		}

		[Test]
		public void ParameterlessCtor ()
		{
			var type = GetTestType ();
			var ctor = type.GetConstructors ().Single (m => m.Parameters.Count == 0);

			AssertDocumentID ("M:N.X.#ctor", ctor);
		}

		[Test]
		public void CtorWithParameters ()
		{
			var type = GetTestType ();
			var ctor = type.GetConstructors ().Single (m => m.Parameters.Count == 1);

			AssertDocumentID ("M:N.X.#ctor(System.Int32)", ctor);
		}

		[Test]
		public void Field ()
		{
			var type = GetTestType ();
			var field = type.Fields.Single (m => m.Name == "q");

			AssertDocumentID ("F:N.X.q", field);
		}

		[Test]
		public void ConstField ()
		{
			var type = GetTestType ();
			var field = type.Fields.Single (m => m.Name == "PI");

			AssertDocumentID ("F:N.X.PI", field);
		}

		[Test]
		public void ParameterlessMethod ()
		{
			var type = GetTestType ();
			var method = type.Methods.Single (m => m.Name == "f");

			AssertDocumentID ("M:N.X.f", method);
		}

		[Test]
		public void MethodWithByRefParameters ()
		{
			var type = GetTestType ();
			var method = type.Methods.Single (m => m.Name == "bb");

			AssertDocumentID ("M:N.X.bb(System.String,System.Int32@)", method);
		}
		
		[Test]
		public void MethodWithArrayParameters ()
		{
			var type = GetTestType ();
			var method = type.Methods.Single (m => m.Name == "gg");

			AssertDocumentID ("M:N.X.gg(System.Int16[],System.Int32[0:,0:])", method);
		}

		[Test]
		public void OpAddition ()
		{
			var type = GetTestType ();
			var op = type.Methods.Single (m => m.Name == "op_Addition");

			AssertDocumentID ("M:N.X.op_Addition(N.X,N.X)", op);
		}

		[Test]
		public void OpExplicit ()
		{
			var type = GetTestType ();
			var op = type.Methods.Single (m => m.Name == "op_Explicit");

			AssertDocumentID ("M:N.X.op_Explicit(N.X)~System.Int32", op);
		}

		[Test]
		public void Property ()
		{
			var type = GetTestType ();
			var property = type.Properties.Single (p => p.Name == "prop");

			AssertDocumentID ("P:N.X.prop", property);
		}

		[Test]
		public void Indexer ()
		{
			var type = GetTestType ();
			var indexer = type.Properties.Single (p => p.Name == "Item");

			AssertDocumentID ("P:N.X.Item(System.String)", indexer);
		}

		[Test]
		public void Event ()
		{
			var type = GetTestType ();
			var @event = type.Events.Single (e => e.Name == "d");

			AssertDocumentID ("E:N.X.d", @event);
		}

		[Test]
		public void Delegate ()
		{
			var type = GetTestType ();
			var @delegate = type.NestedTypes.Single (t => t.Name == "D");

			AssertDocumentID ("T:N.X.D", @delegate);
		}

		[Test]
		public void NestedType ()
		{
			var type = GetTestType ();
			var nestedType = type.NestedTypes.Single (t => t.Name == "Nested");

			AssertDocumentID ("T:N.X.Nested", nestedType);
		}

		[Test]
		public void Linq ()
		{
			var type = GetTestType ();
			var method = type.GetMethod ("Linq");

			AssertDocumentID ("M:N.X.Linq(System.Collections.Generic.IEnumerable{System.String},System.Func{System.String})", method);
		}

		[Test]
		public void EII ()
		{
			var type = GetTestType ();
			var method = type.Methods.Where (m => m.Name.Contains("IXA")).First ();

			AssertDocumentID ("M:N.X.N#IX{N#KVP{System#String,System#Int32}}#IXA(N.KVP{System.String,System.Int32})", method);
		}

		TypeDefinition GetTestType ()
		{
			return typeof (N.X).ToDefinition ();
		}

		static void AssertDocumentID (string docId, IMemberDefinition member)
		{
			Assert.AreEqual (docId, DocCommentId.GetDocCommentId (member));
		}
	}
}
