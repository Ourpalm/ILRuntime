using System;
using System.Collections.Generic;

namespace TestCases
{
    public class ActivatorCreateInstanceTestClass
    {
        public int IntValue { get; set; }
        public string StringValue { get; set; }
        public float FloatValue { get; set; }
        public ActivatorCreateInstanceTestClass ILValue { get; set; }

        public ActivatorCreateInstanceTestClass()
        {

        }

        public ActivatorCreateInstanceTestClass(int intValue, string stringValue, float floatValue, ActivatorCreateInstanceTestClass ilValue)
        {
            IntValue = intValue;
            StringValue = stringValue;
            FloatValue = floatValue;
            ILValue = ilValue;
        }

        public override string ToString()
        {
            return $"Instance's values: {{{nameof(IntValue)}: {IntValue}, {nameof(StringValue)}: {StringValue}, {nameof(FloatValue)}: {FloatValue}, {nameof(ILValue)}: {(ILValue == null ? "null" : ILValue.ToString())}}}";
        }
    }

    class ActivatorCreateInstanceTest
    {
        private static bool IsDefault<T>(T value)
        {
            return EqualityComparer<T>.Default.Equals(value, default);
        }

        public static void ActivatorCreateInstanceWithArgsTest()
        {
            ActivatorCreateInstanceTestClass inst = Activator.CreateInstance<ActivatorCreateInstanceTestClass>();
            Console.WriteLine($"Create without args: {inst}");

            if (!IsDefault(inst.IntValue))
                throw new Exception(inst.IntValue.ToString());
            if (!IsDefault(inst.FloatValue))
                throw new Exception(inst.FloatValue.ToString());
            if (!IsDefault(inst.StringValue))
                throw new Exception(inst.StringValue);
            if (!IsDefault(inst.ILValue))
                throw new Exception(inst.ILValue.ToString());

            var paramObj = new ActivatorCreateInstanceTestClass();
            inst = (ActivatorCreateInstanceTestClass)Activator.CreateInstance(typeof(ActivatorCreateInstanceTestClass), 23, "custome value", 3.14f, paramObj);
            Console.WriteLine($"Create with args 2: {inst}");

            if (inst.IntValue != 23)
                throw new Exception(inst.IntValue.ToString());
            if (inst.StringValue != "custome value")
                throw new Exception(inst.StringValue.ToString());
            if (Math.Abs(inst.FloatValue - 3.14f) > 0.0001f)
                throw new Exception(inst.StringValue.ToString());
            if (inst.ILValue != paramObj)
                throw new Exception(inst.ILValue.ToString());

            paramObj = new ActivatorCreateInstanceTestClass(-1, "repeat", -1.14f, null);
            inst = (ActivatorCreateInstanceTestClass)Activator.CreateInstance(typeof(ActivatorCreateInstanceTestClass), 34, "seconde time", 99.9f, paramObj);
            Console.WriteLine($"Create with args 3: {inst}");

            if (inst.IntValue != 34)
                throw new Exception(inst.IntValue.ToString());
            if (inst.StringValue != "seconde time")
                throw new Exception(inst.StringValue.ToString());
            if (Math.Abs(inst.FloatValue - 99.9f) > 0.0001f)
                throw new Exception(inst.StringValue.ToString());
            {
                if (inst.ILValue.IntValue != -1)
                    throw new Exception(inst.ILValue.IntValue.ToString());
                if (inst.ILValue.StringValue != "repeat")
                    throw new Exception(inst.ILValue.StringValue.ToString());
                if (Math.Abs(inst.ILValue.FloatValue - (-1.14f)) > 0.0001f)
                    throw new Exception(inst.ILValue.StringValue.ToString());
                if (inst.ILValue.ILValue != null)
                    throw new Exception(inst.ILValue.ILValue.ToString());
            }

            inst = (ActivatorCreateInstanceTestClass)Activator.CreateInstance(typeof(ActivatorCreateInstanceTestClass));
            Console.WriteLine($"Create with args 4: {inst}");

            if (!IsDefault(inst.IntValue))
                throw new Exception(inst.IntValue.ToString());
            if (!IsDefault(inst.FloatValue))
                throw new Exception(inst.FloatValue.ToString());
            if (!IsDefault(inst.StringValue))
                throw new Exception(inst.StringValue);
            if (!IsDefault(inst.ILValue))
                throw new Exception(inst.ILValue.ToString());
        }
    }
}