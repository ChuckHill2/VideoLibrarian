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
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace VideoLibrarian
{
    public class PleaseWait
    {
        /// <summary>
        /// Display a simple message while executing a time intensive operation.
        /// </summary>
        /// <param name="owner">Parent/owner of this messagebox</param>
        /// <param name="message">text message to display while waiting for the operation to complete</param>
        /// <param name="command">time intensive delegate/method to execute. Must not contain any UI operations</param>
        /// <param name="value">user data to pass to method to execute or null if nothing is required by method</param>
        /// <param name="timeout">Maximum allowed time in seconds time-intensive method is allowed to execute or -1 for no time limit.</param>
        public static void Show(IWin32Window owner, string message, Action<object> command, object value = null, int timeout = -1)
        {
            bool done = false;

            DialogResult clickResult = DialogResult.None;
            MiniMessageBox.MMClicked = m => { done = true; clickResult = m; };
            MiniMessageBox.Show(owner, message, "Please Wait...", MiniMessageBox.Buttons.Abort, MiniMessageBox.Symbol.Wait);
            var th = new Thread(delegate ()
            {
                try { command(value); }
                finally { Volatile.Write(ref done, true); }
            });

            th.IsBackground = false;
            th.Name = "PleaseWait Worker";
            th.Start();
            int endTicks = 0;
            if (timeout > 0) endTicks = Environment.TickCount + timeout * 1000;
            while (!Volatile.Read(ref done))
            {
                if (clickResult != DialogResult.None || (endTicks > 0 && Environment.TickCount > endTicks))
                {
                    th.Abort();
                    break;
                }

                Application.DoEvents();
                //Thread.Sleep(100);
            }

            MiniMessageBox.Hide();

            if (clickResult == DialogResult.Abort)
            {
                Debug.WriteLine("Aborting Application.");
                Environment.Exit(0);
            }
        }
    }
}
