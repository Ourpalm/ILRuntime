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
        private static Socket udpSocket;
        private static MemoryStream bufferStream;
        private static BinaryReader bufferReader;
        private static System.Threading.Timer checkRemoteDebugersHealthyTimer;
        //private SocketAsyncEventArgs socketAsyncEventArgs;
        public LauncherForm()
        {
            InitializeComponent();
        }

        public static void StartFetchRemoteDebugger()
        {
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpSocket.Bind(new IPEndPoint(IPAddress.Any, 56000));

            var buffer = new byte[64 * 1024];
            bufferStream = new MemoryStream(buffer);
            bufferReader = new BinaryReader(bufferStream);
            var socketAsyncEventArgs = new SocketAsyncEventArgs();
            socketAsyncEventArgs.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            socketAsyncEventArgs.SetBuffer(buffer, 0, 64 * 1024);
            socketAsyncEventArgs.Completed += SocketAsyncEventArgs_Completed;
            BeginReceive(socketAsyncEventArgs);

            checkRemoteDebugersHealthyTimer = new System.Threading.Timer(CheckRemoteDebugersHealthy, null, 1000, 1000);
        }

        private static void BeginReceive(SocketAsyncEventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                try
                {
                    if (!udpSocket.ReceiveFromAsync(e))
                        SocketAsyncEventArgs_Completed(null, e);
                }
                catch /*(Exception ex)*/
                {
                    SocketAsyncEventArgs_Completed(null, e);
                }
            });
        }

        private static ConcurrentDictionary<Tuple<string, int>, RemoteDebuggerInfo> remoteDebuggerInfoList = new ConcurrentDictionary<Tuple<string, int>, RemoteDebuggerInfo>();
        private static void SocketAsyncEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    bufferStream.Position = 0;
                    var projectName = bufferReader.ReadString();
                    var machineName = bufferReader.ReadString();
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