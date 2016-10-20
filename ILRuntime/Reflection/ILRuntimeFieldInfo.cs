using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

using Mono.Cecil;
using ILRuntime.CLR.Utils;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace ILRuntime.Reflection
{
    class ILRuntimeFieldInfo : FieldInfo
    {
        System.Reflection.FieldAttributes attr = System.Reflection.FieldAttributes.Public;
        ILRuntimeType dType;
        ILType ilType;
        IType fieldType;
        bool isStatic;
        int fieldIdx;
        string name;
        List<object> mStack;
        FieldDefinition definition;
        Runtime.Enviorment.AppDomain appdomain;
        object[] customAttributes;
        Type[] attributeTypes;

        public IType ILFieldType { get { return fieldType; } }

        public ILRuntimeFieldInfo(FieldDefinition def, ILRuntimeType declaredType, bool isStatic, int fieldIdx)
        {
            definition = def;
            this.name = def.Name;
            dType = declaredType;
            ilType = dType.ILType;
            appdomain = ilType.AppDomain;
            this.isStatic = isStatic;
            this.fieldIdx = fieldIdx; 
            if (isStatic)
                attr |= System.Reflection.FieldAttributes.Static;
            fieldType = isStatic ? ilType.StaticFieldTypes[fieldIdx] : ilType.FieldTypes[fieldIdx];
        }

        void InitializeCustomAttribute()
        {
            customAttributes = new object[definition.CustomAttributes.Count];
            attributeTypes = new Type[customAttributes.Length];
            for (int i = 0; i < definition.CustomAttributes.Count; i++)
            {
                var attribute = definition.CustomAttributes[i];
                var at = appdomain.GetType(attribute.AttributeType, null);
                try
                {
                    object ins = attribute.CreateInstance(at, appdomain);

                    attributeTypes[i] = at.ReflectionType;
                    customAttributes[i] = ins;
                }
                catch
                {
                    attributeTypes[i] = typeof(Attribute);
                }
            }
        }
        public override System.Reflection.FieldAttributes Attributes
        {
            get
            {
                return attr;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return dType;
            }
        }

        public override RuntimeFieldHandle FieldHandle
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Type FieldType
        {
            get
            {
                return fieldType.ReflectionType;
            }
        }

        public override string Name
        {
            get
            {
                return name;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return fieldType.ReflectionType;
            }
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (customAttributes == null)
                InitializeCustomAttribute();

            return customAttributes;
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (customAttributes == null)
                InitializeCustomAttribute();
            List<object> res = new List<object>();
            for (int i = 0; i < customAttributes.Length; i++)
            {
                if (attributeTypes[i] == attributeType)
                    res.Add(customAttributes[i]);
            }
            return res.ToArray();
        }

        public override object GetValue(object obj)
        {
            unsafe
            {
                StackObject esp;
                ILTypeInstance ins;
                if (isStatic)
                {
                    ins = ilType.StaticInstance;
                }
                else
                {
                    if (obj is ILTypeInstance)
                        ins = (ILTypeInstance)obj;
                    else
                        ins = ((CrossBindingAdaptorType)obj).ILInstance;
                }
                if (mStack == null)
                    mStack = new List<object>();
                ins.PushToStack(fieldIdx, &esp, ilType.AppDomain, mStack);
                var res = fieldType.TypeForCLR.CheckCLRTypes(ilType.AppDomain, StackObject.ToObject(&esp, ilType.AppDomain, mStack));
                mStack.Clear();
                return res;
            }
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            unsafe
            {
                StackObject esp;
                if (mStack == null)
                    mStack = new List<object>();
                if (value is CrossBindingAdaptorType)
                    value = ((CrossBindingAdaptorType)value).ILInstance;
                ILIntepreter.PushObject(&esp, mStack, value);
                ILTypeInstance ins;
                if (isStatic)
                {
                    ins = ilType.StaticInstance;
                }
                else
                {
                    if (obj is ILTypeInstance)
                        ins = (ILTypeInstance)obj;
                    else
                        ins = ((CrossBindingAdaptorType)obj).ILInstance;
                }
                
                ins.AssignFromStack(fieldIdx, &esp, ilType.AppDomain, mStack);
                mStack.Clear();
            }
        }
    }
}
