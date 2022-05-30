using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ILRuntimeDebuggerLauncher
{
    public partial class LauncherForm : Form
    {
        private static SocketAsyncEventArgs socketAsyncEventArgs;
        private static byte[] buffer = new byte[64 * 1024];
        private static byte[] stringBuffer = new byte[1024];
        private static MemoryStream bufferStream;
        private static BinaryReader bufferReader;
        private static System.Threading.Timer checkRemoteDebugersHealthyTimer;
        
        static LauncherForm()
        {
            bufferStream = new MemoryStream(buffer);
            bufferReader = new BinaryReader(bufferStream);
            checkRemoteDebugersHealthyTimer = new System.Threading.Timer(CheckRemoteDebugersHealthy, null, 1000, 1000);
        }

        public LauncherForm()
        {
            InitializeComponent();
        }

        public static void StartFetchRemoteDebugger(Package package)
        {
            DebuggerSettingPage.PortChanged += (p) => CreateUdpSocketAndBeginReceive(p);

            var port = ((DebuggerSettingPage)package.GetDialogPage(typeof(DebuggerSettingPage))).Port;
            CreateUdpSocketAndBeginReceive(port);
        }

        private static void CreateUdpSocketAndBeginReceive(int port)
        {
            if (socketAsyncEventArgs != null)
            {
                var socket = (Socket)socketAsyncEventArgs.UserToken;
                if (socket != null)
                {
                    try
                    {
                        socket.Close();
                    }
                    catch { }
                }
                socketAsyncEventArgs.UserToken = null;
                socketAsyncEventArgs.Dispose();
                socketAsyncEventArgs = null;
            }

            var udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            try
            {
                udpSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            }
            catch /*(Exception e)*/
            {
                MessageBox.Show($"ILRuntime Debugger:Unable to bind udp port to {port}");
            }

            socketAsyncEventArgs = new SocketAsyncEventArgs();
            socketAsyncEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            socketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);
            socketAsyncEventArgs.Completed += SocketAsyncEventArgs_Completed;
            socketAsyncEventArgs.UserToken = udpSocket;
            BeginReceive(socketAsyncEventArgs);
        }

        private static void BeginReceive(SocketAsyncEventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                try
                {
                    var socket = (Socket)e.UserToken;
                    if (socket == null)
                        return;
                    if (!socket.ReceiveFromAsync(e))
                        SocketAsyncEventArgs_Completed(null, e);
                }
                catch /*(Exception ex)*/
                {
                    SocketAsyncEventArgs_Completed(null, e);
                }
            });
        }

        private static ConcurrentDictionary<Tuple<string, int>, RemoteDebuggerInfo> remoteDebuggerInfoList = new ConcurrentDictionary<Tuple<string, int>, RemoteDebuggerInfo>();
        
        static string ReadUTF8String(BinaryReader br)
        {
            int len = br.ReadInt16();
            br.Read(stringBuffer, 0, len);
            return Encoding.UTF8.GetString(stringBuffer, 0, len);
        }

        private static void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    bufferStream.Position = 0;
                    var projectName = ReadUTF8String(bufferReader);
                    var machineName = ReadUTF8String(bufferReader);
                    var processId = bufferReader.ReadInt32();
                    var port = bufferReader.ReadInt32();
                    var ip = ((IPEndPoint)e.RemoteEndPoint).Address.ToString();
                    var key = new Tuple<string, int>(ip, port);
                    if (!remoteDebuggerInfoList.TryGetValue(key, out var item))
                    {
                        item = new RemoteDebuggerInfo(projectName, machineName, processId, port, ip);
                        remoteDebuggerInfoList[key] = item;
                    }
                    item.Time = DateTime.Now;
                }
            }
            catch { }
            BeginReceive(e);
        }

        private static void CheckRemoteDebugersHealthy(object state)
        {
            var now = DateTime.Now;
            foreach (var item in remoteDebuggerInfoList)
            {
                if ((now - item.Value.Time).TotalSeconds > 1)
                    remoteDebuggerInfoList.TryRemove(item.Key, out _);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RefreshContent();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            RefreshContent();
        }

        private void btn_Refresh_Click(object sender, EventArgs e)
        {
            RefreshContent();
        }

        public RemoteDebuggerInfo Debugger
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                    return (RemoteDebuggerInfo)listView1.SelectedItems[0].Tag;
                return null;
            }
        }

        private void RefreshContent()
        {
            listView1.Items.Clear();
            var listViewItems = new List<ListViewItem>();
            foreach (var item in remoteDebuggerInfoList.Values)
            {
                if (checkBox1.Checked && item.MachineName != Environment.MachineName)
                    continue;
                listViewItems.Add(new ListViewItem(new string[] { item.ProjectName, item.MachineName, item.ProcessId.ToString(), item.Host }) { Tag = item });
            }
            if (listViewItems.Count > 0)
            {
                listView1.Items.AddRange(listViewItems.ToArray());
                listView1.Items[0].Selected = true;
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var hitTestInfo = listView1.HitTest(e.X, e.Y);
            if (hitTestInfo != null && hitTestInfo.Item != null)
            {
                hitTestInfo.Item.Selected = true;
                DialogResult = DialogResult.OK;
            }
        }

        public bool ShowEnterIpAndPortForm { get; private set; }
        private void btn_EnterIpAndPort_Click(object sender, EventArgs e)
        {
            ShowEnterIpAndPortForm = true;
            DialogResult = DialogResult.OK;
        }
    }

    public class RemoteDebuggerInfo
    {
        public string ProjectName { get; private set; }
        public string MachineName { get; private set; }
        public int ProcessId { get; private set; }
        public int Port { get; private set; }
        public string Ip { get; private set; }
        public string Host { get; private set; }
        public DateTime Time { get; set; }

        public RemoteDebuggerInfo(string projectName, string machineName, int processId, int port, string ip)
        {
            ProjectName = projectName;
            MachineName = machineName;
            ProcessId = processId;
            Port = port;
            Ip = ip;
            Host = $"{ip}:{port}";
        }
    }
}