using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ILRuntimeDebuggerLauncher
{
    public class DebuggerSettingPage : DialogPage
    {
        public static event Action<int> PortChanged;

        private int port = 56000;
        [Category("Debugger")]
        [DisplayName("广播监听端口")]
        [Description("调试器监听的广播网络端口")]
        public int Port { get { return port; } set { port = value; } }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();
            PortChanged?.Invoke(port);
        }
    }
}