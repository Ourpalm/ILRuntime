using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILRuntime.Runtime.Debugger;
using ILRuntime.Runtime.Debugger.Protocol;

namespace ILRuntimeDebugEngine.AD7
{
    class DebuggedProcess
    {
        DebugSocket socket;

        public bool Connected { get; set; }

        public bool ConnectFailed { get; set; }

        public bool Connecting { get; set; }
        
        int RemoteDebugVersion { get; set; }

        public DebuggedProcess(string host, int port)
        {
            socket = new DebugSocket();
            socket.OnConnect = OnConnected;
            socket.OnConnectFailed = OnConnectFailed;
            socket.OnReciveMessage = OnReceiveMessage;
            socket.Connect(host, port);
            Connecting = true;      
        }

        void OnConnected()
        {
            Connected = true;
            Connecting = false;
        }

        void OnConnectFailed()
        {
            ConnectFailed = true;
            Connecting = false;
        }

        public void Close()
        {
            socket.Close();
        }

        bool waitingAttach;
        public bool CheckDebugServerVersion()
        {
            socket.Send(DebugMessageType.Attach, new byte[0]);
            waitingAttach = true;
            DateTime now = DateTime.Now;
            while (waitingAttach)
            {
                System.Threading.Thread.Sleep(1);
                if((DateTime.Now - now).TotalSeconds >= 30 || !Connected)
                {
                    return false;
                }
            }

            return RemoteDebugVersion == DebuggerServer.Version;
        }

        void OnReceiveMessage(DebugMessageType type, byte[] buffer)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer))
            {
                using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
                {
                    switch (type)
                    {
                        case DebugMessageType.AttachResult:
                            {
                                SCAttachResult result = new SCAttachResult();
                                result.Result = (AttachResults)br.ReadByte();
                                result.DebugServerVersion = br.ReadInt32();
                                RemoteDebugVersion = result.DebugServerVersion;
                                waitingAttach = false;
                            }
                            break;
                    }
                }
            }
        }
    }
}
