using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using ILRuntime.Runtime.Debugger;

namespace ILRuntimeDebugEngine.AD7
{
    class AD7DocumentContext : IDebugDocumentContext2, IDebugCodeContext2
    {
        //private AD7MemoryAddress _codeContext;

        public string FileName { get; private set; }
        uint startLine, startColumn, endLine, endColumn;

        public AD7DocumentContext(StackFrameInfo info)//, AD7MemoryAddress codeContext)
        {
            FileName = info.DocumentName;
            startLine = (uint)info.StartLine;
            startColumn = (uint)info.StartColumn;
            endLine = (uint)info.EndLine;
            endColumn = (uint)info.EndColumn;
        }

        #region IDebugDocumentContext2 Members

        // Compares this document context to a given array of document contexts.
        int IDebugDocumentContext2.Compare(enum_DOCCONTEXT_COMPARE Compare, IDebugDocumentContext2[] rgpDocContextSet, uint dwDocContextSetLen, out uint pdwDocContext)
        {
            dwDocContextSetLen = 0;
            pdwDocContext = 0;

            return Constants.E_NOTIMPL;
        }

        // Retrieves a list of all code contexts associated with this document context.
        // The engine sample only supports one code context per document context and 
        // the code contexts are always memory addresses.
        int IDebugDocumentContext2.EnumCodeContexts(out IEnumDebugCodeContexts2 ppEnumCodeCxts)
        {
            ppEnumCodeCxts = null;
            //try
            //{
            //    AD7MemoryAddress[] codeContexts = new AD7MemoryAddress[1];
            //    codeContexts[0] = _codeContext;
            //    ppEnumCodeCxts = new AD7CodeContextEnum(codeContexts);
            //    return Constants.S_OK;
            //}
            //catch (MIException e)
            //{
            //    return e.HResult;
            //}
            //catch (Exception e)
            //{
            //    return EngineUtils.UnexpectedException(e);
            //}
            return Constants.S_FALSE;
        }

        // Gets the document that contains this document context.
        // This method is for those debug engines that supply documents directly to the IDE. Since the sample engine
        // does not do this, this method returns E_NOTIMPL.
        int IDebugDocumentContext2.GetDocument(out IDebugDocument2 ppDocument)
        {
            ppDocument = null;
            return Constants.E_FAIL;

            //ppDocument = null; // new MonoDocument(_pendingBreakpoint);
            //return VSConstants.S_OK;
        }

        // Gets the language associated with this document context.
        public int GetLanguageInfo(ref string pbstrLanguage, ref Guid pguidLanguage)
        {
            pbstrLanguage = "C#";
            pguidLanguage = new Guid("3F5162F8-07C6-11D3-9053-00C04FA302A1");

            return Constants.S_OK;
        }

        // Gets the displayable name of the document that contains this document context.
        int IDebugDocumentContext2.GetName(enum_GETNAME_TYPE gnType, out string pbstrFileName)
        {
            pbstrFileName = FileName;
            return Constants.S_OK;
        }

        // Gets the source code range of this document context.
        // A source range is the entire range of source code, from the current statement back to just after the previous s
        // statement that contributed code. The source range is typically used for mixing source statements, including 
        // comments, with code in the disassembly window.
        // Sincethis engine does not support the disassembly window, this is not implemented.
        int IDebugDocumentContext2.GetSourceRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            throw new NotImplementedException("This method is not implemented");
        }

        // Gets the file statement range of the document context.
        // A statement range is the range of the lines that contributed the code to which this document context refers.
        int IDebugDocumentContext2.GetStatementRange(TEXT_POSITION[] pBegPosition, TEXT_POSITION[] pEndPosition)
        {
            pBegPosition[0].dwColumn = startColumn;
            pBegPosition[0].dwLine = startLine;

            pEndPosition[0].dwColumn = endColumn;
            pEndPosition[0].dwLine = endLine;
            return Constants.S_OK;
        }

        // Moves the document context by a given number of statements or lines.
        // This is used primarily to support the Autos window in discovering the proximity statements around 
        // this document context. 
        int IDebugDocumentContext2.Seek(int nCount, out IDebugDocumentContext2 ppDocContext)
        {
            ppDocContext = null;
            return Constants.E_NOTIMPL;
        }

        #endregion




        public int Add(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            throw new NotImplementedException();
        }

        public int GetDocumentContext(out IDebugDocumentContext2 ppSrcCxt)
        {
            ppSrcCxt = this;
            return Constants.S_OK;
        }

        public int GetInfo(enum_CONTEXT_INFO_FIELDS dwFields, CONTEXT_INFO[] pinfo)
        {
            pinfo[0].dwFields = enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL | enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS;

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL) != 0 && !string.IsNullOrEmpty(FileName))
            {
                pinfo[0].bstrModuleUrl = FileName;
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_MODULEURL;
            }

            if ((dwFields & enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS) != 0)
            {

                pinfo[0].bstrAddress = startLine.ToString();
                pinfo[0].dwFields |= enum_CONTEXT_INFO_FIELDS.CIF_ADDRESS;
            }

            return Constants.S_OK;
        }

        public int Subtract(ulong dwCount, out IDebugMemoryContext2 ppMemCxt)
        {
            throw new NotImplementedException();
        }

        public int Compare(enum_CONTEXT_COMPARE Compare, IDebugMemoryContext2[] rgpMemoryContextSet,
            uint dwMemoryContextSetLen, out uint pdwMemoryContext)
        {
            throw new NotImplementedException();
        }

        // Gets the user-displayable name for this context
        // This is not supported by the sample engine.
        public int GetName(out string pbstrName)
        {
            throw new NotImplementedException();
        }
    }
}
