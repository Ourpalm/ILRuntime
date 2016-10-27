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
        System.IO.MemoryStream sendStream = new System.IO.MemoryStream(64 * 1024);
        System.IO.BinaryWriter bw;
        DebugSocket socket;
        AD7Engine engine;
        Dictionary<int, AD7PendingBreakPoint> breakpoints = new Dictionary<int, AD7PendingBreakPoint>();
        Dictionary<int, AD7Thread> threads = new Dictionary<int, AD7Thread>();

        public Action OnDisconnected { get; set; }

        public bool Connected { get; set; }

        public bool ConnectFailed { get; set; }

        public bool Connecting { get; set; }
        
        int RemoteDebugVersion { get; set; }

        public DebuggedProcess(AD7Engine engine, string host, int port)
        {
            this.engine = engine;
            bw = new System.IO.BinaryWriter(sendStream);
            socket = new DebugSocket();
            socket.OnConnect = OnConnected;
            socket.OnConnectFailed = OnConnectFailed;
            socket.OnReciveMessage = OnReceiveMessage;
            socket.OnClose = OnClose;
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
            socket.OnClose = null;
            socket.Close();
        }

        void OnClose()
        {
            if (OnDisconnected != null)
                OnDisconnected();
        }

        bool waitingAttach;
        public bool CheckDebugServerVersion()
        {
            socket.Send(DebugMessageType.CSAttach, new byte[0], 0);
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
                        case DebugMessageType.SCAttachResult:
                            {
                                SCAttachResult result = new SCAttachResult();
                                result.Result = (AttachResults)br.ReadByte();
                                result.DebugServerVersion = br.ReadInt32();
                                RemoteDebugVersion = result.DebugServerVersion;
                                waitingAttach = false;
                            }
                            break;
                        case DebugMessageType.SCBindBreakpointResult:
                            {
                                SCBindBreakpointResult msg = new SCBindBreakpointResult();
                                msg.BreakpointHashCode = br.ReadInt32();
                                msg.Result = (BindBreakpointResults)br.ReadByte();
                                OnReceivSendSCBindBreakpointResult(msg);
                            }
                            break;
                        case DebugMessageType.SCBreakpointHit:
                            {
                                SCBreakpointHit msg = new SCBreakpointHit();
                                msg.BreakpointHashCode = br.ReadInt32();
                                msg.ThreadHashCode = br.ReadInt32();
                                OnReceiveSCBreakpointHit(msg);
                            }
                            break;
                        case DebugMessageType.SCThreadStarted:
                            {
                                SCThreadStarted msg = new SCThreadStarted();
                                msg.ThreadHashCode = br.ReadInt32();
                                OnReceiveSCThreadStarted(msg);
                            }
                            break;
                        case DebugMessageType.SCThreadEnded:
                            {
                                SCThreadEnded msg = new SCThreadEnded();
                                msg.ThreadHashCode = br.ReadInt32();
                                OnReceiveSCThreadEnded(msg);
                            }
                            break;
                        case DebugMessageType.SCModuleLoaded:
                            {
                                SCModuleLoaded msg = new SCModuleLoaded();
                                msg.ModuleName = br.ReadString();

                                OnReceiveSCModuleLoaded(msg);
                            }
                            break;
                    }
                }
            }
        }

        public void AddPendingBreakpoint(AD7PendingBreakPoint bp)
        {
            breakpoints[bp.GetHashCode()] = bp;
        }

        public void SendBindBreakpoint(CSBindBreakpoint msg)
        {
            sendStream.Position = 0;
            bw.Write(msg.BreakpointHashCode);
            bw.Write(msg.TypeName);
            bw.Write(msg.MethodName);
            bw.Write(msg.StartLine);
            bw.Write(msg.EndLine);
            socket.Send(DebugMessageType.CSBindBreakpoint, sendStream.GetBuffer(), (int)sendStream.Position);
        }

        void OnReceivSendSCBindBreakpointResult(SCBindBreakpointResult msg)
        {
            AD7PendingBreakPoint bp;
            if(breakpoints.TryGetValue(msg.BreakpointHashCode, out bp))
            {
                bp.Bound(msg.Result);
            }
        }

        void OnReceiveSCModuleLoaded(SCModuleLoaded msg)
        {
            engine.Callback.ModuleLoaded(new AD7Module(msg.ModuleName));

            foreach(var i in breakpoints)
            {
                if (!i.Value.IsBound)
                {
                    i.Value.TryBind();
                }
            }
        }

        void OnReceiveSCBreakpointHit(SCBreakpointHit msg)
        {

        }

        void OnReceiveSCThreadStarted(SCThreadStarted msg)
        {
            AD7Thread t = new AD7Thread(engine, msg.ThreadHashCode);
            threads[msg.ThreadHashCode] = t;
            engine.Callback.ThreadStarted(t);
        }

        void OnReceiveSCThreadEnded(SCThreadEnded msg)
        {
            AD7Thread t;
            if(threads.TryGetValue(msg.ThreadHashCode, out t))
            {
                engine.Callback.ThreadEnded(t);
            }
        }
    }
}
