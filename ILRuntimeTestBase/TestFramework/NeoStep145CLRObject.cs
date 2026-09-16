using System;
using System.Runtime.CompilerServices;

namespace ILRuntimeTest.TestFramework
{
    public sealed class NeoStep145CLRObject
    {
        public byte Small { get; }
        public long Wide { get; }
        public short Narrow { get; }
        public double Real { get; }
        public string Text { get; }
        public object Reference { get; }
        public bool Flag { get; }
        public char Character { get; }

        public NeoStep145CLRObject(byte small, long wide, short narrow, double real,
            string text, object reference, bool flag, char character)
        {
            Small = small;
            Wide = wide;
            Narrow = narrow;
            Real = real;
            Text = text;
            Reference = reference;
            Flag = flag;
            Character = character;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public NeoStep145CLRObject(int fail, Exception exception)
        {
            if (fail != 0) throw exception;
        }

    }

}
