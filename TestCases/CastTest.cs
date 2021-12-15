using System;
using System.Collections.Generic;
using ILRuntimeTest.TestFramework;
namespace TestCases
{
    public class CastSubObj
    {
        public int Value;
       
    }
    public static class CastSubObjExtendMethod
    {
      
        public static CastSubObj2 AddValue(this CastSubObj cast, Cast<CastSubObj> arg)
        {
            Console.WriteLine(arg);
            return new CastSubObj2();
        }
    }
    public class CastSubObj2
    {
        public int Value;
        public void AddValue(int value)
        {
            this.Value += value;
        }
    }

    public class Cast<T>
    {

    }
    public class CastTest
    {

        public delegate CastSubObj2 Cast_Func1();
        delegate CastSubObj2 Cast_Func2(Cast<CastSubObj> a);
        delegate CastSubObj2 Cast_Func3(Cast<CastSubObj> a);
        delegate void Cast_Action4(Cast<CastSubObj> a);
        public delegate object Cast_Func5(CastSubObj2 A);

        public static void CastTest1()
        {
            Cast_Func3 afunc = new CastSubObj().AddValue;

            object obj = afunc;

            Func<Cast<CastSubObj>, CastSubObj> action2 = obj as Func<Cast<CastSubObj>, CastSubObj>;

            Func<Cast<CastSubObj>, CastSubObj2> action3 = obj as Func<Cast<CastSubObj>, CastSubObj2>;
            var action4 = obj as ILRuntimeTest.TestFramework.DelegateCLRTestExtendMethod<Cast<CastSubObj>, CastSubObj2>;
            var action5 = obj as Action<Cast<CastSubObj>, CastSubObj2>;

            Console.WriteLine(action2 != null);
            Console.WriteLine(action3 != null);
            Console.WriteLine(action4 != null);
            Console.WriteLine(action5 != null);

        }

        public static void CastTest2()
        {



            Cast_Action4 afunc = (x) => { };


            object obj = afunc;
         

            Func<Cast<CastSubObj>, CastSubObj> action2 = obj as Func<Cast<CastSubObj>, CastSubObj>;
            Action<Cast<CastSubObj>, CastSubObj> action25 = obj as Action<Cast<CastSubObj>, CastSubObj>;
            Action action6 = obj as Action;

            var action3 = obj as Cast_Action4;

            Console.WriteLine(action2 != null);
            Console.WriteLine(action25 != null);

            Console.WriteLine(action3 != null);
            Console.WriteLine(action6 != null);



        }
        public static void CastTest3()
        {
            var obj = new CastSubObj();
            object f = obj;
            var f2 = f as CastSubObj2;
            var f3 = (CastSubObj2)f;
            var f4 = (CastSubObj)f;

            Console.WriteLine(f2 != null);
            Console.WriteLine(f3 != null);
            Console.WriteLine(f4 != null);

        }
        public static void CastTest4()
        {
            Cast_Func5 afunc = (x) => { return default; };
            object f = afunc;
            var afunc2 = f as Cast_Func5;
            var afunc3 = f as Func<object,int>;
            var afunc4 = f as Func<CastSubObj2, object>;
            Console.WriteLine(afunc2 != null);
            Console.WriteLine(afunc3 != null);
            Console.WriteLine(afunc4 != null);
        }
    }
}
