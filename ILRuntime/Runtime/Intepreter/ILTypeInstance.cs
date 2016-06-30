using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Stack;
namespace ILRuntime.Runtime.Intepreter
{
    class ILTypeInstance
    {
        ILType type;
        StackObject[] fields;
        List<object> managedObjs;

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
                return type.IsValueType;
            }
        }

        public List<object> ManagedObjects { get { return managedObjs; } }
        public ILTypeInstance(ILType type)
        {
            this.type = type;
            fields = new StackObject[type.FieldTypes.Length];
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
            return type.FullName;
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
