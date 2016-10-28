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
        VariableInfo info;

        public ILProperty(VariableInfo info)
        {
            this.info = info;
        }
        public int EnumChildren(enum_DEBUGPROP_INFO_FLAGS dwFields, uint dwRadix, ref Guid guidFilter,
            enum_DBG_ATTRIB_FLAGS dwAttribFilter, string pszNameFilter, uint dwTimeout,
            out IEnumDebugPropertyInfo2 ppEnum)
        {
            ppEnum = null;
            return Constants.S_OK;
        }

        public int GetDerivedMostProperty(out IDebugProperty2 ppDerivedMost)
        {
            throw new NotImplementedException();
        }

        public int GetExtendedInfo(ref Guid guidExtendedInfo, out object pExtendedInfo)
        {
            throw new NotImplementedException();
        }

        public int GetMemoryBytes(out IDebugMemoryBytes2 ppMemoryBytes)
        {
            throw new NotImplementedException();
        }

        public int GetMemoryContext(out IDebugMemoryContext2 ppMemory)
        {
            throw new NotImplementedException();
        }

        public int GetParent(out IDebugProperty2 ppParent)
        {
            throw new NotImplementedException();
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
                propertyInfo.bstrFullName = info != null ? info.Name : info.Name;
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

                if (IsExpandable())
                {
                    propertyInfo.dwAttrib |= enum_DBG_ATTRIB_FLAGS.DBG_ATTRIB_OBJ_IS_EXPANDABLE;
                }
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
            return false;
        }
    }
}
