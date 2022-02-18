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
        [DisplayName("Port")]
        [Description("调试器网络端口，远端须使用此值调用方法ILRuntime.Runtime.Enviorment.AppDomain.DebugService.StartDebugService()以开启调式服务")]
        public int Port { get { return port; } set { port = value; } }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();
            PortChanged?.Invoke(port);
        }
    }
}