//------------------------------------------------------------------------------
// <copyright file="AttachToILRuntime.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;
using ILRuntimeDebugEngine;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace ILRuntimeDebuggerLauncher
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AttachToILRuntime : IVsDebuggerEvents
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        //public const int CommandId = 0x0100;
        public const int AttachToILRuntimeInLANId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("dfb95cf4-1984-41e4-96f9-942063fb4b4c");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        private MenuCommand menuItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachToILRuntime"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private AttachToILRuntime(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            ILRuntimeDebugEngine.AD7.AD7Engine.ShowErrorMessageBoxAction = (title, text) =>
                ShowShellMessageBox(title, text, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_CRITICAL);

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                //AddCommand(commandService, CommandId, this.MenuItemCallback);
                AddCommand(commandService, AttachToILRuntimeInLANId, this.MenuAttachToILRuntimeInLANCallback);
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            var debugger = (IVsDebugger)this.ServiceProvider.GetService(typeof(IVsDebugger));
            if (debugger != null)
                debugger.AdviseDebuggerEvents(this, out _);

            LauncherForm.StartFetchRemoteDebugger(package);
        }

        private void AddCommand(OleMenuCommandService commandService, int commandId, EventHandler menuItemCallback)
        {
            var menuCommandID = new CommandID(CommandSet, commandId);
            menuItem = new MenuCommand(menuItemCallback, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AttachToILRuntime Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        public static JoinableTaskFactory JoinableTaskFactory { get; private set; }
        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package, JoinableTaskFactory joinableTaskFactory)
        {
            JoinableTaskFactory = joinableTaskFactory;
            Instance = new AttachToILRuntime(package);
        }

        ///// <summary>
        ///// This function is the callback used to execute the command when the menu item is clicked.
        ///// See the constructor to see how the menu item is associated with this function using
        ///// OleMenuCommandService service and MenuCommand class.
        ///// </summary>
        ///// <param name="sender">Event sender.</param>
        ///// <param name="e">Event args.</param>
        //private void MenuItemCallback(object sender, EventArgs e)
        //{
        //    FrmLauncher launcher = new FrmLauncher();
        //    if(launcher.ShowDialog() == DialogResult.OK)
        //    {
        //        LaunchDebugTarget(launcher.Host, launcher.Host);
        //    }
        //}

        private void MenuAttachToILRuntimeInLANCallback(object sender, EventArgs e)
        {
            LauncherForm launcher = new LauncherForm();
            if (launcher.ShowDialog() == DialogResult.OK)
            {
                if (launcher.ShowEnterIpAndPortForm)
                {
                    FrmLauncher enterIpAndPortForm = new FrmLauncher();
                    if (enterIpAndPortForm.ShowDialog() == DialogResult.OK)
                        LaunchDebugTarget(enterIpAndPortForm.Host, enterIpAndPortForm.Host);
                }
                else
                    LaunchDebugTarget(launcher.Debugger.Host, $"{launcher.Debugger.ProjectName}({launcher.Debugger.Host})");
            }
        }

        private string currentDebuggerDesc;
        [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<挂起>")]
        private void LaunchDebugTarget(string filePath, string debuggerDesc)
        {       
            var debugger = (IVsDebugger4)this.ServiceProvider.GetService(typeof(IVsDebugger));
            Assumes.Present(debugger);
            ((IVsDebugger)debugger).AdviseDebuggerEvents(this, out _);
            VsDebugTargetInfo4[] debugTargets = new VsDebugTargetInfo4[1];
            debugTargets[0].dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            debugTargets[0].bstrExe = filePath;
            //debugTargets[0].bstrPortName = "1243";
            //debugTargets[0].guidPortSupplier = new Guid(ILRuntimeDebugEngine.EngineConstants.PortSupplier);
            debugTargets[0].guidLaunchDebugEngine = new Guid(EngineConstants.EngineGUID);
            VsDebugTargetProcessInfo[] processInfo = new VsDebugTargetProcessInfo[debugTargets.Length];
            try
            {
                currentDebuggerDesc = debuggerDesc;
                debugger.LaunchDebugTargets4(1, debugTargets, processInfo);
            }
            catch /*(Exception ex)*/
            {
                var shell = (IVsUIShell)this.ServiceProvider.GetService(typeof(SVsUIShell));
                Assumes.Present(shell);
                string msg;
                shell.GetErrorInfo(out msg);
            }
        }

        int IVsDebuggerEvents.OnModeChange(DBGMODE dbgmodeNew)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (menuItem != null)
            {
                menuItem.Enabled = dbgmodeNew == DBGMODE.DBGMODE_Design;
                if (menuItem.Enabled)
                    ClearVsStatusbarText();
                else
                    SetVsStatusbarText($"ILRuntime Debugger:已附加至{currentDebuggerDesc}");
            }
            return VSConstants.S_OK;
        }

        private void SetVsStatusbarText(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsStatusbar statusBar = (IVsStatusbar)ServiceProvider.GetService(typeof(SVsStatusbar));
            Assumes.Present(statusBar);
            int frozen;
            statusBar.IsFrozen(out frozen);
            if (frozen != 0)
                statusBar.FreezeOutput(0);

            statusBar.SetText(text);
            statusBar.FreezeOutput(1);
        }

        private void ClearVsStatusbarText()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsStatusbar statusBar = (IVsStatusbar)ServiceProvider.GetService(typeof(SVsStatusbar));
            Assumes.Present(statusBar);
            statusBar.FreezeOutput(0);
            statusBar.Clear();
        }

        private async void ShowShellMessageBox(string title, string text, OLEMSGBUTTON button, OLEMSGICON icon)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                var VsUiShell = (IVsUIShell)ServiceProvider.GetService(typeof(SVsUIShell));
                Guid tempGuid = Guid.Empty;
                int result = 0;
                if (VsUiShell != null)
                {
                    VsUiShell.ShowMessageBox(0, ref tempGuid, title, text, null, 0,
                        button, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                        icon, 0, out result);
                }
            }
            catch { }
        }
    }
}
