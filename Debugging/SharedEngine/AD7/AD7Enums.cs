using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Debugger.Interop;
using System.Runtime.InteropServices;

namespace ILRuntimeDebugEngine.AD7
{
    #region Base Class
    class AD7Enum<T, I> where I : class
    {
        readonly T[] m_data;
        uint m_position;

        public AD7Enum(T[] data)
        {
            m_data = data;
            m_position = 0;
        }

        public int Clone(out I ppEnum)
        {
            ppEnum = null;
            return  Constants.E_NOTIMPL;
        }

        public int GetCount(out uint pcelt)
        {
            pcelt = (uint)m_data.Length;
            return Constants.S_OK;
        }

        public int Next(uint celt, T[] rgelt, out uint celtFetched)
        {
            return Move(celt, rgelt, out celtFetched);
        }

        public int Reset()
        {
            lock (this)
            {
                m_position = 0;

                return Constants.S_OK;
            }
        }

        public int Skip(uint celt)
        {
            uint celtFetched;

            return Move(celt, null, out celtFetched);
        }

        private int Move(uint celt, T[] rgelt, out uint celtFetched)
        {
            lock (this)
            {
                int hr = Constants.S_OK;
                celtFetched = (uint)m_data.Length - m_position;

                if (celt > celtFetched)
                {
                    hr = Constants.S_FALSE;
                }
                else if (celt < celtFetched)
                {
                    celtFetched = celt;
                }

                if (rgelt != null)
                {
                    for (int c = 0; c < celtFetched; c++)
                    {
                        rgelt[c] = m_data[m_position + c];
                    }
                }

                m_position += celtFetched;

                return hr;
            }
        }
    }
    #endregion Base Class
    internal class AD7PropertyInfoEnum : AD7Enum<DEBUG_PROPERTY_INFO, IEnumDebugPropertyInfo2>, IEnumDebugPropertyInfo2
    {
        public AD7PropertyInfoEnum(DEBUG_PROPERTY_INFO[] data)
            : base(data)
        {
        }
    }
    class AD7ThreadEnum : AD7Enum<IDebugThread2, IEnumDebugThreads2>, IEnumDebugThreads2
    {
        public AD7ThreadEnum(IDebugThread2[] data)
            : base(data)
        {
        }

        public int Next(uint celt, IDebugThread2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }
    class AD7FrameInfoEnum : AD7Enum<FRAMEINFO, IEnumDebugFrameInfo2>, IEnumDebugFrameInfo2
    {
        public AD7FrameInfoEnum(FRAMEINFO[] data)
            : base(data)
        {
        }

        public int Next(uint celt, FRAMEINFO[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }
    class AD7ErrorBreakpointsEnum : AD7Enum<IDebugErrorBreakpoint2, IEnumDebugErrorBreakpoints2>, IEnumDebugErrorBreakpoints2
    {
        public AD7ErrorBreakpointsEnum(IDebugErrorBreakpoint2[] breakpoints)
            : base(breakpoints)
        {
        }

        public int Next(uint celt, IDebugErrorBreakpoint2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }
    class AD7BoundBreakpointsEnum : AD7Enum<IDebugBoundBreakpoint2, IEnumDebugBoundBreakpoints2>, IEnumDebugBoundBreakpoints2
    {
        public AD7BoundBreakpointsEnum(IDebugBoundBreakpoint2[] breakpoints)
            : base(breakpoints)
        {
        }

        public int Next(uint celt, IDebugBoundBreakpoint2[] rgelt, ref uint celtFetched)
        {
            return Next(celt, rgelt, out celtFetched);
        }
    }
    class AD7PortEnum : AD7Enum<IDebugPort2, IEnumDebugPorts2>, IEnumDebugPorts2
    {
        public AD7PortEnum(IDebugPort2[] data) : base(data)
        {
        }

        int IEnumDebugPorts2.Next(uint celt, IDebugPort2[] rgelt, ref uint pceltFetched)
        {
            return Next(celt, rgelt, out pceltFetched);
        }
    }

    class AD7ProcessEnum : AD7Enum<IDebugProcess2, IEnumDebugProcesses2>, IEnumDebugProcesses2
    {
        public AD7ProcessEnum(IDebugProcess2[] data) : base(data)
        {
        }

        public int Next(uint celt, IDebugProcess2[] rgelt, ref uint pceltFetched)
        {
            return Next(celt, rgelt, out pceltFetched);
        }
    }
}
