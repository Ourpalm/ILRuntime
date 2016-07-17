using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Stack;
namespace ILRuntime.Runtime.Intepreter
{
    class ILTypeStaticInstance : ILTypeInstance
    {
        public ILTypeStaticInstance(ILType type)
        {
            this.type = type;
            fields = new StackObject[type.StaticFieldTypes.Length];
            managedObjs = new List<object>(fields.Length);
            for (int i = 0; i < fields.Length; i++)
            {
                managedObjs.Add(null);
            }
            int idx = 0;
            foreach(var i in type.TypeDefinition.Fields)
            {
                if (i.IsStatic)
                {
                    if (i.InitialValue != null)
                    {
                        fields[idx].ObjectType = ObjectTypes.Object;
                        managedObjs[idx] = i.InitialValue;
                    }
                    idx++;
                }
            }
        }
    }

    class ILTypeInstance
    {
        protected ILType type;
        protected StackObject[] fields;
        protected List<object> managedObjs;

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

        public bool IsValueType
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

        protected ILTypeInstance()
        {

        }
        public ILTypeInstance(ILType type)
        {
            this.type = type;
            fields = new StackObject[type.TotalFieldCount];
            managedObjs = new List<object>(fields.Length);
            for (int i = 0; i < fields.Length; i++)
                managedObjs.Add(null);
        }

        internal unsafe void PushFieldAddress(int fieldIdx, StackObject* esp, List<object> managedStack)
        {
            esp->ObjectType = ObjectTypes.FieldReference;
            esp->Value = managedStack.Count;
            managedStack.Add(this);
            esp->ValueLow = fieldIdx;
        }

        internal unsafe void PushToStack(int fieldIdx, StackObject* esp, List<object> managedStack)
        {
            PushToStackSub(ref fields[fieldIdx], fieldIdx, esp, managedStack);
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

        internal unsafe void AssignFromStack(int fieldIdx, StackObject* esp, List<object> managedStack)
        {
            AssignFromStackSub(ref fields[fieldIdx], fieldIdx, esp, managedStack);
        }

        unsafe void AssignFromStackSub(ref StackObject field, int fieldIdx, StackObject* esp, List<object> managedStack)
        {
            field = *esp;
            if (field.ObjectType >= ObjectTypes.Object)
            {
                managedObjs[fieldIdx] = managedStack[esp->Value];
            }
        }

        public override string ToString()
        {
            IMethod m = type.AppDomain.ObjectType.GetMethod("ToString", 0);
            m = type.GetVirtualMethod(m);
            if (m != null)
            {
                var res = type.AppDomain.Invoke(m, this);
                return res.ToString();
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
            return ins;
        }
    }
}
