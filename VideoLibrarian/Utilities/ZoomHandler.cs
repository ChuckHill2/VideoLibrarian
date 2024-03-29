//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ZoomHandler.cs" company="Chuck Hill">
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// NOT COMPILED, NOT USED. For reference purposes only.

namespace VideoLibrarian
{
    public class ZoomHandler : IDisposable
    {
        private static IntPtr _hActiveHook = IntPtr.Zero;
        private static readonly HookProc _activeHookProc = new HookProc(ActiveHookProc); //this MUST be static so it won't get garbage collected!

        public static int Zoom { get; set; }
        
        public ZoomHandler()
        {
            _hActiveHook = SetWindowsHookEx(WH_CALLWNDPROCRET, _activeHookProc, IntPtr.Zero, GetCurrentThreadId()); //current process, current thread
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hActiveHook);
        }

        #region Control Detection
        //System.Windows.Forms.Application.AddMessageFilter(FormCreateDetector) this doesn't recieve all the window messages!
        //System.Windows.Forms.Application.RemoveMessageFilter(FormCreateDetector);
        #region Win32
        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private const int WM_CREATE = 0x0001;
        private const int WM_DESTROY = 0x0002;

        private const int WH_CALLWNDPROCRET = 12; //gets window management msgs AFTER window processed them
        [StructLayout(LayoutKind.Sequential)]
        private struct CWPRETSTRUCT
        {
            public IntPtr lResult;
            public IntPtr lParam;
            public IntPtr wParam;
            public int message;
            public IntPtr hwnd;
        }

        [DllImport("user32.dll")]   private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll")]   private static extern int UnhookWindowsHookEx(IntPtr idHook);
        [DllImport("user32.dll")]   private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")] private static extern int GetCurrentThreadId();
        #endregion

        private static IntPtr ActiveHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return CallNextHookEx(_hActiveHook, nCode, wParam, lParam);
            CWPRETSTRUCT m = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
            if (m.message == WM_CREATE)
            {
                Control ctrl = Control.FromHandle(m.hwnd);
                if (ctrl is Form)
                {
                    Form f = ctrl as Form;
                    //Translate late as possible because child controls are usually not fully initialized until Form.OnLoad() is complete.
                    //We *could* translate what we have immediately upon WM_CREATE and just subscribe to change events, but this is not 
                    //trivial when the child controls are Infragistics due to the requirement of NOT including Infragistics references 
                    //into this DLL. If we could, it would be a lot more efficient than this late recursion.
                    f.Load += delegate(object sender, EventArgs e)
                    {
                        ResizeControl((Form)sender);
                    };
                }
            }

            if (m.message == WM_DESTROY)
            {
                TranslatedControls.Remove(m.hwnd.ToInt32()); //Cleanup: remove unused window handles.
            }

            return CallNextHookEx(_hActiveHook, nCode, wParam, lParam);
        }
        #endregion

        #region Resize Windows.Forms Controls
        //A control may be a child of many other controls. Keeping the IntPtr handle is a nice way to detect 
        //recursion without interfering with GC. See ActiveHookProc():WM_DESTROY for removal/cleanup.
        private static HashSet<int> TranslatedControls = new HashSet<int>();

        private static void ResizeControl(Control ctrl)
        {
            try
            {
                if (ctrl.IsHandleCreated)
                {
                    if (TranslatedControls.Contains(ctrl.Handle.ToInt32())) return; //already translated!
                    TranslatedControls.Add(ctrl.Handle.ToInt32());
                }

                //Handle when child controls are added/removed
                ctrl.ControlAdded += ctrl_ControlAdded;
                ctrl.ControlRemoved += ctrl_ControlRemoved;
            }
            catch //(Exception ex)
            {
                //LOG.LogWarning(ex, "Error Translating {0}", ctrl.GetType().FullName);
            }

            //Repeat for each child control.
            foreach (Control c in ctrl.Controls)
            {
                ResizeControl(c);
            }        
        }

        private static void RemoveTranslateControl(Control ctrl)
        {
            ctrl.ControlAdded -= ctrl_ControlAdded;
            ctrl.ControlRemoved -= ctrl_ControlRemoved;
            foreach (Control c in ctrl.Controls)
            {
                RemoveTranslateControl(c);
            }        
        }
        private static void ctrl_ControlRemoved(object sender, ControlEventArgs e)
        {
            RemoveTranslateControl(e.Control);
        }
        private static void ctrl_ControlAdded(object sender, ControlEventArgs e)
        {
            ResizeControl(e.Control);
        }
        #endregion
    }
}
