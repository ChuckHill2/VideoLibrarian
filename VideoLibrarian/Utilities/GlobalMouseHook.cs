//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="GlobalMouseHook.cs" company="Chuck Hill">
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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VideoLibrarian
{
    public class GlobalMouseHook : IDisposable
    {
        private readonly HookProc _hookProc;
        private IntPtr _hHook;
        private GlobalMouseEventArgs prevEvent =  new GlobalMouseEventArgs(0,0,0);
        private Timer timr;
        private Form form;

        /// <summary>
        /// Handle all mouse-move events for the client area of the main application form. 
        /// </summary>
        public event GlobalMouseEventHandler MouseMove;
        /// <summary>
        /// Handle event when mouse moves outside of app window.
        /// </summary>
        public event GlobalMouseEventHandler MouseLeave;

        #region -= Win32 API =-
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private const int WH_MOUSE = 7;
        private const int HTCLIENT = 1;

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEHOOKSTRUCT
        {
            public int X;
            public int Y;
            public IntPtr hwnd;
            public uint wHitTestCode;
            public IntPtr dwExtraInfo; //never used. always zero.
        }

        [DllImport("kernel32.dll")] private static extern int GetCurrentThreadId();
        [DllImport("user32.dll")] private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll")] private static extern bool UnhookWindowsHookEx(IntPtr idHook);
        [DllImport("user32.dll")] private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);
        #endregion -= Win32 API =-

        public GlobalMouseHook()
        {
            _hookProc = new HookProc(this.MouseHookProc);

            _hHook = SetWindowsHookEx(WH_MOUSE, _hookProc, IntPtr.Zero, GetCurrentThreadId());

            form = FormMain.This;
            timr = new Timer();
            timr.Interval = 250;
            timr.Tick += timr_Tick;
//            timr.Start();
        }

        /// <summary>
        /// The scope of this global mouse hook is just this application/thread/main form. We could
        /// change the scope to cover the entire desktop but that cause problems with windows OS.
        /// As it stands, mouse movement is captured ONLY within this main form and it's children. 
        /// This means that when the cursor moves outside the app window, tracking stops and only 
        /// resumes when moves back into the app window. The problem is we have no way to know when 
        /// the cursor moves out of the app or just stops. Since the Forms.Cursor is a system-wide 
        /// resource we can test if the cursor position is within the app window or not with a timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        void timr_Tick(object sender, EventArgs ev)
        {
            const int dMax = 5;
            var pos = Cursor.Position;
            //TODO: Test only visible area, not entire window.
            //if (!form.RectangleToScreen(form.Bounds).Contains(form.PointToScreen(pos)))
            var dx = pos.X - prevEvent.Location.X;
            var dy = pos.Y - prevEvent.Location.Y;
            if (dx < 0) dx = -dx;
            if (dy < 0) dy = -dy;

            if (dx > dMax || dy > dMax)
            {
                if (MouseLeave != null) MouseLeave(form, new GlobalMouseEventArgs(prevEvent.ButtonState, pos.X, pos.Y));
                timr.Stop();
            }
        }

        private IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            IntPtr hHook = _hHook;
            if (nCode < 0) return CallNextHookEx(hHook, nCode, wParam, lParam);

            MOUSEHOOKSTRUCT msg = (MOUSEHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MOUSEHOOKSTRUCT));
            if (msg.wHitTestCode == HTCLIENT) //we only care about the mouse movement in the Main Form client area.
            {
                GlobalMouseEventArgs e = new GlobalMouseEventArgs((MouseButtonState)wParam.ToInt32(),msg.X, msg.Y);
                if (!e.Equals(prevEvent))
                {
                    Control ctrl = Control.FromHandle(msg.hwnd);
                    //Diagnostics.WriteLine("{0}: X={1} Y={2} wParam={3}", ctrl.Name, msg.X, msg.Y, (MouseButtonState)wParam.ToInt32());
                    if (MouseMove != null) MouseMove(ctrl, e);
                    prevEvent = e;
//                    timr.Start();
                }
            }
            else
            {
                GlobalMouseEventArgs e = new GlobalMouseEventArgs((MouseButtonState)wParam.ToInt32(), msg.X, msg.Y);
                if (!e.Equals(prevEvent))
                {
                    if (MouseMove != null) MouseMove(null, e);
                    prevEvent = e;
                }
            }

            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// Cleanup Win32 resources.
        /// </summary>
        public void Dispose()
        {
            if (_hHook == IntPtr.Zero) return;
            UnhookWindowsHookEx(_hHook);
            _hHook = IntPtr.Zero;
            timr.Stop();
            timr.Dispose();
        }
    }

    public enum MouseButtonState
    {
        MOUSEMOVE = 0x200,
        LBUTTONDBLCLK = 0x0203,
        LBUTTONDOWN = 0x0201,
        LBUTTONUP = 0x0202,
        MBUTTONDBLCLK = 0x0209,
        MBUTTONDOWN = 0x0207,
        MBUTTONUP = 0x0208,
        RBUTTONDBLCLK = 0x0206,
        RBUTTONDOWN = 0x0204,
        RBUTTONUP = 0x0205
    }

    public delegate void GlobalMouseEventHandler(object sender, GlobalMouseEventArgs e);

    public class GlobalMouseEventArgs : EventArgs, IEquatable<GlobalMouseEventArgs>
    {
        /// <summary>
        /// Mouse button state. Values same as Win32 WM_ messages.
        /// </summary>
        public MouseButtonState ButtonState { get; private set; }

        /// <summary>
        /// Mouse position in screen coordinates.
        /// </summary>
        public Point Location { get; private set; }

        public GlobalMouseEventArgs(MouseButtonState buttonState, int x, int y)
        {
            ButtonState = buttonState;
            Location = new Point(x, y);
        }

        public override bool Equals(object obj)
        {
            GlobalMouseEventArgs e = (GlobalMouseEventArgs)obj;
            if (e == null) return false;
            return this.Location.X == e.Location.X && this.Location.Y == e.Location.Y && this.ButtonState == e.ButtonState;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(GlobalMouseEventArgs e)
        {
            if (e == null) return false;
            return this.Location.X == e.Location.X && this.Location.Y == e.Location.Y && this.ButtonState == e.ButtonState;
        }
    }
}
