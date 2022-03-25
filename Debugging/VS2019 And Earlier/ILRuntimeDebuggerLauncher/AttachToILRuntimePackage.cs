//------------------------------------------------------------------------------
// <copyright file="AttachToILRuntimePackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;

namespace ILRuntimeDebuggerLauncher
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#1110", "#1112", "1.0", IconResourceID = 1400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(AttachToILRuntimePackage.PackageGuidString)]
    [ProvideOptionPage(typeof(DebuggerSettingPage), "ILRuntime", "Debugger", 120, 121, true)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class AttachToILRuntimePackage : Package
    {
        /// <summary>
        /// AttachToILRuntimePackage GUID string.
        /// </summary>
        public const string PackageGuidString = "53f442a7-3d74-4f75-bdb1-9dd6cfc8e361";

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachToILRuntime"/> class.
        /// </summary>
        public AttachToILRuntimePackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            var joinableTaskCollection = ThreadHelper.JoinableTaskContext.CreateCollection();
            joinableTaskCollection.DisplayName = GetType().FullName;
            var joinableTaskFactory = ThreadHelper.JoinableTaskContext.CreateFactory(joinableTaskCollection);
            AttachToILRuntime.Initialize(this, joinableTaskFactory);
            base.Initialize();
        }

        #endregion
    }
}
