//------------------------------------------------------------------------------
// <copyright file="AttachToILRuntime.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace ILRuntimeDebuggerLauncher
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class AttachToILRuntime
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
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
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
            FrmLauncher launcher = new ILRuntimeDebuggerLauncher.FrmLauncher();
            if(launcher.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LaunchDebugTarget(launcher.Host);
            }
        }

        private void LaunchDebugTarget(string filePath)
        {
            var debugger = (IVsDebugger4)this.ServiceProvider.GetService(typeof(IVsDebugger));
            VsDebugTargetInfo4[] debugTargets = new VsDebugTargetInfo4[1];
            debugTargets[0].dlo = (uint)DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            debugTargets[0].bstrExe = filePath;
            //debugTargets[0].bstrPortName = "1243";
            //debugTargets[0].guidPortSupplier = new Guid(ILRuntimeDebugEngine.EngineConstants.PortSupplier);
            debugTargets[0].guidLaunchDebugEngine = new Guid(ILRuntimeDebugEngine.EngineConstants.EngineGUID);
            VsDebugTargetProcessInfo[] processInfo = new VsDebugTargetProcessInfo[debugTargets.Length];
            try
            {
                debugger.LaunchDebugTargets4(1, debugTargets, processInfo);
            }
            catch (Exception ex)
            {
                var shell = (IVsUIShell)this.ServiceProvider.GetService(typeof(SVsUIShell));
                string msg;
                shell.GetErrorInfo(out msg);
            }
        }
    }
}
