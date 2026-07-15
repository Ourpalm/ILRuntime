using System;

namespace TestCases
{
    public interface INeoStep11Basic
    {
        int GetValue();
    }

    public interface INeoStep11Second
    {
        string Describe();
    }

    public interface INeoStep11Base
    {
        int Foo();
    }

    public interface INeoStep11Derived : INeoStep11Base
    {
        int Bar();
    }

    public interface INeoStep11Explicit
    {
        int GetValue();
    }

    public class NeoStep11BasicImpl : INeoStep11Basic
    {
        public int GetValue()
        {
            return 42;
        }
    }

    public class NeoStep11MultiImpl : INeoStep11Basic, INeoStep11Second
    {
        public int GetValue()
        {
            return 100;
        }

        public string Describe()
        {
            return "multi";
        }
    }

    public class NeoStep11DerivedImpl : INeoStep11Derived
    {
        public int Foo()
        {
            return 7;
        }

        public int Bar()
        {
            return 13;
        }
    }

    public class NeoStep11ExplicitImpl : INeoStep11Explicit
    {
        public int GetValue()
        {
            return 1;
        }

        int INeoStep11Explicit.GetValue()
        {
            return 999;
        }
    }

    public class NeoStep11InheritedBase : INeoStep11Basic
    {
        public virtual int GetValue()
        {
            return 5;
        }
    }

    public class NeoStep11InheritedDerived : NeoStep11InheritedBase
    {
        public override int GetValue()
        {
            return 50;
        }
    }

    public class NeoStep11Test
    {
        public static void NeoStep11InterfaceBasic()
        {
            INeoStep11Basic target = new NeoStep11BasicImpl();
            int result = target.GetValue();
            AssertEqualInt("NeoStep11InterfaceBasic", 42, result);
        }

        public static void NeoStep11MultipleInterfaces()
        {
            NeoStep11MultiImpl impl = new NeoStep11MultiImpl();
            INeoStep11Basic asBasic = impl;
            INeoStep11Second asSecond = impl;

            int a = asBasic.GetValue();
            string b = asSecond.Describe();

            AssertEqualInt("NeoStep11MultipleInterfaces basic", 100, a);
            AssertEqual("NeoStep11MultipleInterfaces second", "multi", b);
        }

        public static void NeoStep11InterfaceInheritance()
        {
            NeoStep11DerivedImpl impl = new NeoStep11DerivedImpl();

            INeoStep11Base viaBase = impl;
            INeoStep11Derived viaDerived = impl;

            int fooViaBase = viaBase.Foo();
            int fooViaDerived = viaDerived.Foo();
            int barViaDerived = viaDerived.Bar();

            AssertEqualInt("NeoStep11InterfaceInheritance base.Foo", 7, fooViaBase);
            AssertEqualInt("NeoStep11InterfaceInheritance derived.Foo", 7, fooViaDerived);
            AssertEqualInt("NeoStep11InterfaceInheritance derived.Bar", 13, barViaDerived);
        }

        public static void NeoStep11ExplicitImplementation()
        {
            NeoStep11ExplicitImpl impl = new NeoStep11ExplicitImpl();
            int viaDirect = impl.GetValue();
            INeoStep11Explicit viaInterface = impl;
            int viaExplicit = viaInterface.GetValue();

            AssertEqualInt("NeoStep11ExplicitImplementation direct", 1, viaDirect);
            AssertEqualInt("NeoStep11ExplicitImplementation explicit", 999, viaExplicit);
        }

        public static void NeoStep11InheritedInterfaceImpl()
        {
            INeoStep11Basic target = new NeoStep11InheritedDerived();
            int result = target.GetValue();
            AssertEqualInt("NeoStep11InheritedInterfaceImpl", 50, result);
        }

        private static void AssertEqualInt(string scenario, int expected, int actual)
        {
            if (actual != expected)
            {
                Console.WriteLine(scenario);
                Console.WriteLine(expected);
                Console.WriteLine(actual);
                int z = 1;
                int d = 0;
                int _ = z / d;
            }
        }

        private static void AssertEqual(string scenario, string expected, string actual)
        {
            if (actual != expected)
            {
                Console.WriteLine(scenario);
                Console.WriteLine(expected);
                Console.WriteLine(actual);
                int z = 1;
                int d = 0;
                int _ = z / d;
            }
        }
    }
}
