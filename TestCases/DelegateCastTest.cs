using System;
namespace TestCases
{
    public class CastSubObj
    {
        public int Value;

    }
    public static class CastSubObjExtendMethod
    {

        public static CastSubObj2 AddValue(this CastSubObj cast, DelegateCast<CastSubObj> arg)
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

    public class DelegateCast<T>
    {

    }
    public class DelegateCastTest
    {

        public delegate CastSubObj2 Cast_Func1();
        public delegate CastSubObj2 Cast_Func2(DelegateCast<CastSubObj> a);
        public delegate CastSubObj2 Cast_Func3(DelegateCast<CastSubObj> a);

        public delegate object Cast_Func5(CastSubObj2 A);
        public delegate void Cast_Action4(DelegateCast<CastSubObj> a);


        public static void DelegateCastTest1()
        {
            Cast_Func3 afunc = new CastSubObj().AddValue;




            object obj = afunc;


            Func<DelegateCast<CastSubObj>, CastSubObj> action2 = obj as Func<DelegateCast<CastSubObj>, CastSubObj>;
            Func<DelegateCast<CastSubObj>, CastSubObj2> action3 = obj as Func<DelegateCast<CastSubObj>, CastSubObj2>;
            var action4 = obj as Cast_Func2;
            var action5 = obj as Action<DelegateCast<CastSubObj>, CastSubObj2>;


            if (action2 != null)
            {
                throw new Exception();
            }
            if (action3 == null)
            {
                throw new Exception();
            }
            if (action4 == null)
            {
                throw new Exception();
            }
            if (action5 != null)
            {
                throw new Exception();
            }
        }
        public static void DelegateCastTest2()
        {



            Cast_Func2 afunc = (x) => { return null; };


            object obj = afunc;




            Func<DelegateCast<CastSubObj>, CastSubObj> action2 = obj as Func<DelegateCast<CastSubObj>, CastSubObj>;
            Action<DelegateCast<CastSubObj>, CastSubObj> action3 = obj as Action<DelegateCast<CastSubObj>, CastSubObj>;
            Action action4 = obj as Action;

            var action5 = obj as Action<DelegateCast<CastSubObj>, CastSubObj>;

            var action7 = obj as Func<DelegateCast<CastSubObj>, CastSubObj2>;





            if (action2 != null)
            {
                throw new Exception();
            }
            if (action3 != null)
            {
                throw new Exception();
            }
            if (action4 != null)
            {
                throw new Exception();
            }
            if (action5 != null)
            {
                throw new Exception();
            }

            if (action7 == null)
            {
                throw new Exception();
            }


        }
        [ILRuntimeTest.ILRuntimeTest(IsToDo = true)]
        public static void DelegateCastTest3()
        {
            Cast_Func2 afunc = (x) => { return null; };


            object obj = afunc;

            var action6 = (Action<DelegateCast<CastSubObj>, CastSubObj>)obj;

            if (action6 != null)//TODO:cast后应为空
            {
                throw new Exception();
            }
        }
    }
}
