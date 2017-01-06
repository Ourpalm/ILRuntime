using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
namespace ILRuntime.Runtime.Stack
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct StackObject
    {
        public static StackObject Null = new StackObject() { ObjectType = ObjectTypes.Null, Value = -1, ValueLow = 0 };
        public ObjectTypes ObjectType;
        public int Value;
        public int ValueLow;

        /// <summary>
        /// 根据当前的ESP获取对象
        /// </summary>
        /// <param name="esp">ESP指针</param>
        /// <param name="appdomain">appdomain</param>
        /// <param name="mStack">对象栈</param>
        /// <returns></returns>
        //IL2CPP can't process esp->ToObject() properly, so I can only use static function for this
        public static unsafe object ToObject(StackObject* esp, ILRuntime.Runtime.Enviorment.AppDomain appdomain, List<object> mStack)
        {
            switch (esp->ObjectType)
            {
                case ObjectTypes.Integer:
                    return esp->Value;
                case ObjectTypes.Long:
                    {
                        return *(long*)&esp->Value;
                    }
                case ObjectTypes.Float:
                    {
                        return *(float*)&esp->Value;
                    }
                case ObjectTypes.Double:
                    {
                        return *(double*)&esp->Value;
                    }
                case ObjectTypes.Object:
                    return mStack[esp->Value];
                case ObjectTypes.FieldReference:
                    {
                        ILTypeInstance instance = mStack[esp->Value] as ILTypeInstance;
                        if (instance != null) //判断下是否此类型是 ILTypeInstance如果是的话直接到 ILTypeInstance拿对象
                        {
                            return instance[esp->ValueLow];
                        }
                        else
                        {
                            var obj = mStack[esp->Value];
                            IType t = null;
                            if (obj is CrossBindingAdaptorType) //如果此类型是继承了其他模块的类型
                            {
                                //返回其他模块的类型
                                t = appdomain.GetType(((CrossBindingAdaptor)((CrossBindingAdaptorType)obj).ILInstance.Type.FirstCLRBaseType).BaseCLRType);
                            }
                            else
                                t = appdomain.GetType(obj.GetType());

                            var fi = ((CLRType)t).GetField(esp->ValueLow);
                            return fi.GetValue(obj); //获取值
                        }
                    }
                case ObjectTypes.ArrayReference:
                    {
                        Array instance = mStack[esp->Value] as Array;
                        return instance.GetValue(esp->ValueLow);
                    }
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = appdomain.GetType(esp->Value);
                        if (t is CLR.TypeSystem.ILType)
                        {
                            CLR.TypeSystem.ILType type = (CLR.TypeSystem.ILType)t;
                            return type.StaticInstance[esp->ValueLow];
                        }
                        else
                        {
                            CLR.TypeSystem.CLRType type = (CLR.TypeSystem.CLRType)t;
                            var fi = type.Fields[esp->ValueLow];
                            return fi.GetValue(null);
                        }
                    }
                case ObjectTypes.StackObjectReference:
                    {
                        return ToObject((*(StackObject**)&esp->Value), appdomain, mStack);
                    }
                case ObjectTypes.Null:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }

        public unsafe static void Initialized(ref StackObject esp, int idx, Type t, IType fieldType, List<object> mStack)
        {
            if (t.IsPrimitive)
            {
                if (t == typeof(int) || t == typeof(uint) || t == typeof(short) || t == typeof(ushort) || t == typeof(byte) || t == typeof(sbyte) || t == typeof(char) || t == typeof(bool))
                {
                    esp.ObjectType = ObjectTypes.Integer;
                    esp.Value = 0;
                    esp.ValueLow = 0;
                }
                else if (t == typeof(long) || t == typeof(ulong))
                {
                    esp.ObjectType = ObjectTypes.Long;
                    esp.Value = 0;
                    esp.ValueLow = 0;
                }
                else if (t == typeof(float))
                {
                    esp.ObjectType = ObjectTypes.Float;
                    esp.Value = 0;
                    esp.ValueLow = 0;
                }
                else if (t == typeof(double))
                {
                    esp.ObjectType = ObjectTypes.Double;
                    esp.Value = 0;
                    esp.ValueLow = 0;
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                if (fieldType.IsValueType)
                {
                    esp.ObjectType = ObjectTypes.Object;
                    esp.Value = idx;
                    if (fieldType is CLRType)
                    {
                        mStack[idx] = Activator.CreateInstance(t);
                    }
                    else
                    {
                        mStack[idx] = ((ILType)fieldType).Instantiate();
                    }
                }
                else
                    esp = Null;
            }
        }

        //IL2CPP can't process esp->Initialized() properly, so I can only use static function for this
        public unsafe static void Initialized(StackObject* esp, Type t)
        {
            if (t.IsPrimitive)
            {
                if (t == typeof(int) || t == typeof(uint) || t == typeof(short) || t == typeof(ushort) || t == typeof(byte) || t == typeof(sbyte) || t == typeof(char) || t == typeof(bool))
                {
                    esp->ObjectType = ObjectTypes.Integer;
                    esp->Value = 0;
                    esp->ValueLow = 0;
                }
                else if (t == typeof(long) || t == typeof(ulong))
                {
                    esp->ObjectType = ObjectTypes.Long;
                    esp->Value = 0;
                    esp->ValueLow = 0;
                }
                else if (t == typeof(float))
                {
                    esp->ObjectType = ObjectTypes.Float;
                    esp->Value = 0;
                    esp->ValueLow = 0;
                }
                else if (t == typeof(double))
                {
                    esp->ObjectType = ObjectTypes.Double;
                    esp->Value = 0;
                    esp->ValueLow = 0;
                }
                else
                    throw new NotImplementedException();
            }
            else
            {
                *esp = Null;
            }
        }
    }

    public enum ObjectTypes
    {
        /// <summary>
        /// 空类型
        /// </summary>
        Null,
        /// <summary>
        /// int32
        /// </summary>
        Integer,
        /// <summary>
        /// int64
        /// </summary>
        Long,
        /// <summary>
        /// 浮点数
        /// </summary>
        Float,
        /// <summary>
        /// 双精浮点
        /// </summary>
        Double,
        /// <summary>
        /// 栈对象引用ESP存放了引用对象的指针
        /// </summary>
        StackObjectReference,//Value = pointer, 
        /// <summary>
        /// 静态字段引用 ESP 指向了静态对象表中的数据
        /// </summary>
        StaticFieldReference,
        /// <summary>
        /// 对象，ESP存放了对象栈索引
        /// </summary>
        Object,
        /// <summary>
        /// 字段引用，value存放了所引用对象的索引,ValueLow存放了此字段的索引
        /// </summary>
        FieldReference,//Value = objIdx, ValueLow = fieldIdx
        /// <summary>
        /// 数组引用 value存放了所引用Array对象的索引,ValueLow存放了Arrayd的索引
        /// </summary>
        ArrayReference,//Value = objIdx, ValueLow = elemIdx
    }
}
