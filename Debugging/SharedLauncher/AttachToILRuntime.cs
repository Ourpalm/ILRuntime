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
        public const int CommandId = 0x0100;

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

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            var debugger = (IVsDebugger)this.ServiceProvider.GetService(typeof(IVsDebugger));
            if (debugger != null)
                debugger.AdviseDebuggerEvents(this, out _);

            LauncherForm.StartFetchRemoteDebugger();
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

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new AttachToILRuntime(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            LauncherForm launcher = new LauncherForm();
            if(launcher.ShowDialog() == DialogResult.OK)
            {
                LaunchDebugTarget(launcher.Debugger);
            }
        }

        private RemoteDebuggerInfo currentDebuggerInfo;
        [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<挂起>")]
        private void LaunchDebugTarget(RemoteDebuggerInfo debuggerInfo)
        {
            if (debuggerInfo == null || string.IsNullOrWhiteSpace(debuggerInfo.Host))
                return;
          
            var debugger = (IVsDebugger4)this.ServiceProvider.GetService(typeof(IVsDebugger));
            ((IVsDebugger)debugger).AdviseDebuggerEvents(this, out _);
            VsDebugTargetInfo4[] debugTargets = new VsDebugTargetInfo4[1];
            debugTargets[0].dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            debugTargets[0].bstrExe = debuggerInfo.Host;
            //debugTargets[0].bstrPortName = "1243";
            //debugTargets[0].guidPortSupplier = new Guid(ILRuntimeDebugEngine.EngineConstants.PortSupplier);
            debugTargets[0].guidLaunchDebugEngine = new Guid(EngineConstants.EngineGUID);
            VsDebugTargetProcessInfo[] processInfo = new VsDebugTargetProcessInfo[debugTargets.Length];
            try
            {
                currentDebuggerInfo = debuggerInfo;
                debugger.LaunchDebugTargets4(1, debugTargets, processInfo);
            }
            catch /*(Exception ex)*/
            {
                var shell = (IVsUIShell)this.ServiceProvider.GetService(typeof(SVsUIShell));
                string msg;
                shell.GetErrorInfo(out msg);
            }
        }

        int IVsDebuggerEvents.OnModeChange(DBGMODE dbgmodeNew)
        {
            if (menuItem != null)
            {
                menuItem.Enabled = dbgmodeNew == DBGMODE.DBGMODE_Design;
                if (menuItem.Enabled)
                    ClearVsStatusbarText();
                else
                    SetVsStatusbarText($"ILRuntime Debugger:已附加至{currentDebuggerInfo.ProjectName}({currentDebuggerInfo.Host})");
            }
            return VSConstants.S_OK;
        }

        private void SetVsStatusbarText(string text)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsStatusbar statusBar = (IVsStatusbar)ServiceProvider.GetService(typeof(SVsStatusbar));

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
            statusBar.FreezeOutput(0);
            statusBar.Clear();
        }
    }
}
