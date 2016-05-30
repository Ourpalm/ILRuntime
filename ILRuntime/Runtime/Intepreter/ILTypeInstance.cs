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

        public List<object> ManagedObjects { get { return managedObjs; } }
        public ILTypeInstance(ILType type)
        {
            this.type = type;
            fields = new StackObject[type.FieldTypes.Length];
            managedObjs = new List<object>(fields.Length);                   
        }

        internal unsafe void PushToStack(int fieldIdx, StackObject* esp, List<object> managedStack)
        {

        }

        unsafe void PushToStackSub(ref StackObject field, int fieldIdx, StackObject* esp, List<object> managedStack)
        {

        }

        internal unsafe void AssignFromStack(int fieldIdx, StackObject* esp, List<object> managedStack)
        {
            AssignFromStackSub(ref fields[fieldIdx], fieldIdx, esp, managedStack);
        }

        unsafe void AssignFromStackSub(ref StackObject field, int fieldIdx, StackObject* esp, List<object> managedStack)
        {
            field = *esp;
            if (field.ObjectType == ObjectTypes.Object)
            {
                managedObjs[fieldIdx] = managedStack[esp->Value];
            }
        }

        public override string ToString()
        {
            return type.FullName;
        }
    }
}
