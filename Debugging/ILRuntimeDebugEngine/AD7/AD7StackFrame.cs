using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;

using ILRuntime.Runtime.Debugger;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7StackFrame : IDebugStackFrame2, IDebugExpressionContext2
    {
        public AD7Engine Engine { get; private set; }
        public AD7Thread Thread { get; private set; }
        public StackFrameInfo StackFrameInfo { get; private set; }

        private string _functionName;

        private readonly AD7DocumentContext docContext;

        public AD7StackFrame(AD7Engine engine, AD7Thread thread, StackFrameInfo info)
        {

            Engine = engine;
            this.Thread = thread;
            this.StackFrameInfo = info;

            _functionName = info.MethodName;

            docContext = new AD7DocumentContext(info);

        }

        public int ParseText(string pszCode, enum_PARSEFLAGS dwFlags, uint nRadix, out IDebugExpression2 ppExpr,
            out string pbstrError, out uint pichError)
        {
            pbstrError = "";
            pichError = 0;
            ppExpr = null;
            /*string lookup = pszCode;


            LocalVariable result = ThreadContext.GetVisibleVariableByName(lookup);
            if (result != null)
            {
                ppExpr = new AD7Expression(new MonoProperty(ThreadContext, result));
                return VSConstants.S_OK;
            }
            */
            pbstrError = "Unsupported Expression";
            pichError = (uint)pbstrError.Length;
            return Constants.S_FALSE;
        }

        public int EnumProperties(enum_DEBUGPROP_INFO_FLAGS dwFields, uint nRadix, ref Guid guidFilter, uint dwTimeout,
            out uint pcelt, out IEnumDebugPropertyInfo2 ppEnum)
        {
            pcelt = 0;
            ppEnum = null;
            //ppEnum = new AD7PropertyInfoEnum(LocalVariables.Select(x => x.GetDebugPropertyInfo(dwFields)).ToArray());
            //ppEnum.GetCount(out pcelt);
            return Constants.E_NOTIMPL;
        }

        public int GetCodeContext(out IDebugCodeContext2 ppCodeCxt)
        {
            ppCodeCxt = docContext;
            return Constants.S_OK;
        }

        public int GetDebugProperty(out IDebugProperty2 ppProperty)
        {
            ppProperty = null; // _locals;
            return Constants.S_OK;
        }

        public int GetDocumentContext(out IDebugDocumentContext2 ppCxt)
        {
            ppCxt = docContext;
            return Constants.S_OK;
        }

        public int GetExpressionContext(out IDebugExpressionContext2 ppExprCxt)
        {
            ppExprCxt = this;
            return Constants.S_OK;
        }

        public int GetInfo(enum_FRAMEINFO_FLAGS dwFieldSpec, uint nRadix, FRAMEINFO[] pFrameInfo)
        {
            pFrameInfo[0] = GetFrameInfo(dwFieldSpec);
            return Constants.S_OK;
        }

        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            if (docContext != null)
                return docContext.GetLanguageInfo(ref pbstrLanguage, ref pguidLanguage);
            else
                return Constants.S_OK;
            //pbstrLanguage = AD7Guids.LanguageName;
            //pguidLanguage = AD7Guids.guidLanguageCpp;
            //return VSConstants.S_OK;
        }

        public int GetName(out string pbstrName)
        {
            pbstrName = StackFrameInfo.DocumentName;
            return Constants.S_OK;
        }

        public int GetPhysicalStackRange(out ulong paddrMin, out ulong paddrMax)
        {
            paddrMin = 0;
            paddrMax = 0;
            return Constants.S_OK;
        }

        public int GetThread(out IDebugThread2 ppThread)
        {
            ppThread = Thread;
            return Constants.S_OK;
        }

        internal FRAMEINFO GetFrameInfo(enum_FRAMEINFO_FLAGS dwFieldSpec)
        {
            var frameInfo = new FRAMEINFO();
            frameInfo.m_bstrFuncName = StackFrameInfo.MethodName;
            frameInfo.m_pFrame = this;
            frameInfo.m_fHasDebugInfo = 1;
            frameInfo.m_fStaleCode = 0;

            frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FRAME;
            frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_STALECODE;
            frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_DEBUGINFO;
            if (!string.IsNullOrEmpty(StackFrameInfo.DocumentName))
                frameInfo.m_dwValidFields |= enum_FRAMEINFO_FLAGS.FIF_FUNCNAME;
            return frameInfo;
        }
    }
}
