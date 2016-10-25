using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILRuntimeDebugEngine.AD7
{
    static class Constants
    {
        public const int S_OK = 0;

        public const int S_FALSE = 1;

        public const int E_NOTIMPL = unchecked((int)0x80004001);
        public const int E_FAIL = unchecked((int)0x80004005);
        public const int E_ABORT = unchecked((int)(0x80004004));
        public const int RPC_E_SERVERFAULT = unchecked((int)(0x80010105));
    }
}
