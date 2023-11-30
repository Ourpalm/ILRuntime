#define TEST_MISSING_METHOD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILRuntime.Other;

namespace ILRuntimeTest.TestFramework
{
    [NeedAdaptor]
    public class TestClass3
    {
        public TestStruct Struct;
        public static string getString(int startIndex = 0, int length = -1)
        {
            throw new Exception();
        }

        public static string getString(ref int startIndex, int length = -1)
        {
            startIndex++;
            return startIndex.ToString() + length;
        }

        public static void setBit(ref byte value, int pos, int bit) { value = (byte)(value & ~(1 << pos) | (bit << pos)); }
    }

    [NeedAdaptor]
    public class TestClass4
    {
        protected int a;
        protected int b;
        protected TestClass2 cls2;

        public virtual void KKK()
        {
            a = 1;
            b = 2;
        }

        public void TestArrayOut(out TestStruct[] arr)
        {
            arr = new TestStruct[10];
        }
    }

    public struct TestStruct
    {
        public static TestStruct instance;
        public Action<int> testField;
        public int value;
        public static void DoTest(ref TestStruct a)
        {
            a.value = 11111;
        }

        public static void DoTest(ref int a)
        {
            a = 22222;
        }

        public static void DoTest2(TestStruct aaa)
        {
            aaa.value = 232425235;
        }

        public static int Add(int a, int b)
        {
            return a + b;
        }
    }
    public class TestHashMap<TKey, TValue>
    {
        private System.Collections.Generic.Dictionary<TKey, TValue> dic;
        public TestHashMap()
        {
            dic = new System.Collections.Generic.Dictionary<TKey, TValue>();
        }
        public TValue this[TKey key]
        {
            get => dic[key];
            set => dic[key] = value;
        }

        public System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dic.GetEnumerator();
        }

        public bool Add(TKey key, TValue value)
        {
            dic.Add(key, value);
            return true;
        }
    }

    public class TestCLRBinding
    {
        public void LoadAsset<T>(string name, T obj)
        {

        }
        public void Emit<T>(T obj)
        {
            LoadAsset("123", obj);
        }

#if TEST_MISSING_METHOD
        public int missingField;
        public void MissingMethodGeneric<T>(T obj)
        {

        }

        public void MissingMethod()
        {

        }

        public static void MissingStaticMethod()
        {

        }
#endif
    }

#if TEST_MISSING_METHOD
    public class MissingType
    {
    }
#endif
}
