using System;
using System.Collections.Generic;
using ILRuntimeTest.TestFramework;

namespace TestCases
{
    public static class NeoStep145Test
    {
        public static void NeoStep145Exception()
        {
            var exception = new Exception("Neo constructor");
            try { throw exception; }
            catch (Exception actual)
            {
                if (!exception.Equals(actual)) throw new Exception("Exception identity lost");
                if (actual.Message != "Neo constructor") throw new Exception("Message lost");
            }
        }

        public static void NeoStep145GeneratedBinding()
        {
            var first = new List<int>();
            var second = new List<int>();
            first.Add(42);
            second.Add(7);
            if (first[0] != 42 || second[0] != 7) throw new Exception("Constructor result slot lost");
        }

        public static void NeoStep145GeneratedLongParameter()
        {
            var property = new BindableProperty<long>(1234567890123L);
            if (property.Value != 1234567890123L) throw new Exception("Generated constructor alignment");
        }

        public static void NeoStep145ReflectionArguments()
        {
            var value = new NeoStep145CLRObject(3, 1234567890123L, 2, 6.25, "text", null, true, 'X');
            if (value.Small != 3 || value.Wide != 1234567890123L || value.Narrow != 2 || value.Real != 6.25 ||
                value.Text != "text" || !object.Equals(value.Reference, null) || !value.Flag || value.Character != 'X')
                throw new Exception("Mixed constructor arguments corrupted");
        }

        public static void NeoStep145Strings()
        {
            if (new string('x', 3) != "xxx") throw new Exception("String repeat");
            if (new string('x', 0) != "") throw new Exception("String empty");
            var characters = "abcd".ToCharArray();
            if (new string(characters) != "abcd") throw new Exception("String array");
            if (new string(characters, 1, 2) != "bc") throw new Exception("String slice");
            if (new string((char[])null) != "") throw new Exception("String null array");
        }

        public static void NeoStep145StringFailure()
        {
            string original = "original";
            try { original = new string('x', -1); }
            catch (ArgumentOutOfRangeException)
            {
                if (original != "original") throw new Exception("String rollback");
                return;
            }
            throw new Exception("Missing string exception");
        }

        public static void NeoStep145ReflectionRollback()
        {
            var failure = new InvalidOperationException("ctor failure");
            var value = new NeoStep145CLRObject(0, failure);
            var original = value;
            try { value = new NeoStep145CLRObject(1, failure); }
            catch (InvalidOperationException actual)
            {
                if (!failure.Equals(actual)) throw new Exception("Constructor exception identity lost");
                if (!original.Equals(value)) throw new Exception("Constructor rollback lost old value");
                if (!actual.StackTrace.Contains("NeoStep145CLRObject..ctor"))
                    throw new Exception("Constructor stack lost");
                return;
            }
            throw new Exception("Missing constructor exception");
        }

        public static void NeoStep145RepeatedConstruction()
        {
            for (int i = 0; i < 20; i++)
            {
                NeoStep145ReflectionRollback();
                NeoStep145ReflectionArguments();
            }
        }

        public static void NeoStep145RedirectionRollback()
        {
            for (int i = 0; i < 20; i++)
            {
                string value = "original";
                bool caught = false;
                try { value = new string('x', -1); }
                catch (ArgumentOutOfRangeException)
                {
                    if (value != "original") throw new Exception("Redirection rollback lost old value");
                    caught = true;
                }
                if (!caught) throw new Exception("Missing redirection failure");
                value = new string('x', 3);
                if (value != "xxx") throw new Exception("Redirection recovery failed");
            }
        }
    }
}
