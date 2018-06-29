using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestCases
{
    public static class GCTest
    {
        static DataManager dataManager;
        public static void Start()
        {
            Console.WriteLine("Start!");
            dataManager = new DataManager();
            dataManager.FillCollections();
        }
        public static void CallGC()
        {
            Console.WriteLine("CallGC!");
            dataManager.CallGC();
        }

        public static void TestDicEnumerator()
        {
            Dictionary<string, int> p3 = new Dictionary<string, int>();
            p3["1"] = 1;
            p3["2"] = 2;
            string key;
            Dictionary<string, int>.Enumerator enumerator = p3.GetEnumerator();
            while (enumerator.MoveNext())
            {
                key = enumerator.Current.Key;
                Console.WriteLine(key);
            }
        }
    }
    class DataManager
    {
        //ArrayCollectionA ca;
        //ArrayCollectionB cb;

        ListCollectionA ca;
        ListCollectionB cb;

        public void FillCollections()
        {
            //cb = new ArrayCollectionB();
            //ca = new ArrayCollectionA(cb);

            cb = new ListCollectionB();
            ca = new ListCollectionA(cb);
        }

        public void CallGC()
        {
            cb = null;
            ca = null;
        }
    }

    public class ArrayCollectionA
    {
        int[] m_array;

        ArrayCollectionB m_tb;

        public ArrayCollectionA(ArrayCollectionB tb)
        {
            m_array = new int[10000000];

            m_tb = tb;
            m_tb.TestFun();
        }
    }

    public class ArrayCollectionB
    {
        int[] m_array;

        public ArrayCollectionB()
        {
            m_array = new int[10000000];
        }

        public void TestFun()
        {
            Console.WriteLine("Fill Start");
        }
    }

    public class ListCollectionA
    {
        List<IntWarp> m_list;
        ListCollectionB lcb;

        public ListCollectionA(ListCollectionB lcb)
        {
            m_list = new List<IntWarp>(1000000);
            this.lcb = lcb;
            lcb.TestFun();
        }

    }

    public class ListCollectionB
    {
        List<IntWarp> m_list;

        public ListCollectionB()
        {
            m_list = new List<IntWarp>(1000000);
        }

        public void TestFun()
        {
            Console.WriteLine("Fill Start");
        }
    }

    public class IntWarp
    {
        public int i;
    }
}
