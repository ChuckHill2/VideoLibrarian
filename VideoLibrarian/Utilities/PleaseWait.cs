using System;
using System.Threading;
using System.Windows.Forms;

namespace VideoLibrarian
{
    public partial class PleaseWait : Form
    {
        private int endTicks = 0;

        /// <summary>
        /// Display a simple message while executing a time intensive operation.
        /// </summary>
        /// <param name="owner">Parent/owner of this messagebox</param>
        /// <param name="message">text message to display while waiting for the operation to complete</param>
        /// <param name="command">time intensive delegate/method to execute. Must not contain any UI operations</param>
        /// <param name="value">user data to pass to method to execute or null if nothing is required by method</param>
        /// <param name="timeout">Maximum allowed time in seconds time-intensive method is allowed to execute or -1 for no time limit.</param>
        public static void Show(IWin32Window owner, string message, Action<object> command, object value = null, int timeout=-1)
        {
            PleaseWait dlg = new PleaseWait(message, command, value, timeout);
            dlg.ShowDialog(owner);
        }

        private PleaseWait(string message, Action<object> command, object value, int timeout)
        {
            InitializeComponent();
            m_lblMessage.Text = message;
            this.Shown += delegate(object sender, EventArgs e)
            {
                if (command == null) { this.Close(); return; }
                bool done = false;
                var th = new Thread(delegate()
                {
                    try { command(value); }
                    finally { Volatile.Write(ref done, true); }
                });
                th.IsBackground = false;
                th.Name = "PleaseWait Worker";
                th.Start();
                if (timeout>0) endTicks = Environment.TickCount + timeout * 1000;
                while (!Volatile.Read(ref done))
                {
                    if (endTicks>0 && Environment.TickCount>endTicks)
                    {
                        th.Abort();
                        break;
                    }
                    Application.DoEvents();
                    //Thread.Sleep(100);
                }
                this.Close();
            };
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            endTicks = 1;
            base.OnFormClosing(e);
        }
    }
}
