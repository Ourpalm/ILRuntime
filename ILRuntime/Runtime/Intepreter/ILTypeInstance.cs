using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.Utils;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Stack;
namespace ILRuntime.Runtime.Intepreter
{
    public class ILTypeStaticInstance : ILTypeInstance
    {
        public unsafe ILTypeStaticInstance(ILType type)
        {
            this.type = type;
            fields = new StackObject[type.StaticFieldTypes.Length];
            managedObjs = new List<object>(fields.Length);
            for (int i = 0; i < fields.Length; i++)
            {
                var t = type.StaticFieldTypes[i].TypeForCLR;
                StackObject.Initialized(ref fields[i], t);
                managedObjs.Add(null);
            }
            int idx = 0;
            foreach (var i in type.TypeDefinition.Fields)
            {
                if (i.IsStatic)
                {
                    if (i.InitialValue != null && i.InitialValue.Length > 0)
                    {
                        fields[idx].ObjectType = ObjectTypes.Object;
                        managedObjs[idx] = i.InitialValue;
                    }
                    idx++;
                }
            }
        }
    }

    unsafe class ILEnumTypeInstance : ILTypeInstance
    {
        public ILEnumTypeInstance(ILType type)
        {
            if (!type.IsEnum)
                throw new NotSupportedException();
            this.type = type;
            fields = new StackObject[1];
        }

        public override string ToString()
        {
            var fields = type.TypeDefinition.Fields;
            long longVal = 0;
            int intVal = 0;
            bool isLong = this.fields[0].ObjectType == ObjectTypes.Long;
            if (isLong)
            {
                fixed (StackObject* f = this.fields)
                    longVal = *(long*)&f->Value;
            }
            else
                intVal = this.fields[0].Value;
            for (int i = 0; i < fields.Count; i++)
            {
                var f = fields[i];
                if (f.IsStatic)
                {
                    if (isLong)
                    {
                        long val = f.Constant is long ? (long)f.Constant : (long)(ulong)f.Constant;
                        if (val == longVal)
                            return f.Name;
                    }
                    else
                    {
                        if (f.Constant is int)
                        {
                            if ((int)f.Constant == intVal)
                                return f.Name;
                        }
                        else if (f.Constant is short)
                        {
                            if ((short)f.Constant == intVal)
                                return f.Name;
                        }
                        else if (f.Constant is byte)
                        {
                            if ((byte)f.Constant == intVal)
                                return f.Name;
                        }
                        else
                            throw new NotImplementedException();
                    }
                }
            }
            return isLong ? longVal.ToString() : intVal.ToString();
        }
    }

    public class ILTypeInstance
    {
        protected ILType type;
        protected StackObject[] fields;
        protected List<object> managedObjs;
        object clrInstance;
        Dictionary<ILMethod, IDelegateAdapter> delegates;

        public ILType Type
        {
            get
            {
                return type;
            }
        }

        public StackObject[] Fields
        {
            get { return fields; }
        }

        public virtual bool IsValueType
        {
            get
            {
                return type.IsValueType && !Boxed;
            }
        }

        /// <summary>
        /// 是否已装箱
        /// </summary>
        public bool Boxed { get; set; }

        public List<object> ManagedObjects { get { return managedObjs; } }

        public object CLRInstance { get { return clrInstance; } }

        protected ILTypeInstance()
        {

        }
        public ILTypeInstance(ILType type)
        {
            this.type = type;
            fields = new StackObject[type.TotalFieldCount];
            managedObjs = new List<object>(fields.Length);
            for (int i = 0; i < fields.Length; i++)
            {
                managedObjs.Add(null);
            }
            InitializeFields(type);
            if (type.BaseType is Enviorment.CrossBindingAdaptor)
            {
                clrInstance = ((Enviorment.CrossBindingAdaptor)type.BaseType).CreateCLRInstance(type.AppDomain, this);
            }
            else
            {
                clrInstance = this;
            }

            if (type.Implements != null)
            {
                foreach (var i in type.Implements)
                {
                    if (i is Enviorment.CrossBindingAdaptor)
                    {
                        if (clrInstance != this)//Only one CLRInstance is allowed atm, so implementing multiple interfaces is not supported
                        {
                            throw new NotSupportedException("Inheriting and implementing interface at the same time is not supported yet");
                        }
                        clrInstance = ((Enviorment.CrossBindingAdaptor)i).CreateCLRInstance(type.AppDomain, this);
                        break;
                    }
                }
            }
        }

