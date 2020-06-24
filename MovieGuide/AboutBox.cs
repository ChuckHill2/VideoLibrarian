using System;
using System.Reflection;
using System.Windows.Forms;

namespace MovieGuide
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
            this.labelVersion.Text = String.Format("Version {0}   {1:g}", asm.GetName().Version.ToString(), new PeHeader(asm.Location).TimeStamp);
            this.labelCopyright.Text = asm.Attribute<AssemblyCopyrightAttribute>();
            //this.labelCompanyName.Text = asm.Attribute<AssemblyCompanyAttribute>();
            this.labelCompanyName.Text = String.Format("Build: {0}", asm.Attribute<AssemblyConfigurationAttribute>());
            //this.textBoxDescription.Text = asm.Attribute<AssemblyDescriptionAttribute>();
            this.textBoxDescription.Rtf = global::MovieGuide.Properties.Resources.Readme;
        }

    }
}
