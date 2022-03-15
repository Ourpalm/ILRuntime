using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;

using ILRuntime.Runtime.Debugger;
namespace ILRuntimeDebugEngine.AD7
{
    class ILProperty : IDebugProperty2
    {
        AD7Engine engine;
        AD7Thread thread;
        VariableInfo info;
        Dictionary<string, ILProperty> children = new Dictionary<string, ILProperty>();

        public Dictionary<string, ILProperty> Children { get { return children; } }
        public ILProperty Parent { get; set; }
        public VariableReference[] Parameters { get; set; }
        public string Name { get { return info.Name; } set { info.Name = value; } }

        public string FullName
        {
            get
            {
                if (Parent != null)
                {
                    switch (info.Type)
                    {
                        case VariableTypes.FieldReference:
                        case VariableTypes.PropertyReference:
                            return string.Format("{0}.{1}", Parent.FullName, Name);
                        case VariableTypes.IndexAccess:
                            return string.Format("{0}[{1}]", Parent.FullName, Parameters[0].FullName);
                        case VariableTypes.Error:
                        case VariableTypes.NotFound:
                        case VariableTypes.Timeout:
                            return Name;
                        default:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    switch (info.Type)
                    {
                        case VariableTypes.String:
                            return string.Format("\"{0}\"", Name);
                        case VariableTypes.Integer:
                            return info.Offset.ToString();
                        case VariableTypes.Boolean:
                            return (info.Offset == 1).ToString();
                        default:
                            return Name;
                    }
                }
            }
        }
        public ILProperty(AD7Engine engine, AD7Thread thread, VariableInfo info)
        {
            this.engine = engine;
            this.thread = thread;
            this.info = info;
        }
        public int EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter,
            enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout,
            out IEnumDebugPropertyInfo2 ppEnum)
        {
            uint thId;
            thread.GetThreadId(out thId);
            var children = engine.DebuggedProcess.EnumChildren(GetVariableReference(), (int)thId, dwTimeout);
            DEBUG_PROPERTY_INFO[] info = new DEBUG_PROPERTY_INFO[children.Length];
            for (int i = 0; i < children.Length; i++)
            {
                var vi = children[i];
                ILProperty prop = new ILProperty(engine, thread, vi);
                if (vi.Type == VariableTypes.IndexAccess)
                    prop.Parameters = new VariableReference[] { VariableReference.GetInteger(vi.Offset) };
                prop.Parent = this;
                info[i] = prop.GetDebugPropertyInfo(dwFields);
            }
            ppEnum = new AD7PropertyInfoEnum(info);
            return Constants.S_OK;
        }

        public int GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost)
        {
            ppDerivedMost = null;
            return Constants.E_NOTIMPL;
        }

        public int GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo)
        {
            pExtendedInfo = null;
            return Constants.E_NOTIMPL;
        }

        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            ppMemoryBytes = null;
            return Constants.E_NOTIMPL;
        }

        public int GetMemoryContext(out IDebugMemoryContext2 ppMemory)
        {
            ppMemory = null;
            return Constants.E_NOTIMPL;
        }

        public int GetParent(out IDebugProperty2 ppParent)
        {
            ppParent = Parent;
            return Constants.S_OK;
        }

        public int GetPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, uint dwTimeout,
            IDebugReference2[] rgpArgs, uint dwArgCount, DEBUG_PROPERTY_INFO[] pPropertyInfo)
        {
            rgpArgs = null;
            pPropertyInfo[0] = GetDebugPropertyInfo(dwFields);
            return Constants.S_OK;
        }

        public int GetReference(out IDebugReference2 ppReference)
        {
            throw new NotImplementedException();
        }

        public int GetSize(out uint pdwSize)
        {
            throw new NotImplementedException();
        }

        public int SetValueAsReference(IDebugReference2[] rgpArgs, uint dwArgCount, IDebugReference2 pValue,
            uint dwTimeout)
        {
            throw new NotImplementedException();
        }

        public int SetValueAsString(string pszValue, uint dwRadix, uint dwTimeout)
        {
            throw new NotImplementedException();
        }

        internal DEBUG_PROPERTY_INFO GetDebugPropertyInfo(enum_DEBUGPROP_INFO_FLAGS dwFields)
        {
            var propertyInfo = new DEBUG_PROPERTY_INFO();
            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME) != 0)
            {
                propertyInfo.bstrFullName = FullName;
                propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_FULLNAME;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME) != 0)
            {
                propertyInfo.bstrName = info != null ? info.Name : info.Name;
                propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_NAME;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE) != 0)
            {
                propertyInfo.bstrType = info.TypeName;
                propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_TYPE;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE) != 0)
            {
                propertyInfo.bstrValue = info.Value;

                propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_VALUE;
            }

            if ((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB) != 0)
            {
                propertyInfo.dwAttrib = enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_READONLY;
                if (info.Type == VariableTypes.PropertyReference)
                    propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_PROPERTY;
                if(info.Type >= VariableTypes.Error)
                    propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_ERROR;
                if(info.Type == VariableTypes.Timeout)
                    propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_TIMEOUT;
                if (info.ValueType == ValueTypes.String)
                    propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_RAW_STRING;
                if(info.ValueType == ValueTypes.Boolean)
                {
                    if(info.Offset == 1)
                        propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN_TRUE;
                    else
                        propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_VALUE_BOOLEAN;
                }
                if (IsExpandable())
                {
                    propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                }
                if (info.IsPrivate)
                    propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_ACCESS_PRIVATE;
                if(info.IsProtected)
                    propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_ACCESS_PROTECTED;
                propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_ATTRIB;
            }

            if (((dwFields & enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP) != 0) || IsExpandable())
            {
                propertyInfo.pProperty = this;
                propertyInfo.dwFields |= enum_DEBUGPROP_INFO_FLAGS.DEBUGPROP_INFO_PROP;
            }

            return propertyInfo;
        }

        private bool IsExpandable()
        {
            return info.Expandable;
        }

        public VariableReference GetVariableReference()
        {
            if (info != null)
            {
                VariableReference res = new VariableReference();
                res.Address = info.Address;
                res.Name = info.Name;
                res.Type = info.Type;
                res.Offset = info.Offset;
                if (Parent != null)
                    res.Parent = Parent.GetVariableReference();
                res.Parameters = Parameters;

                return res;
            }
            else
                return null;
        }
    }
}
