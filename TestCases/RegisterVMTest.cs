﻿using ILRuntime.Runtime;
using ILRuntimeTest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace TestCases
{
    public class RegisterVMTest
    {
        public static void RegisterVMTest01()
        {
            InlineTest test = new InlineTest();
            test.DoTest(true);
        }

        [ILRuntimeJIT(ILRuntimeJITFlags.NoJIT)]
        public static void RegisterVMTest02()
        {
            LoginData1 data1 = new LoginData1();
            data1.Id = 1;

            for (int i = 0; i < 20; i++)
            {
                var val = data1.GetAttrValue("id");
                if (val == null)
                    throw new Exception();
                Console.WriteLine(val + ":::" + i);
            }
        }

        public static void RegisterVMTest03()
        {
            Bug779.instance.Init();
        }

        [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
        public static void RegisterVMTest04()
        {
            Bug778.instance.Init();
        }
        public class Bug778
        {

            public ILScrollRect2<ScrollItem2> skillView;
            public ILButton2 skillButton;
            Action viewRectEvent;
            public static Bug778 instance = new Bug778();

            /// <summary>
            /// INIT
            /// </summary>
            [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
            public void Init()
            {
                skillView = new ILScrollRect2<ScrollItem2>();
                skillButton = new ILButton2();
                skillButton.onClick = SetSkillFrame;
                //for (int i = 0; i < 20; i++)
                skillButton.SetStateEvent(1);
            }

            [ILRuntimeJIT(ILRuntimeJITFlags.NoJIT)]
            public void SetViewRect(ChangeType2 type = ChangeType2.TopUp,
                                             bool isAnim = false,
                                             bool isJudgeEmpty = true,
                                             Action action = null)
            {
                viewRectEvent = action;
            }

            [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
            public void SetSkillFrame(int state, ILButton2 item)
            {
                skillView.SetViewRect(ChangeType2.TopUp);
            }


        }
        public enum ChangeType2
        {
            TopUp,
        }
        public abstract class ILScrollRect2
        {
            public Action viewRectEvent;
            public abstract void SetViewRect(ChangeType2 type = ChangeType2.TopUp, bool isAnim = false, bool isJudgeEmpty = true, Action action = null);
        }
        [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]

        public class ILScrollRect2<T> : ILScrollRect2 where T : ScrollItem2, new()
        {
            public ILScrollRect2() { }

            //把 Action = null 删除掉  报错停止
            [ILRuntimeJIT(ILRuntimeJITFlags.NoJIT)]
            public override void SetViewRect(ChangeType2 type = ChangeType2.TopUp,
                                             bool isAnim = false,
                                             bool isJudgeEmpty = true,
                                             Action action = null)
            {
                viewRectEvent = action;
            }
        }
        public class TopPetItem2 : ScrollItem2
        {
        }
        public class ScrollItem2 : TabButton2
        {

            public ScrollItem2() { }
            public virtual void Init() { }
            public virtual void OnShow() { }
            public virtual void OnClose() { }

        }
        public class TabButton2 : ILButton2
        {

            public TabButton2() { }
            public TabButton2 InitTab()
            {
                return this;
            }

        }
        [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
        public class ILButton2
        {

            public Action<int, ILButton2> onClick;
            public ILButton2() { }

            public virtual void SetStateEvent(int state)
            {
                onClick?.Invoke(state, this);
            }

        }

        public class SqlData3 { }
        public class ILObject3 { }
        public class ScrollItem3 { }
        public abstract class ILScrollRect3
        {

            public abstract void Clear(bool isClear = true);

        }
        public class ILScrollRect3<T> : ILScrollRect3 where T : ScrollItem3, new()
        {

            public List<T> list;
            public ILScrollRect3(Object transform, bool enableTab = false, bool isClear = true)
            {

                list = new List<T>();

                //重置
                Clear(isClear);


            }

            public override void Clear(bool isClear = true)
            {

                Console.WriteLine("list 在构造方法中 有创建，不可能 为 null, 但是确实为null 下方打印 如果为 true 为null");
                Console.WriteLine(list == null);
                for (int i = list.Count - 1; i >= 0; i--) Del(list[i]);

            }

            public void Del(T item) { }
            public virtual void ToLowerAddSqlDataItem<K, V>(Func<V, string> whereSql, string orderSql, int addCount, int maxCount) where K : SqlData3, new() where V : ILObject3
            {
            }
            public virtual void ToUpperAddSqlDataItem<K, V>(Func<V, string> whereSql, string orderSql, int addCount, int maxCount) where K : SqlData3, new() where V : ILObject3
            {
            }
            public virtual V ToLowerFindListData<V>() where V : ILObject3
            {
                return null;
            }
            public virtual V ToUpperFindListData<V>() where V : ILObject3
            {
                return null;
            }

        }
        public class ILScrollTop3 : ILScrollRect3<ScrollItem3>
        {
            public Action callback;
            public ILScrollTop3(Object transform, Action callback = null) : base(transform, false, true)
            {
                this.callback = callback;
            }

        }

        public class Bug779
        {

            public static Bug779 instance = new Bug779();
            public void Init()
            {

                //获取
                ILScrollTop3 scrollTop = new ILScrollTop3(null);
                scrollTop.Clear();
                return;

            }

        }
        class InlineTest
        {
            int a;

            int A
            {
                [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
                get
                {
                    if (a > 10)
                        return 10;

                    if (a < 2)
                        return 2;

                    return a;
                }
            }
            [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
            public void DoTest(bool arg)
            {
                if (arg)
                {
                    a = 8;
                    var tmp = A;
                    if (tmp != 8)
                        throw new Exception();
                    Console.WriteLine(tmp);
                }
            }
        }

        public class ILSqlite3
        {
            public static ILSqlite3 instance = new ILSqlite3();

            public static object singleLock = new object();

            [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
            public static object GetILSqlData<T>(ILSqlData1<T> data, bool onlyEmpty = false)
            {

                if (data == null) return null;

                //如果是仅判断是否为 null 直接返回一个固定object 这样可以避免很多强转消耗
                if (onlyEmpty == true) return singleLock;
                return data.value;

            }
        }
        public class ILSqlData1<T>
        {
            public T value;
            public ILSqlData1(T value)
            {
                this.value = value;
            }
            public ILSqlData1()
            {
            }
        }
        public class LoginData1
        {

            public ILSqlData1<int> id;
            public int Id
            {
                get
                {
                    if (id == null) return 0;
                    return id.value;
                }
                set
                {
                    if (id == null) id = new ILSqlData1<int>();
                    id.value = value;
                }
            }

            [ILRuntimeJIT(ILRuntimeJITFlags.JITImmediately)]
            public object GetAttrValue(string fieldName)
            {

                switch (fieldName)
                {
                    case "id": return ILSqlite3.GetILSqlData(id);
                }

                return null;

            }
        }
    }
}
