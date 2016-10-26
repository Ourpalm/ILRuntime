using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
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

        /// <summary>
        /// 服务器监听的端口
        /// </summary>
        public int Port { get { return port; } set { this.port = value; } }

        public DebugSocket Client { get { return clientSocket; } }

        public DebuggerServer()
        {
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
                        Console.WriteLine("Client Disconnected");
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
            ClientConnected();
        }

        void ClientConnected()
        {

        }

        void OnReceive(DebugMessageType type, byte[] buffer)
        {
            Console.WriteLine("Received:" + type);
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
            clientSocket.Send(type, sendStream.GetBuffer(), (int)sendStream.Position);
        }

        void TryBindBreakpoint(CSBindBreakpoint msg)
        {
            SCBindBreakpointResult res = new Protocol.SCBindBreakpointResult();
            res.BreakpointHashCode = msg.BreakpointHashCode;
            res.Result = BindBreakpointResults.TypeNotFound;

            SendSCBindBreakpointResult(res);
        }

        void SendSCBindBreakpointResult(SCBindBreakpointResult msg)
        {
            sendStream.Position = 0;
            bw.Write(msg.BreakpointHashCode);
            bw.Write((byte)msg.Result);
            DoSend(DebugMessageType.SCBindBreakpointResult);
        }
    }
}
