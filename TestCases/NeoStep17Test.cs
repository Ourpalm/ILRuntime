using System;
using ILRuntimeTest.TestFramework;

namespace TestCases
{
    public static class NeoStep17Test
    {
        class Holder { public int Number; public string Text; public Pair Value; }
        struct Pair { public int Number; public string Text; }
        static int number;
        static string text;
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void Check(bool ok) { if (!ok) throw new Exception("Step17 assertion failed"); }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void Increment(ref int value) { value++; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void Forward(ref int value) { Increment(ref value); }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void Set(out int value) { value = 42; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void Replace(ref string value) { value = "changed"; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void Swap<T>(ref T a, ref T b) { T tmp = a; a = b; b = tmp; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void Change(ref Pair value) { value.Number = 19; value.Text = "pair"; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void ThrowAfterWrite(ref int value) { value = 7; throw new ArgumentException(); }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static ref int Identity(ref int value) { return ref value; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static ref int HeapRef(Holder value) { return ref value.Number; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static ref int ClrHeapRef(NeoStep17CLRHost value) { return ref value.Value.Inner.Number; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void Reset(ref Pair value) { value = default(Pair); }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void Alias(ref int a, ref int b) { a = 17; Check(b == 17); b = 23; }

        public static void NeoStep17FrameAndOut()
        {
            int x = 3; Forward(ref x); Check(x == 4);
            Set(out x); Check(x == 42);
            try { ThrowAfterWrite(ref x); } catch (ArgumentException) { }
            Check(x == 7);
        }
        public static void NeoStep17HeapAndStatic()
        {
            var h = new Holder(); Increment(ref h.Number); Replace(ref h.Text);
            number = 8; Forward(ref number); Replace(ref text);
            Check(h.Number == 1 && h.Text == "changed" && number == 9 && text == "changed");
        }
        public static void NeoStep17ReferenceLifetime()
        {
            string value = "old"; Replace(ref value);
            GC.Collect(); Check(value == "changed");
            string other = "other"; Swap(ref value, ref other);
            Check(value == "other" && other == "changed");
        }
        public static void NeoStep17StructFields()
        {
            Pair pair = default(Pair); Change(ref pair);
            Check(pair.Number == 19 && pair.Text == "pair");
            var holder = new Holder(); Change(ref holder.Value);
            Check(holder.Value.Number == 19 && holder.Value.Text == "pair");
        }
        public static void NeoStep17Arrays()
        {
            var values = new int[2]; Forward(ref values[1]); Check(values[1] == 1);
            var strings = new string[1]; Replace(ref strings[0]); Check(strings[0] == "changed");
            var pairs = new Pair[1]; Change(ref pairs[0]); Check(pairs[0].Text == "pair" && pairs[0].Number == 19);
        }
        public static void NeoStep17GenericStruct()
        {
            Pair a = default(Pair), b = default(Pair); a.Number = 3; a.Text = "a"; b.Number = 5; b.Text = "b";
            Swap(ref a, ref b); Check(a.Number == 5 && a.Text == "b" && b.Number == 3 && b.Text == "a");
        }
        public static void NeoStep17ClrRefOut()
        {
            int n = 4; string s = "old";
            NeoStep17CLRHost.Change(ref n, out s); Check(n == 9 && s == "clr");
            var h = new NeoStep17CLRHost(); Forward(ref h.Number); Replace(ref h.Text);
            Check(h.Number == 1 && h.Text == "changed");
            Forward(ref NeoStep17CLRHost.StaticNumber); Check(NeoStep17CLRHost.StaticNumber == 1);
        }
        public static void NeoStep17ClrNested()
        {
            var h = new NeoStep17CLRHost();
            Forward(ref h.Value.Inner.Number); Replace(ref h.Value.Inner.Text);
            Check(h.Value.Inner.Number == 1 && h.Value.Inner.Text == "changed");
            var values = new NeoStep17Outer[1];
            Forward(ref values[0].Inner.Number); Replace(ref values[0].Inner.Text);
            Check(values[0].Inner.Number == 1 && values[0].Inner.Text == "changed");
        }
        public static void NeoStep17RefReturnAndAlias()
        {
            int value = 1; Identity(ref value) = 6; Check(value == 6);
            Alias(ref value, ref value); Check(value == 23);
            var holder = new Holder(); HeapRef(holder) = 11; Check(holder.Number == 11);
            var clrHolder = new NeoStep17CLRHost(); ClrHeapRef(clrHolder) = 24; Check(clrHolder.Value.Inner.Number == 24);
        }
        public static void NeoStep17InitThroughRef()
        {
            var pairs = new Pair[1]; Change(ref pairs[0]); Reset(ref pairs[0]);
            Check(pairs[0].Number == 0 && pairs[0].Text == null);
        }
        public static void NeoStep17ClrStructRef()
        {
            NeoStep17Inner value = default(NeoStep17Inner);
            NeoStep17CLRHost.ChangeStruct(ref value); Check(value.Number == 21 && value.Text == "struct");
            var values = new NeoStep17Inner[1]; NeoStep17CLRHost.ChangeStruct(ref values[0]);
            Check(values[0].Number == 21 && values[0].Text == "struct");
            string replacement = "replacement";
            Swap(ref value.Text, ref replacement);
            Check(value.Text == "replacement" && replacement == "struct");
        }
        public static void NeoStep17ClrNarrowRefs()
        {
            byte[] b = new byte[2]; b[1] = 77;
            short[] s = new short[2]; s[1] = 123;
            NeoStep17CLRHost.ChangeNarrow(ref b[0], ref s[0]);
            Check(b[0] == 255 && b[1] == 77 && s[0] == -12 && s[1] == 123);
        }
        static void InlineChange(ref Pair pair) { pair.Number = 31; pair.Text = "inline"; }
        public static void NeoStep17InlinedAddressReuse()
        {
            Pair pair = default(Pair); InlineChange(ref pair);
            Check(pair.Number == 31 && pair.Text == "inline");
        }
        public static void NeoStep17ReferenceFieldRead()
        {
            var a = new Holder(); var b = new Holder(); a.Text = "a"; b.Text = "b";
            Swap(ref a.Text, ref b.Text); Check(a.Text == "b" && b.Text == "a");
        }
        public static void NeoStep17ClrWholeFields()
        {
            var h = new NeoStep17CLRHost(); NeoStep17Outer value = default(NeoStep17Outer);
            value.Inner.Number = 18; value.Inner.Text = "whole"; h.Value = value;
            NeoStep17Outer copy = h.Value;
            Check(copy.Inner.Number == 18 && copy.Inner.Text == "whole");
            h.Value.Inner.Number = 25; Check(h.Value.Inner.Number == 25 && copy.Inner.Number == 18);
        }
        public static void NeoStep17RejectDanglingReturn() { NeoStep17CLRHost.CheckDanglingReturn(); }
        public static void NeoStep17OwnerReassignment()
        {
            var holder = new Holder(); var original = holder;
            ref int numberRef = ref holder.Number;
            holder = new Holder(); numberRef = 12;
            Check(original.Number == 12 && holder.Number == 0);
            int[] values = new int[1]; int[] originalArray = values;
            ref int element = ref values[0];
            values = new int[1]; element = 15;
            Check(originalArray[0] == 15 && values[0] == 0);
        }
        public static void NeoStep17LoopReferenceLifetime()
        {
            int fallback = 0;
            ref int saved = ref fallback;
            int[] original = null;
            for (int i = 0; i < 2; i++)
            {
                var values = new int[1];
                ref int current = ref values[0];
                if (i == 0) { saved = ref current; original = values; }
            }
            saved = 37; Check(original[0] == 37 && fallback == 0);
        }
    }
}
