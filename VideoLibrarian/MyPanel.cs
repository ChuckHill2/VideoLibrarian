//#undef DEBUG
using System.Windows.Forms;

namespace VideoLibrarian
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
