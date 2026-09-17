using System;
using System.Collections.Generic;

namespace TestCases
{
    public interface INeoStep15Marker { }

    public class NeoStep15Base { }

    public class NeoStep15Derived : NeoStep15Base, INeoStep15Marker { }

    public class NeoStep15Unrelated { }

    public interface INeoStep15Producer<out T>
    {
        T Get();
    }

    public sealed class NeoStep15Producer<T> : INeoStep15Producer<T>
    {
        private readonly T value;

        public NeoStep15Producer(T value)
        {
            this.value = value;
        }

        public T Get()
        {
            return value;
        }
    }

    public interface INeoStep15Consumer<in T>
    {
        void Accept(T value);
    }

    public sealed class NeoStep15Consumer<T> : INeoStep15Consumer<T>
    {
        public void Accept(T value) { }
    }

    public interface INeoStep15Invariant<T> { }

    public sealed class NeoStep15Invariant<T> : INeoStep15Invariant<T> { }

    public static class NeoStep15Test
    {
        public static void NeoStep15ClassAndInterfaceIsinst()
        {
            object value = new NeoStep15Derived();
            if (!(value is NeoStep15Derived)) throw new Exception("Exact IL class isinst failed");
            if (!(value is NeoStep15Base)) throw new Exception("IL base isinst failed");
            if (!(value is INeoStep15Marker)) throw new Exception("IL interface isinst failed");
            if (value is NeoStep15Unrelated) throw new Exception("Unrelated IL isinst succeeded");

            INeoStep15Marker marker = value as INeoStep15Marker;
            if (marker == null) throw new Exception("IL interface as failed");
            if ((value as NeoStep15Unrelated) != null) throw new Exception("Failed as did not return null");
        }

        public static void NeoStep15ClrIsinst()
        {
            object value = new List<int>();
            if (!(value is List<int>)) throw new Exception("Exact CLR isinst failed");
            if (!(value is IEnumerable<int>)) throw new Exception("CLR interface isinst failed");
            if (value is List<long>) throw new Exception("Unrelated CLR isinst succeeded");
            if ((value as IEnumerable<int>) == null) throw new Exception("CLR as failed");
        }

        public static void NeoStep15NullSemantics()
        {
            object value = null;
            if ((value as NeoStep15Base) != null) throw new Exception("Null isinst did not return null");
            NeoStep15Base cast = (NeoStep15Base)value;
            if (cast != null) throw new Exception("Null castclass did not return null");
        }

        public static void NeoStep15CastclassFailure()
        {
            object value = new NeoStep15Unrelated();
            try
            {
                NeoStep15Base ignored = (NeoStep15Base)value;
                if (ignored != null) throw new Exception("Unexpected cast result");
            }
            catch (InvalidCastException)
            {
                return;
            }
            throw new Exception("castclass failure did not throw InvalidCastException");
        }

        public static void NeoStep15BoxedValueTypes()
        {
            object value = 42;
            if (!(value is object)) throw new Exception("Boxed int is object failed");
            if (!(value is int)) throw new Exception("Boxed int exact isinst failed");
            if (value is long) throw new Exception("Boxed int converted during isinst");
            if ((value as IComparable) == null) throw new Exception("Boxed int interface isinst failed");
        }

        public static void NeoStep15BoxedNativeInts()
        {
            // Avoid value-type newobj here: that execution path belongs to Step 18.
            // This test is intentionally scoped to native-int box/unbox marshalling.
            IntPtr signed = IntPtr.Add(IntPtr.Zero, 0x12345678);
            object boxedSigned = signed;
            if (!(boxedSigned is IntPtr)) throw new Exception("Boxed IntPtr type mismatch");
            if ((IntPtr)boxedSigned != signed) throw new Exception("Boxed IntPtr value mismatch");

            UIntPtr unsigned = UIntPtr.Add(UIntPtr.Zero, 0x12345678);
            object boxedUnsigned = unsigned;
            if (!(boxedUnsigned is UIntPtr)) throw new Exception("Boxed UIntPtr type mismatch");
            if ((UIntPtr)boxedUnsigned != unsigned) throw new Exception("Boxed UIntPtr value mismatch");
        }

        public static void NeoStep15GenericBoxPeepholeTrue()
        {
            if (!GenericIsInt<int>(42)) throw new Exception("Generic box/isinst true path failed");
        }

        public static void NeoStep15GenericBoxPeepholeFalse()
        {
            if (GenericIsInt<long>(42L)) throw new Exception("Generic box/isinst false path failed");
        }

        public static void NeoStep15GenericTarget()
        {
            object value = new NeoStep15Derived();
            if (As<NeoStep15Base>(value) == null) throw new Exception("Generic isinst target failed");
            if (As<NeoStep15Unrelated>(value) != null) throw new Exception("Generic isinst target mismatch");
            if (Cast<NeoStep15Base>(value) == null) throw new Exception("Generic castclass target failed");
        }

        public static void NeoStep15Variance()
        {
            object producer = new NeoStep15Producer<NeoStep15Derived>(new NeoStep15Derived());
            var covariant = producer as INeoStep15Producer<NeoStep15Base>;
            if (covariant == null) throw new Exception("IL covariance failed");

            object consumer = new NeoStep15Consumer<NeoStep15Base>();
            var contravariant = consumer as INeoStep15Consumer<NeoStep15Derived>;
            if (contravariant == null) throw new Exception("IL contravariance failed");

            object invariant = new NeoStep15Invariant<NeoStep15Derived>();
            if (invariant is INeoStep15Invariant<NeoStep15Base>)
                throw new Exception("Invariant IL interface was treated as variant");
        }

        public static void NeoStep15ClrVariance()
        {
            object values = new List<string>();
            if (!(values is IEnumerable<object>)) throw new Exception("CLR covariance failed");
        }

        private static bool GenericIsInt<T>(T value)
        {
            return value is int;
        }

        private static T As<T>(object value) where T : class
        {
            return value as T;
        }

        private static T Cast<T>(object value) where T : class
        {
            return (T)value;
        }
    }
}