        void InitializeFields(ILType type)
        {
            for (int i = 0; i < type.FieldTypes.Length; i++)
            {
                StackObject.Initialized(ref fields[type.FieldStartIndex + i], type.FieldTypes[i].TypeForCLR);
            }
            if (type.BaseType != null && type.BaseType is ILType)
                InitializeFields((ILType)type.BaseType);
        }

        internal unsafe void PushFieldAddress(int fieldIdx, StackObject* esp, List<object> managedStack)
        {
            esp->ObjectType = ObjectTypes.FieldReference;
            esp->Value = managedStack.Count;
            managedStack.Add(this);
            esp->ValueLow = fieldIdx;
        }

        internal unsafe void PushToStack(int fieldIdx, StackObject* esp, Enviorment.AppDomain appdomain, List<object> managedStack)
        {
            if (fieldIdx < fields.Length && fieldIdx >= 0)
                PushToStackSub(ref fields[fieldIdx], fieldIdx, esp, managedStack);
            else
            {
                if (Type.BaseType != null && Type.BaseType is Enviorment.CrossBindingAdaptor)
                {
                    CLRType clrType = appdomain.GetType(((Enviorment.CrossBindingAdaptor)Type.BaseType).BaseCLRType) as CLRType;
                    var field = clrType.Fields[fieldIdx];
                    var obj = field.GetValue(clrInstance);
                    ILIntepreter.PushObject(esp, managedStack, obj);
                }
                else
                    throw new TypeLoadException();
            }
        }

        unsafe void PushToStackSub(ref StackObject field, int fieldIdx, StackObject* esp, List<object> managedStack)
        {
            *esp = field;
            if (field.ObjectType >= ObjectTypes.Object)
            {
                esp->Value = managedStack.Count;
                managedStack.Add(managedObjs[fieldIdx]);
            }
        }

        internal void Clear()
        {
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = StackObject.Null;
                managedObjs[i] = null;
            }
        }

        internal unsafe void AssignFromStack(int fieldIdx, StackObject* esp, Enviorment.AppDomain appdomain, List<object> managedStack)
        {
            if (fieldIdx < fields.Length && fieldIdx >= 0)
                AssignFromStackSub(ref fields[fieldIdx], fieldIdx, esp, managedStack);
            else
            {
                if (Type.BaseType != null && Type.BaseType is Enviorment.CrossBindingAdaptor)
                {
                    CLRType clrType = appdomain.GetType(((Enviorment.CrossBindingAdaptor)Type.BaseType).BaseCLRType) as CLRType;
                    var field = clrType.Fields[fieldIdx];
                    field.SetValue(clrInstance, field.FieldType.CheckCLRTypes(appdomain, StackObject.ToObject(esp, appdomain, managedStack)));
                }
                else
                    throw new TypeLoadException();
            }
        }

        unsafe void AssignFromStackSub(ref StackObject field, int fieldIdx, StackObject* esp, List<object> managedStack)
        {
            field = *esp;
            if (field.ObjectType >= ObjectTypes.Object)
            {
                field.Value = fieldIdx;
                managedObjs[fieldIdx] = managedStack[esp->Value];
            }
        }

        public override string ToString()
        {
            IMethod m = type.AppDomain.ObjectType.GetMethod("ToString", 0);
            m = type.GetVirtualMethod(m);
            if (m != null)
            {
                if (m is ILMethod)
                {
                    var res = type.AppDomain.Invoke(m, this, null);
                    return res.ToString();
                }
                else
                    return clrInstance.ToString();
            }
            else
                return type.FullName;
        }

        public bool CanAssignTo(IType type)
        {
            return this.type.CanAssignTo(type);
        }

        public ILTypeInstance Clone()
        {
            ILTypeInstance ins = new ILTypeInstance(type);
            for (int i = 0; i < fields.Length; i++)
            {
                ins.fields[i] = fields[i];
                ins.managedObjs[i] = managedObjs[i];
            }
            ins.clrInstance = clrInstance;
            return ins;
        }

        internal IDelegateAdapter GetDelegateAdapter(ILMethod method)
        {
            if (delegates == null)
                delegates = new Dictionary<ILMethod, IDelegateAdapter>();

            IDelegateAdapter res;
            if (delegates.TryGetValue(method, out res))
                return res;
            return null;
        }

        internal void SetDelegateAdapter(ILMethod method, IDelegateAdapter adapter)
        {
            if (!delegates.ContainsKey(method))
                delegates[method] = adapter;
            else
                throw new NotSupportedException();
        }
    }
}
