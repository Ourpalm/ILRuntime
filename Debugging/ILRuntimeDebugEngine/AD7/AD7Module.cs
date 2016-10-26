using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7Module : IDebugModule2
    {
        string moduleName;

        public string ModuleName { get { return moduleName; } }
        public AD7Module(string moduleName)
        {
            this.moduleName = moduleName;
        }
        public int GetInfo(enum_MODULE_INFO_FIELDS dwFields, MODULE_INFO[] pinfo)
        {
            pinfo[0].dwValidFields = enum_MODULE_INFO_FIELDS.MIF_NAME;
            pinfo[0].m_bstrName = moduleName;
            return Constants.S_OK;
        }

        public int ReloadSymbols_Deprecated(string pszUrlToSymbols, out string pbstrDebugMessage)
        {
            pbstrDebugMessage = null;
            return Constants.E_NOTIMPL;
        }
    }
}
