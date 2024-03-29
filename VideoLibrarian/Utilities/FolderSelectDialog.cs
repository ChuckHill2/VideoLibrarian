//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="FolderSelectDialog.cs" company="Chuck Hill">
// Copyright (c) 2020 Chuck Hill.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1
// of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// The GNU Lesser General Public License can be viewed at
// http://www.opensource.org/licenses/lgpl-license.php. If
// you unfamiliar with this license or have questions about
// it, here is an http://www.gnu.org/licenses/gpl-faq.html.
//
// All code and executables are provided "as is" with no warranty
// either express or implied. The author accepts no liability for
// any damage or loss of business that this product may cause.
// </copyright>
// <repository>https://github.com/ChuckHill2/VideoLibrarian</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
using System;
using System.Reflection;
using System.Windows.Forms;

// ------------------------------------------------------------------
// Wraps System.Windows.Forms.OpenFileDialog to make it present
// a vista-style dialog.
// ------------------------------------------------------------------

namespace VideoLibrarian
{
    /// <summary>
    /// Wraps System.Windows.Forms.OpenFileDialog to make it present
    /// a vista-style dialog.
    /// See: http://www.lyquidity.com/devblog/?p=136
    /// </summary>
    public class FolderSelectDialog : IDisposable
    {
        System.Windows.Forms.OpenFileDialog ofd = null;

