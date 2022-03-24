//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="PleaseWait.cs" company="Chuck Hill">
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
using System.Threading;
using System.Windows.Forms;

namespace VideoLibrarian
{
    public partial class PleaseWait : Form
    {
        private int endTicks = 0;
        private static PleaseWait Worker { get; set; }

        /// <summary>
        /// Executes the specified delegate, on the thread that owns the PleaseWait dialog's underlying window handle, with the specified list of arguments.
        /// Force anonymous method to execute on the same thread as the PleaseWait dialog.
        /// </summary>
        /// <param name="method">A delegate to a method that takes parameters of the same number and type that are contained in the args parameter.</param>
        /// <param name="args">An array of objects to pass as arguments to the specified method. This parameter can be null if the method takes no arguments.</param>
        /// <returns>An System.Object that contains the return value from the delegate being invoked, or null if the delegate has no return value.</returns>
        /// </summary>
        /// <param name="retVal">An System.Object that contains the return value from the delegate being invoked, or null if the delegate has no return value.</param>
        /// <param name="method">A delegate to a method that takes parameters of the same number and type that are contained in the args parameter.</param>
        /// <param name="args">An array of objects to pass as arguments to the specified method. This parameter can be null if the method takes no arguments.</param>
        /// <returns>True if succcessful and retVal contains the returned value, if any., Upon false, the delegate was NOT invoked and retVal == null.</returns>
        public static bool TryInvoke(out object retVal, Delegate method, params object[] args)
        {
            if (Worker != null && Worker.InvokeRequired)
            {
                retVal = Worker.Invoke(method, args);
                return true;
            }

            retVal = null;
            return false;
        }

        /// <summary>
        /// Executes the specified delegate, on the thread that owns the PleaseWait dialog's underlying window handle, with the specified list of arguments.
        /// Force anonymous method to always execute on the same thread as the PleaseWait dialog.
        /// Do not use to call a function within the same function! This will lead to recursion! Use TryInvoke() instead.
        /// </summary>
        /// <param name="method">A delegate to a method that takes parameters of the same number and type that are contained in the args parameter.</param>
        /// <param name="args">An array of objects to pass as arguments to the specified method. This parameter can be null if the method takes no arguments.</param>
        /// <returns>An System.Object that contains the return value from the delegate being invoked, or null if the delegate has no return value.</returns>
        public static object InvokeW(Delegate method, params object[] args)
        {
            if (Worker != null && Worker.InvokeRequired)
                return Worker.Invoke(method, args);
            else return method.DynamicInvoke(args);
        }

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
            Worker = dlg;
            dlg.ShowDialog(owner);
            Worker = null;
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
