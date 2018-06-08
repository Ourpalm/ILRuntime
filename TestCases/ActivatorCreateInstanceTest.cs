using System;

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
        public static void ActivatorCreateInstanceWithArgsTest()
        {
            ActivatorCreateInstanceTestClass inst = Activator.CreateInstance<ActivatorCreateInstanceTestClass>();
            Console.WriteLine($"Create without args: {inst}");

            inst = (ActivatorCreateInstanceTestClass)Activator.CreateInstance(typeof(ActivatorCreateInstanceTestClass), 23, "custome value", 3.14f, new ActivatorCreateInstanceTestClass());
            Console.WriteLine($"Create with args 2: {inst}");

            inst = (ActivatorCreateInstanceTestClass)Activator.CreateInstance(typeof(ActivatorCreateInstanceTestClass), 34, "seconde time", 99.9f, new ActivatorCreateInstanceTestClass(-1, "repeat", -1.14f, null));
            Console.WriteLine($"Create with args 3: {inst}");

            inst = (ActivatorCreateInstanceTestClass)Activator.CreateInstance(typeof(ActivatorCreateInstanceTestClass));
            Console.WriteLine($"Create with args 4: {inst}");
        }
    }
}