using System;

namespace TestCases
{
    public class NeoStep10Base
    {
        public virtual string Dispatch()
        {
            return "base";
        }
    }

    public class NeoStep10Derived : NeoStep10Base
    {
        public override string Dispatch()
        {
            return "derived";
        }
    }

    public class NeoStep10ChainRoot
    {
        public virtual string Dispatch()
        {
            return "root";
        }
    }

    public class NeoStep10ChainMiddle : NeoStep10ChainRoot
    {
        public override string Dispatch()
        {
            return "middle";
        }
    }

    public class NeoStep10ChainLeaf : NeoStep10ChainMiddle
    {
        public override string Dispatch()
        {
            return "leaf";
        }
    }

    public class NeoStep10ChainLeafInherit : NeoStep10ChainMiddle
    {
    }

    public class NeoStep10ObjectTarget
    {
        private readonly string name;

        public NeoStep10ObjectTarget(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }

    public class NeoStep10NonVirtualTarget
    {
        private readonly string name;

        public NeoStep10NonVirtualTarget(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return name;
        }
    }

    public class NeoStep10Test
    {
        public static void NeoStep10TestSimpleVirtualOverride()
        {
            NeoStep10Base target = new NeoStep10Derived();
            string result = target.Dispatch();
            AssertEqual("NeoStep10TestSimpleVirtualOverride", "derived", result);
        }

        public static void NeoStep10TestMultiLevelVirtualOverride()
        {
            NeoStep10ChainRoot leaf = new NeoStep10ChainLeaf();
            string leafResult = leaf.Dispatch();
            AssertEqual("NeoStep10TestMultiLevelVirtualOverride leaf", "leaf", leafResult);

            NeoStep10ChainRoot inherited = new NeoStep10ChainLeafInherit();
            string inheritedResult = inherited.Dispatch();
            AssertEqual("NeoStep10TestMultiLevelVirtualOverride inherited", "middle", inheritedResult);
        }

        public static void NeoStep10TestClrVirtualToString()
        {
            string target = "clr-to-string";
            string result = target.ToString();
            AssertEqual("NeoStep10TestClrVirtualToString", "clr-to-string", result);
        }

        public static void NeoStep10TestObjectDispatchForILAndCLR()
        {
            object ilObject = new NeoStep10ObjectTarget("il-object");
            string ilResult = ilObject.ToString();
            AssertEqual("NeoStep10TestObjectDispatchForILAndCLR IL", "il-object", ilResult);

            object clrObject = "clr-object";
            string clrResult = clrObject.ToString();
            AssertEqual("NeoStep10TestObjectDispatchForILAndCLR CLR", "clr-object", clrResult);
        }

        public static void NeoStep10TestNonVirtualCallvirtDirectCall()
        {
            var target = new NeoStep10NonVirtualTarget("direct-call");
            string result = target.GetName();
            AssertEqual("NeoStep10TestNonVirtualCallvirtDirectCall", "direct-call", result);
        }

        private static void AssertEqual(string scenario, string expected, string actual)
        {
            if (actual != expected)
            {
                Console.WriteLine(scenario);
                Console.WriteLine(expected);
                Console.WriteLine(actual);
                throw new Exception(scenario);
            }
        }
    }
}