        /// <summary>
        /// Shows the Folder select dialog and returns the result.
        /// </summary>
        /// <param name="hWndOwner">Parent control if this dialog or NULL if no owner.</param>
        /// <param name="title">Title of this dialog or "Select a folder" if undefined.</param>
        /// <param name="initialDirectory">The default selected directory or the CurrentWorkingDirectory if undefined.</param>
        /// <returns>user selected directory or null if user cancelled.</returns>
        public static string Show(IWin32Window hWndOwner = null, string title = null, string initialDirectory = null)
        {
            using (var dlg = new FolderSelectDialog())
            {
                dlg.InitialDirectory = initialDirectory;
                dlg.Title = title;
                return (dlg.ShowDialog(hWndOwner) ? dlg.FileName : null);
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public FolderSelectDialog()
        {
            ofd = new System.Windows.Forms.OpenFileDialog();

            ofd.Filter = "Folders|\n";
            ofd.AddExtension = false;
            ofd.CheckFileExists = false;
            ofd.DereferenceLinks = true;
            ofd.Multiselect = false;
        }

        public void Dispose()
        {
            if (ofd == null) return;
            ofd.Dispose();
            ofd = null;
        }

        #region Properties

        /// <summary>
        /// Gets/Sets the initial folder to be selected. A null value selects the current directory.
        /// </summary>
        public string InitialDirectory
        {
            get { return ofd.InitialDirectory; }
            set { ofd.InitialDirectory = value == null || value.Length == 0 ? Environment.CurrentDirectory : value; }
        }

        /// <summary>
        /// Gets/Sets the title to show in the dialog
        /// </summary>
        public string Title
        {
            get { return ofd.Title; }
            set { ofd.Title = value == null ? "Select a folder" : value; }
        }

        /// <summary>
        /// Gets the selected folder
        /// </summary>
        public string FileName
        {
            get { return ofd.FileName; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Shows the dialog
        /// </summary>
        /// <returns>True if the user presses OK else false</returns>
        public bool ShowDialog()
        {
            return ShowDialog(IntPtr.Zero);
        }

        /// <summary>
        /// Shows the dialog
        /// </summary>
        /// <param name="hWndOwner">Handle of the control to be parent</param>
        /// <returns>True if the user presses OK else false</returns>
        public bool ShowDialog(IntPtr hWndOwner)
        {
            return ShowDialog(new WindowWrapper(hWndOwner));
        }
        private class WindowWrapper : System.Windows.Forms.IWin32Window
        {
            private IntPtr _hwnd;
            public WindowWrapper(IntPtr handle) { _hwnd = handle; }
            public IntPtr Handle { get { return _hwnd; } }
        }

        /// <summary>
        /// Shows the dialog
        /// </summary>
        /// <param name="hWndOwner">Handle of the control to be parent</param>
        /// <returns>True if the user presses OK else false</returns>
        public bool ShowDialog(IWin32Window hWndOwner)
        {
            bool flag = false;

            if (Environment.OSVersion.Version.Major >= 6)
            {
                var r = new Reflector("System.Windows.Forms");

                uint num = 0;
                Type typeIFileDialog = r.GetType("FileDialogNative.IFileDialog");
                object dialog = r.Call(ofd, "CreateVistaDialog");
                r.Call(ofd, "OnBeforeVistaDialog", dialog);

                uint options = (uint)r.CallAs(typeof(System.Windows.Forms.FileDialog), ofd, "GetOptions");
                options |= (uint)r.GetEnum("FileDialogNative.FOS", "FOS_PICKFOLDERS");
                r.CallAs(typeIFileDialog, dialog, "SetOptions", options);

                object pfde = r.New("FileDialog.VistaDialogEvents", ofd);
                object[] parameters = new object[] { pfde, num };
                r.CallAs2(typeIFileDialog, dialog, "Advise", parameters);
                num = (uint)parameters[1];
                try
                {
                    int num2 = (int)r.CallAs(typeIFileDialog, dialog, "Show", hWndOwner.Handle);
                    flag = 0 == num2;
                }
                finally
                {
                    r.CallAs(typeIFileDialog, dialog, "Unadvise", num);
                    GC.KeepAlive(pfde);
                }
            }
            else
            {
                var fbd = new FolderBrowserDialog();
                fbd.Description = this.Title;
                fbd.SelectedPath = this.InitialDirectory;
                fbd.ShowNewFolderButton = false;
                if (fbd.ShowDialog(hWndOwner) != DialogResult.OK) return false;
                ofd.FileName = fbd.SelectedPath;
                flag = true;
            }

            return flag;
        }
        #endregion

        /// <summary>
        /// This class is from the Front-End for Dosbox and is used to present a 'vista' dialog box to select folders.
        /// Being able to use a vista style dialog box to select folders is much better then using the shell folder browser.
        /// http://code.google.com/p/fed/
        ///
        /// Example:
        /// var r = new Reflector("System.Windows.Forms");
        /// </summary>
        private class Reflector
        {
            #region variables

            string m_ns;
            Assembly m_asmb;

            #endregion

            #region Constructors

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="ns">The namespace containing types to be used</param>
            public Reflector(string ns)
                : this(ns, ns)
            { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="an">A specific assembly name (used if the assembly name does not tie exactly with the namespace)</param>
            /// <param name="ns">The namespace containing types to be used</param>
            public Reflector(string an, string ns)
            {
                m_ns = ns;
                m_asmb = null;
                foreach (AssemblyName aN in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                {
                    if (aN.FullName.StartsWith(an))
                    {
                        m_asmb = Assembly.Load(aN);
                        break;
                    }
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Return a Type instance for a type 'typeName'
            /// </summary>
            /// <param name="typeName">The name of the type</param>
            /// <returns>A type instance</returns>
            public Type GetType(string typeName)
            {
                Type type = null;
                string[] names = typeName.Split('.');

                if (names.Length > 0)
                    type = m_asmb.GetType(m_ns + "." + names[0]);

                for (int i = 1; i < names.Length; ++i)
                {
                    type = type.GetNestedType(names[i], BindingFlags.NonPublic);
                }
                return type;
            }

            /// <summary>
            /// Create a new object of a named type passing along any params
            /// </summary>
            /// <param name="name">The name of the type to create</param>
            /// <param name="parameters"></param>
            /// <returns>An instantiated type</returns>
            public object New(string name, params object[] parameters)
            {
                Type type = GetType(name);

                ConstructorInfo[] ctorInfos = type.GetConstructors();
                foreach (ConstructorInfo ci in ctorInfos)
                {
                    try
                    {
                        return ci.Invoke(parameters);
                    }
                    catch { }
                }

                return null;
            }

            /// <summary>
            /// Calls method 'func' on object 'obj' passing parameters 'parameters'
            /// </summary>
            /// <param name="obj">The object on which to excute function 'func'</param>
            /// <param name="func">The function to execute</param>
            /// <param name="parameters">The parameters to pass to function 'func'</param>
            /// <returns>The result of the function invocation</returns>
            public object Call(object obj, string func, params object[] parameters)
            {
                return Call2(obj, func, parameters);
            }

            /// <summary>
            /// Calls method 'func' on object 'obj' passing parameters 'parameters'
            /// </summary>
            /// <param name="obj">The object on which to excute function 'func'</param>
            /// <param name="func">The function to execute</param>
            /// <param name="parameters">The parameters to pass to function 'func'</param>
            /// <returns>The result of the function invocation</returns>
            public object Call2(object obj, string func, object[] parameters)
            {
                return CallAs2(obj.GetType(), obj, func, parameters);
            }

            /// <summary>
            /// Calls method 'func' on object 'obj' which is of type 'type' passing parameters 'parameters'
            /// </summary>
            /// <param name="type">The type of 'obj'</param>
            /// <param name="obj">The object on which to excute function 'func'</param>
            /// <param name="func">The function to execute</param>
            /// <param name="parameters">The parameters to pass to function 'func'</param>
            /// <returns>The result of the function invocation</returns>
            public object CallAs(Type type, object obj, string func, params object[] parameters)
            {
                return CallAs2(type, obj, func, parameters);
            }

            /// <summary>
            /// Calls method 'func' on object 'obj' which is of type 'type' passing parameters 'parameters'
            /// </summary>
            /// <param name="type">The type of 'obj'</param>
            /// <param name="obj">The object on which to excute function 'func'</param>
            /// <param name="func">The function to execute</param>
            /// <param name="parameters">The parameters to pass to function 'func'</param>
            /// <returns>The result of the function invocation</returns>
            public object CallAs2(Type type, object obj, string func, object[] parameters)
            {
                MethodInfo methInfo = type.GetMethod(func, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return methInfo.Invoke(obj, parameters);
            }

            /// <summary>
            /// Returns the value of property 'prop' of object 'obj'
            /// </summary>
            /// <param name="obj">The object containing 'prop'</param>
            /// <param name="prop">The property name</param>
            /// <returns>The property value</returns>
            public object Get(object obj, string prop)
            {
                return GetAs(obj.GetType(), obj, prop);
            }

            /// <summary>
            /// Returns the value of property 'prop' of object 'obj' which has type 'type'
            /// </summary>
            /// <param name="type">The type of 'obj'</param>
            /// <param name="obj">The object containing 'prop'</param>
            /// <param name="prop">The property name</param>
            /// <returns>The property value</returns>
            public object GetAs(Type type, object obj, string prop)
            {
                PropertyInfo propInfo = type.GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return propInfo.GetValue(obj, null);
            }

            /// <summary>
            /// Returns an enum value
            /// </summary>
            /// <param name="typeName">The name of enum type</param>
            /// <param name="name">The name of the value</param>
            /// <returns>The enum value</returns>
            public object GetEnum(string typeName, string name)
            {
                Type type = GetType(typeName);
                FieldInfo fieldInfo = type.GetField(name);
                return fieldInfo.GetValue(null);
            }

            #endregion
        }
    }
}
