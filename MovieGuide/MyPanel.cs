//#undef DEBUG

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MovieGuide;

namespace MovieGuide
{
    class MyPanel : Panel
    {
        public MyPanel() : base()
        {
        }

        public event LayoutEventHandler PostLayout;

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e); //let FormLayoutPanel compute positions of all its children
            //Diagnostics.WriteLine("MyPanel.OnLayout: [Call PostLayout event] {0}", ScrollPanel.FormatLayoutEventArgs(e));
            if (PostLayout != null) PostLayout(this, e);
        }
    }
}
