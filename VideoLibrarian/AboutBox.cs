//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="AboutBox.cs" company="Chuck Hill">
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
﻿using System;
using System.Reflection;
using System.Windows.Forms;

namespace VideoLibrarian
{
    partial class AboutBox : Form
    {
        public static new void Show(IWin32Window owner)
        {
            using(var dlg = new AboutBox())
            {
                dlg.ShowDialog(owner);
            }
        }

        private AboutBox()
        {
            InitializeComponent();
            Assembly asm = Assembly.GetExecutingAssembly();

            this.Text = String.Format("About {0}", asm.Attribute<AssemblyProductAttribute>());
            this.labelProductName.Text = asm.Attribute<AssemblyTitleAttribute>();
            this.labelVersion.Text = String.Format("Version {0}   {1:g}", asm.GetName().Version.ToString(), asm.PEtimestamp());
            this.labelCopyright.Text = asm.Attribute<AssemblyCopyrightAttribute>();
            //this.labelCompanyName.Text = asm.Attribute<AssemblyCompanyAttribute>();
            this.labelCompanyName.Text = String.Format("Build: {0}", asm.Attribute<AssemblyConfigurationAttribute>());
            //this.textBoxDescription.Text = asm.Attribute<AssemblyDescriptionAttribute>();
            this.textBoxDescription.Rtf = global::VideoLibrarian.Properties.Resources.Readme;
        }

    }
}
