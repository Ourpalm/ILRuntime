using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Debugger.Protocol;

namespace ILRuntime.Runtime.Debugger
{
    public class DebuggerServer
    {
        public const int Version = 1;
        TcpListener listener;
        //HashSet<Session<T>> clients = new HashSet<Session<T>>();
        bool isUp = false;
        int maxNewConnections = 1;
        int port;
        Thread mainLoop;
        DebugSocket clientSocket;
        System.IO.MemoryStream sendStream = new System.IO.MemoryStream(64 * 1024);
        System.IO.BinaryWriter bw;
        DebugService ds;

        /// <summary>
        /// 服务器监听的端口
        /// </summary>
        public int Port { get { return port; } set { this.port = value; } }

        public DebugSocket Client { get { return clientSocket; } }

        public bool IsAttached { get { return clientSocket != null && !clientSocket.Disconnected; } }

        public DebuggerServer(DebugService ds)
        {
            this.ds = ds;
            bw = new System.IO.BinaryWriter(sendStream);
        }

        public virtual bool Start()
        {
            mainLoop = new Thread(new ThreadStart(this.NetworkLoop));
            mainLoop.Start();

            this.listener = new TcpListener(port);
            try { listener.Start(); }
            catch
            {
                return false;
            }
            isUp = true;
            return true;
        }

        public virtual void Stop()
        {
            isUp = false;
            if (this.listener != null)
                this.listener.Stop();
            mainLoop.Abort();
            mainLoop = null;
        }

        void NetworkLoop()
        {
            while (true)
            {
                try
                {
                    // let new clients (max 10) connect
                    if (isUp && clientSocket == null)
                    {
                        for (int i = 0; listener.Pending() && i < maxNewConnections; i++)
                        {
                            CreateNewSession(listener);
                        }
                    }
                    if (clientSocket != null && clientSocket.Disconnected)
                    {
                        clientSocket = null;
                    }
                    System.Threading.Thread.Sleep(1);
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception)
                {
                    
                }
            }
        }

        void CreateNewSession(TcpListener listener)
        {
            Socket sock = listener.AcceptSocket();
            sock.NoDelay = true;
            clientSocket = new DebugSocket(sock);           
            clientSocket.OnReciveMessage = OnReceive;
            clientSocket.OnClose = OnClose;
            ClientConnected();
        }

        void ClientConnected()
        {

        }

        void OnClose()
        {
            ds.Detach();
        }

        void OnReceive(DebugMessageType type, byte[] buffer)
        {
            if (clientSocket == null || clientSocket.Disconnected)
                return;
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer);
            System.IO.BinaryReader br = new System.IO.BinaryReader(ms);

            switch (type)
            {
                case DebugMessageType.CSAttach:
                    {
                        SendAttachResult();
                    }
                    break;
                case DebugMessageType.CSBindBreakpoint:
                    {
                        CSBindBreakpoint msg = new Protocol.CSBindBreakpoint();
                        msg.BreakpointHashCode = br.ReadInt32();
                        msg.TypeName = br.ReadString();
                        msg.MethodName = br.ReadString();
                        msg.StartLine = br.ReadInt32();
                        msg.EndLine = br.ReadInt32();
                        TryBindBreakpoint(msg);
                    }
                    break;
                case DebugMessageType.CSDeleteBreakpoint:
                    {
                        CSDeleteBreakpoint msg = new Protocol.CSDeleteBreakpoint();
                        msg.BreakpointHashCode = br.ReadInt32();
                        ds.DeleteBreakpoint(msg.BreakpointHashCode);
                    }
                    break;
            }

        }

        void SendAttachResult()
        {
            sendStream.Position = 0;
            bw.Write((byte)AttachResults.OK);
            bw.Write(Version);
            DoSend(DebugMessageType.SCAttachResult);
        }

        void DoSend(DebugMessageType type)
        {
            if (clientSocket != null && !clientSocket.Disconnected)
                clientSocket.Send(type, sendStream.GetBuffer(), (int)sendStream.Position);
        }

        void TryBindBreakpoint(CSBindBreakpoint msg)
        {
            var domain = ds.AppDomain;
            SCBindBreakpointResult res = new Protocol.SCBindBreakpointResult();
            res.BreakpointHashCode = msg.BreakpointHashCode;
            IType type;
            if (domain.LoadedTypes.TryGetValue(msg.TypeName, out type))
            {
                if(type is ILType)
                {
                    ILType it = (ILType)type;
                    ILMethod found = null;
                    foreach(var i in it.GetMethods())
                    {
                        if(i.Name == msg.MethodName)
                        {
                            ILMethod ilm = (ILMethod)i;
                            if (ilm.StartLine <= (msg.StartLine + 1) && ilm.EndLine >= (msg.StartLine + 1))
                            {
                                found = ilm;
                                break;
                            }
                        }
                    }
                    if(found != null)
                    {
                        ds.SetBreakPoint(found.GetHashCode(), msg.BreakpointHashCode, msg.StartLine);
                        res.Result = BindBreakpointResults.OK;
                    }
                    else
                    {
                        res.Result = BindBreakpointResults.CodeNotFound;
                    }
                }
                else
                {
                    res.Result = BindBreakpointResults.TypeNotFound;
                }
            }
            else
            {
                res.Result = BindBreakpointResults.TypeNotFound;
            }
            SendSCBindBreakpointResult(res);
        }

        void SendSCBindBreakpointResult(SCBindBreakpointResult msg)
        {
            sendStream.Position = 0;
            bw.Write(msg.BreakpointHashCode);
            bw.Write((byte)msg.Result);
            DoSend(DebugMessageType.SCBindBreakpointResult);
        }

        internal void SendSCBreakpointHit(int intpHash, int bpHash, KeyValuePair<int, StackFrameInfo[]>[] info)
        {
            sendStream.Position = 0;
            bw.Write(bpHash);
            bw.Write(intpHash);
            bw.Write(info.Length);
            foreach(var i in info)
            {
                bw.Write(i.Key);
                bw.Write(i.Value.Length);
                foreach (var j in i.Value)
                {
                    bw.Write(j.MethodName);
                    bw.Write(j.DocumentName);
                    bw.Write(j.StartLine);
                    bw.Write(j.StartColumn);
                    bw.Write(j.EndLine);
                    bw.Write(j.EndColumn);
                    bw.Write(j.LocalVariables.Length);
                    foreach(var k in j.LocalVariables)
                    {
                        bw.Write(k.Name);
                        bw.Write(k.Value);
                        bw.Write(k.TypeName);
                    }
                }
            }
            DoSend(DebugMessageType.SCBreakpointHit);
        }

        internal void SendSCThreadStarted(int threadHash)
        {
            sendStream.Position = 0;
            bw.Write(threadHash);
            DoSend(DebugMessageType.SCThreadStarted);
        }

        internal void SendSCThreadEnded(int threadHash)
        {
            sendStream.Position = 0;
            bw.Write(threadHash);
            DoSend(DebugMessageType.SCThreadEnded);
        }

        public void NotifyModuleLoaded(string modulename)
        {
            sendStream.Position = 0;
            bw.Write(modulename);
            DoSend(DebugMessageType.SCModuleLoaded);
        }
    }
}
