//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="MiniMessageBox.cs" company="Chuck Hill">
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
// <repository>https://github.com/ChuckHill2/ChuckHill2.Utilities</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Lifted from ChuckHill2,Utilities.Forms
// Depends on GDI.ExtractAssociatedIcon() and GDI.ApplyShadows() only.

namespace VideoLibrarian
{
    /// <summary>
    /// @image{inline} html MiniMessageBox.png
    /// Displays a small message box in front of the specified object and with the specified text, caption, buttons, and icon.
    /// </summary>
    /// <remarks>
    /// Functionally equivalant to MessageBoxEx except it is much smaller and more discreet.
    /// Plus it may be used modalless (i.e. returns immediately).
    /// </remarks>
    public class MiniMessageBox : Form
    {
        /// <summary>
        /// Response buttons to display on the messagebox. Includes the standard <see cref="MessageBoxButtons"/> with the ability to pick and choose any combination of individual buttons to to display.
        /// </summary>
        [Flags]
        public enum Buttons
        {
            /// <summary>
            /// No buttons will be displayed.  However if a modal popup is displayed this will default to an OK buton.
            /// </summary>
            None = 0,
            /// <summary>
            /// Display an OK button. Upon click DialogResult.OK will be returned.
            /// </summary>
            OK = 0x01,
            /// <summary>
            /// Display a Cancel button. Upon click DialogResult.Cancel will be returned.
            /// </summary>
            Cancel = 0x02,
            /// <summary>
            /// Display an Abort button. Upon click DialogResult.Abort will be returned.
            /// </summary>
            Abort = 0x04,
            /// <summary>
            /// Display a Retry button. Upon click DialogResult.Retry will be returned.
            /// </summary>
            Retry = 0x08,
            /// <summary>
            /// Display an Ignore button. Upon click DialogResult.Ignore will be returned.
            /// </summary>
            Ignore = 0x10,
            /// <summary>
            /// Display a Yes button. Upon click DialogResult.Yes will be returned.
            /// </summary>
            Yes = 0x20,
            /// <summary>
            /// Display a No button. Upon click DialogResult.No will be returned.
            /// </summary>
            No = 0x40,
            /// <summary>
            /// Display a combination of OK and Cancel buttons.
            /// </summary>
            OKCancel = (OK | Cancel),
            /// <summary>
            /// Display a combination of Abort, Retry, and Ignore buttons.
            /// </summary>
            AbortRetryIgnore = (Abort | Retry | Ignore),
            /// <summary>
            /// Display a combination of Yes, No, and Cancel buttons.
            /// </summary>
            YesNoCancel = (Yes | No | Cancel),
            /// <summary>
            /// Display a combination of Yes and No buttons.
            /// </summary>
            YesNo = (Yes | No),
            /// <summary>
            /// Display a combination of Retry and Cancel buttons.
            /// </summary>
            RetryCancel = (Retry | Cancel)
        }

        /// <summary>
        /// Specifies which graphic symbol to display along with the message. This mirrors <see cref="MessageBoxIcon" /> but with extra symbols.
        /// </summary>
        public enum Symbol
        {
            /// <summary>
            /// The message box  does not contain any graphic symbols.
            /// </summary>
            None = 0,
            /// <summary>
            /// @image{inline} html error24.png
            /// The message box contains an icon consisting of white X in a circle with a red background. Usually associated with an error status.
            /// </summary>
            Error = 16,
            /// <summary>
            /// @image{inline} html question24.png
            /// The message box contains an icon consisting of a question mark in a circle. Usually associated with a query message and YesNo buttons
            /// </summary>
            Question = 32,
            /// <summary>
            /// @image{inline} html warning24.png
            /// The message box contains an icon consisting of an exclamation point in a triangle with a yellow background. Usually associated with a warning status.
            /// </summary>
            Warning = 48,
            /// <summary>
            /// @image{inline} html info24.png
            /// The message box contains an icon consisting of a lowercase letter i in a circle. Usually associated with an informational status.
            /// </summary>
            Information = 64,
            /// <summary>
            /// @image{inline} html wait24.png
            /// The message box contains an icon consisting of an hourglass figure in a circle. Usually associated with an waiting for an action to complete and maybe an abort button.
            /// </summary>
            Wait = 128
        }

        [ThreadStatic] private static MiniMessageBox MMDialog = null;
        [ThreadStatic] private static DialogResult MMResult = DialogResult.None;
        private Resources resx = new Resources();
        private System.Drawing.Icon CaptionIcon = GetAppIcon();
        private Control OwningControl;
        private Font CaptionFont;
        private Font MessageFont;
        private Image MessageIcon;
        private string MessageIconString; //for clipboard
        private string Message;
        private string Caption;
        private string[] ButtonNames;
        private bool IsModal;
        private Rectangle rcCaptionIcon; //painted positions on form
        private Rectangle rcCaption;
        private Rectangle rcDivider;
        private Rectangle rcMessageIcon;
        private Rectangle rcMessage;

        /// <summary>
        /// Customize colors and fonts of various parts of the message box.
        /// </summary>
        /// <remarks>
        /// Any changes are permanant until explicitly changed again.<br/>
        /// If the properties need to be restored back after a special messagebox, then the caller must save them first.<br/>
        /// If the custom fonts are set, then the caller is responsible of disposing of them later.
        /// </remarks>
        public static readonly MsgBoxColors Colors = new MsgBoxColors();
        public class MsgBoxColors
        {
            /// <summary>
            /// Starting gradient color of caption. Default is the classic Win7 caption color.
            /// </summary>
            public Color CaptionGradientLeft { get; set; } = Color.FromArgb(15, 42, 111); //Win7 Color  //SystemColors.ActiveCaption;
            /// <summary>
            /// Ending gradient color of caption. Default is the classic Win7 caption color.
            /// </summary>
            public Color CaptionGradientRight { get; set; } = Color.FromArgb(165, 201, 239); //Win7 Color //SystemColors.GradientActiveCaption
            /// <summary>
            /// Color of the caption text. Default is SystemColors.HighlightText (aka White)
            /// </summary>
            public Color CaptionText { get; set; } = SystemColors.HighlightText; //SystemColors.ActiveCaptionText;
            /// <summary>
            /// Font used for the caption text. Default is null, aka whatever is the window's default font.
            /// </summary>
            public Font  CaptionFont { get; set; } = null;  //null == default font
            /// <summary>
            /// Starting gradient color of the inactive caption.  Default is SystemColors.InactiveCaption (aka faded blue)
            /// </summary>
            public Color InactiveCaptionGradientLeft { get; set; } = SystemColors.InactiveCaption;
            /// <summary>
            /// Ending gradient color of the inactive caption.  Default is SystemColors.GradientInactiveCaption (aka faded blue)
            /// </summary>
            public Color InactiveCaptionGradientRight { get; set; } = SystemColors.GradientInactiveCaption;
            /// <summary>
            /// Color of the inactive caption text. Default is SystemColors.GrayText (aka Gray)
            /// </summary>
            public Color InactiveCaptionText { get; set; } = SystemColors.GrayText; // SystemColors.InactiveCaptionText;
            /// <summary>
            /// Color of the message text. Default is SystemColors.WindowText (aka Black)
            /// </summary>
            public Color MessageText { get; set; } = SystemColors.WindowText;
            /// <summary>
            /// Font used for the message text. Default is null, aka whatever is the window's default font.
            /// </summary>
            public Font  MessageFont { get; set; } = null;  //null == default font
            /// <summary>
            /// Color of the message text background. Default is SystemColors.Window (aka White)
            /// </summary>
            public Color Background { get; set; } = SystemColors.Window;

            /// <summary>
            /// Create shallow copy of these properties. Meaning the fonts refer to the same font (class, by-ref) object. The colors are structs and copied by value.
            /// This is useful when the messagebox is changed for only one instance.
            /// </summary>
            /// <returns>MsgBoxColors copy.</returns>
            public MsgBoxColors Backup() => (MsgBoxColors)typeof(Object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null).Invoke(this, new object[0]);

            /// <summary>
            /// Copy all the values back into this object. Note: the original font instances to be overriden are NOT disposed. 
            /// </summary>
            /// <param name="backupProperties">Object to copy all the values from.</param>
            public void Restore(MsgBoxColors backupProperties)
            {
                if (backupProperties == null) throw new ArgumentNullException(nameof(backupProperties));
                foreach(var p in typeof(MsgBoxColors).GetProperties())
                {
                    //if (p.PropertyType.IsClass && typeof(IDisposable).IsAssignableFrom(p.PropertyType)) ((IDisposable)p.GetValue(clr)).Dispose();
                    p.SetValue(this, p.GetValue(backupProperties));
                }
            }
        }

        /// <summary>
        /// Displays a tiny modal (i.e. waits) message box in front of the specified object and with the specified text and caption.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box. A null owner will attempt to find it's owner, which may be the desktop.</param>
        /// <param name="text">The text to display in the message box. Text is automatically wrapped to fit a 3x2 rectangle and size of popup is adjusted accordingly.</param>
        /// <param name="caption">The text to display in the title bar of the message box. Text is a single line, never wrapped.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        /// <remarks>This functions just like it's big brothers: MessageBox and MessageBoxEx. This does not need to be a child of a form owner. This may also run within Program.Main or Console.Main</remarks>
        public static DialogResult ShowDialog(IWin32Window owner, string text, string caption = null)
        {
            return ShowDialog(owner, text, caption, MiniMessageBox.Buttons.OK, MiniMessageBox.Symbol.None);
        }

        /// <summary>
        /// Displays a tiny modal (i.e. waits) message box in front of the specified object and with the specified text, caption, buttons, and icon.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box. A null owner will attempt to find it's owner, which may be the desktop.</param>
        /// <param name="text">The text to display in the message box. Text is automatically wrapped to fit a 3x2 rectangle and size of popup is adjusted accordingly.</param>
        /// <param name="caption">The text to display in the title bar of the message box. Text is a single line, never wrapped.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        /// <remarks>This functions just like it's big brothers: MessageBox and MessageBoxEx. This does not need to be a child of a form owner. This may also run within Program.Main or Console.Main</remarks>
        public static DialogResult ShowDialog(IWin32Window owner, string text, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            return ShowDialog(owner, text, caption, ToMMButton(buttons), ToMMSymbol(icon));
        }

        /// <summary>
        /// Displays a tiny modal (i.e. waits) message box in front of the specified object and with the specified text, caption, buttons, and icon.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box. A null owner will attempt to find it's owner, which may be the desktop.</param>
        /// <param name="text">The text to display in the message box. Text is automatically wrapped to fit a 3x2 rectangle and size of popup is adjusted accordingly.</param>
        /// <param name="caption">The text to display in the title bar of the message box. Text is a single line, never wrapped.</param>
        /// <param name="buttons">Combination of buttons to display in the message box. Using None will default to an OK button.</param>
        /// <param name="icon">A value that specifies which symbol graphic to display along side the message.</param>
        /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
        /// <remarks>This functions just like it's big brothers: MessageBox and MessageBoxEx. This does not need to be a child of a form owner. This may also run within Program.Main or Console.Main</remarks>
        public static DialogResult ShowDialog(IWin32Window owner, string text, string caption = null, Buttons buttons = Buttons.OK, Symbol icon = Symbol.None)
        {
            var owningControl = GetOwner(owner);

            using (var dlg = new MiniMessageBox(owningControl, true, text, caption, buttons, icon))
            {
                return dlg.ShowDialog(owningControl);
            }
        }

        /// <summary>
        /// Displays a tiny modalless (i.e. returns immediately) message box in front of the specified object and with the specified text, caption, buttons, and icon.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box. A null owner will attempt to find it's owner, which may be the desktop.</param>
        /// <param name="text">The text to display in the message box. Text is automatically wrapped to fit a 3x2 rectangle and size of popup is adjusted accordingly.</param>
        /// <param name="caption">The text to display in the title bar of the message box. Text is a single line, never wrapped.</param>
        /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box OR ((MessageBoxButtons) -1) for no buttons. In which case, the calling code is responsible for closing the dialog via Hide().</param>
        /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
        /// <remarks>
        /// This is thread static. Meaning it must be closed within the context of the same thread. See Hide().
        /// This functions just like it's big brothers: MessageBox and MessageBoxEx. This does not need to be a child of a form owner. This may also run within Program.Main or Console.Main
        /// </remarks>
        public static void Show(IWin32Window owner, string text, string caption = null, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None)
        {
            Show(owner, text, caption, ToMMButton(buttons), ToMMSymbol(icon));
        }

        /// <summary>
        /// Displays a tiny modalless (i.e. returns immediately) message box in front of the specified object and with the specified text, caption, buttons, and icon.
        /// </summary>
        /// <param name="owner">An implementation of System.Windows.Forms.IWin32Window that will own the modal dialog box. A null owner will attempt to find it's owner, which may be the desktop.</param>
        /// <param name="text">The text to display in the message box. Text is automatically wrapped to fit a 3x2 rectangle and size of popup is adjusted accordingly.</param>
        /// <param name="caption">The text to display in the title bar of the message box. Text is a single line, never wrapped.</param>
        /// <param name="buttons">Combination of buttons to display in the message box or None for no buttons. In which case, the calling code is entirely responsible for closing the dialog via Hide().</param>
        /// <param name="icon">A value that specifies which symbol graphic to display along side the message.</param>
        /// <remarks>
        /// This is thread static. Meaning it must be closed within the context of the same thread. See Hide().
        /// This functions just like it's big brothers: MessageBox and MessageBoxEx. This does not need to be a child of a form owner. This may also run within Program.Main or Console.Main
        /// </remarks>
        public static void Show(IWin32Window owner, string text, string caption = null, Buttons buttons = Buttons.OK, Symbol icon = Symbol.None)
        {
            if (MMDialog != null)
            {
                var emsg = $"Can show only ONE modalless MiniMessageBox popup at a time.\r\n\r\n[{caption ?? "(null)"}]\r\n{text ?? "(null)"}";
                MiniMessageBox.ShowDialog(owner, emsg, "Multiple Modalless Popups", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var owningControl = GetOwner(owner);
            MMDialog = new MiniMessageBox(owningControl, false, text, caption, buttons, icon);
            MMDialog.Show(owningControl);
        }

        private static Buttons ToMMButton(MessageBoxButtons b)
        {
            switch (b)
            {
                case (MessageBoxButtons)(-1): return Buttons.None;
                case MessageBoxButtons.OK: return Buttons.OK;
                case MessageBoxButtons.OKCancel: return Buttons.OKCancel;
                case MessageBoxButtons.AbortRetryIgnore: return Buttons.AbortRetryIgnore;
                case MessageBoxButtons.YesNoCancel: return Buttons.YesNoCancel;
                case MessageBoxButtons.YesNo: return Buttons.YesNo;
                case MessageBoxButtons.RetryCancel: return Buttons.RetryCancel;
                default: return Buttons.OK;
            }
        }

        private static Symbol ToMMSymbol(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.None: return Symbol.None;
                case MessageBoxIcon.Error: return Symbol.Error;
                case MessageBoxIcon.Question: return Symbol.Question;
                case MessageBoxIcon.Warning: return Symbol.Warning;
                case MessageBoxIcon.Information: return Symbol.Information;
                default: return Symbol.None;
            }
        }

        /// <summary>
        /// Closes a modalless dialog if it is not already closed.
        /// Only valid within the context of the thread that called Show().
        /// </summary>
        /// <returns>The final Dialog result value of the closed modalless dialog. Only meaningful for a modalless dialog. If dialog closed by user via a MessagBoxButton, this will return the last closing value.</returns>
        public static new DialogResult Hide()
        {
            if (MMDialog !=null)
            {
                MMDialog.Close();
                MMDialog.Dispose();
                MMDialog = null;
                MMResult = DialogResult.None;
                return MMResult;
            }
            else
            {
                return MMResult;
            }
        }

        private static void StaticButtonClick(object sender, EventArgs e)
        {
            //This is for Modalless dialogs that return immediately upon calling Show()
            Button btn = (Button)sender;
            switch(btn.Name)
            {
                case "OK": MMResult = DialogResult.OK; break;
                case "Cancel": MMResult = DialogResult.Cancel; break;
                case "Abort": MMResult = DialogResult.Abort; break;
                case "Retry": MMResult = DialogResult.Retry; break;
                case "Ignore": MMResult = DialogResult.Ignore; break;
                case "Yes": MMResult = DialogResult.Yes; break;
                case "No": MMResult = DialogResult.No; break;
                default: MMResult = DialogResult.None; break;
            }

            MMDialog.Close();
            MMDialog.Dispose();
            MMDialog = null;
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            //This is for Modal dialogs that hang until user presses a button via ShowDialog()
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "OK": this.DialogResult = DialogResult.OK; break;
                case "Cancel": this.DialogResult = DialogResult.Cancel; break;
                case "Abort": this.DialogResult = DialogResult.Abort; break;
                case "Retry": this.DialogResult = DialogResult.Retry; break;
                case "Ignore": this.DialogResult = DialogResult.Ignore; break;
                case "Yes": this.DialogResult = DialogResult.Yes; break;
                case "No": this.DialogResult = DialogResult.No; break;
                default: this.DialogResult = DialogResult.None; break;
            }

            this.Close();
        }

        //*********** private constructor *******************************************************************************
        private MiniMessageBox(IWin32Window owningControl, bool isModal, string msg, string caption, Buttons buttons, Symbol icon)
        {
            this.OwningControl = owningControl as Control;

            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Colors.Background;
            this.Name = "MiniMessageBox";
            this.ShowInTaskbar = owningControl==null; //if 'owned' by desktop, show in taskbar.
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "MiniMessageBox";
            this.ResumeLayout(false);

            // With modalless dialogs, the calling code has control when to close the dialog via Hide(); but modal dialogs
            // need an action for the user to close the dialog as the calling code must wait. Thus a valid enum value is required.
            if (isModal && buttons==Buttons.None)
            {
                buttons = Buttons.OK;
            }

            IsModal = isModal;

            MessageFont = Colors.MessageFont ?? this.Font;
            CaptionFont = Colors.CaptionFont ?? this.Font;
            MessageIcon = GetMessageIcon(icon, out MessageIconString);
            Message = string.IsNullOrWhiteSpace(msg) ? null : msg.Trim();
            Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim();
            ButtonNames = GetButtonNames(buttons);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            GDI.ApplyShadows(this);
            base.OnHandleCreated(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            //Compute size and position of all the elements on the popup

            this.SuspendLayout();
            const int Border = 2;
            int Spacing = MessageFont.Height / 2; //spacing beteween the elements in the client area.
            Size szButton = Size.Empty;
            Size szButtonGroup = Size.Empty;
            if (ButtonNames.Length > 0)
            {
                szButton = new Size(ButtonNames.Max(s => ComputeTextDimensions(null, s, MessageFont, int.MaxValue).Width) + 10, ComputeTextDimensions(null, "H", MessageFont, int.MaxValue).Height + 10);
                szButtonGroup = new Size(ButtonNames.Length * (szButton.Width + Spacing) - Spacing, szButton.Height);
            }

            //Compute size of the elements in pixels
            Size szCaptionIcon = Size.Empty;
            Size szCaption = Size.Empty;
            if (Caption != null)
            {
                szCaptionIcon = CaptionIcon == null ? new Size(0, 0) : CaptionIcon.Size;
                szCaption = string.IsNullOrEmpty(Caption) ? Size.Empty : ComputeTextDimensions(null, Caption, CaptionFont, int.MaxValue);
                if (szCaptionIcon.Height > szCaption.Height) szCaption.Height = szCaptionIcon.Height;
            }

            var szMessageIcon = MessageIcon == null ? Size.Empty : MessageIcon.Size;
            Size szMessage = new Size(0, szMessageIcon.Height);
            if (Message != null)
            {
                szMessage = ComputeTextDimensions(null, Message, MessageFont);
                int max = Math.Max(Math.Max(szButtonGroup.Width, szCaptionIcon.Width + 1 + szCaption.Width), szMessageIcon.Width + Spacing + szMessage.Width);
                szMessage = ComputeTextDimensions(null, Message, MessageFont, max - szMessageIcon.Width - 1);
                if (szMessageIcon.Height > szMessage.Height) szMessage.Height = szMessageIcon.Height;
            }

            //Compute position of elements
            if (Caption != null)
            {
                rcCaptionIcon = new Rectangle(Border + 1, Border + 1, szCaptionIcon.Width, szCaptionIcon.Height);
                rcCaption = new Rectangle(rcCaptionIcon.Right + 1, rcCaptionIcon.Top, szCaption.Width, szCaption.Height);
                rcDivider = Rectangle.FromLTRB(0, rcCaption.Bottom + 1, rcCaption.Right + 1000, rcCaption.Bottom + 1 + 1);
            }

            rcMessageIcon = new Rectangle(Border+Spacing, rcDivider.Bottom + Spacing, szMessageIcon.Width, szMessageIcon.Height);
            rcMessage = new Rectangle((MessageIcon == null ? Border + Spacing : rcMessageIcon.Right + Spacing), rcMessageIcon.Top, szMessage.Width, szMessage.Height);

            //Compute size of popup
            var rc1 = Rectangle.Union(rcCaptionIcon, rcCaption);
            var rc2 = Rectangle.Union(rcMessageIcon, rcMessage);
            var rc3 = Rectangle.Union(rc1, rc2);
            var rc4 = Rectangle.Union(rc3, new Rectangle(Border + Spacing, rcMessage.Bottom + Spacing, szButtonGroup.Width, szButtonGroup.Height));

            Width = rc4.Right + Border + Spacing;  //Add border spacing
            Height = rc4.Bottom + Border + Spacing;

            //Add buttons
            int xOffset = (Width - szButtonGroup.Width) / 2;
            foreach (var buttonName in ButtonNames)
            {
                var btn = new Button();
                btn.Name = buttonName; //This is the internal key.
                btn.Text = buttonName; //This is the translated display value.
                btn.Size = szButton;
                btn.Location = new Point(xOffset, rcMessage.Bottom + Spacing);
                xOffset += szButton.Width + Spacing;
                if (IsModal) btn.Click += ButtonClick;
                else btn.Click += StaticButtonClick;
                this.Controls.Add(btn);
            }

            //Center popup over parent

            Rectangle ownerBounds = this.OwningControl == null ?
                this.Owner == null ?
                Screen.FromControl(this).WorkingArea :
                this.Owner.DesktopBounds :
                OwningControl.Parent==null ?
                this.Owner.DesktopBounds : 
                OwningControl.Parent.RectangleToScreen(OwningControl.Bounds);

            Rectangle ownerClientRetangle = this.OwningControl == null ?
                this.Owner == null ?
                Screen.FromControl(this).WorkingArea :
                this.Owner.ClientRectangle :
                OwningControl.ClientRectangle;

            this.Location = new Point(
                (ownerBounds.Width - this.DesktopBounds.Width) / 2 + ownerBounds.X,
                (ownerBounds.Height - this.DesktopBounds.Height) / 2 + ownerBounds.Y - ownerClientRetangle.Height / 6);

            this.ResumeLayout(false);
            base.OnLoad(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (CaptionIcon != null) { CaptionIcon.Dispose(); CaptionIcon = null; }
            if (resx != null) { resx.Dispose(); resx = null; }

            base.Dispose(disposing);
        }

        private bool IsActivated = true; 
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            IsActivated = true;
            this.Refresh();
        }
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            IsActivated = false;
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            const TextFormatFlags flags = TextFormatFlags.HidePrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;

            base.OnPaint(e);

            if (Caption != null)
            {
                var rc = new RectangleF(0, 0, this.Size.Width, rcDivider.Bottom);
                using (var br = new LinearGradientBrush(rc,
                    IsActivated ? Colors.CaptionGradientLeft : Colors.InactiveCaptionGradientLeft,
                    IsActivated ? Colors.CaptionGradientRight : Colors.InactiveCaptionGradientRight,
                    LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(br, rc);
                if (CaptionIcon != null)
                {
                    e.Graphics.DrawIcon(CaptionIcon, rcCaptionIcon);
                    //e.Graphics.DrawRectangle(Pens.Red, rcCaptionIcon.X, rcCaptionIcon.Y, rcCaptionIcon.Width - 1, rcCaptionIcon.Height - 1);
                }

                TextRenderer.DrawText(e.Graphics, Caption, CaptionFont, rcCaption, IsActivated ? Colors.CaptionText : Colors.InactiveCaptionText, Color.Transparent, flags);
                //e.Graphics.DrawRectangle(Pens.Red, rcCaption.X, rcCaption.Y, rcCaption.Width - 1, rcCaption.Height - 1);

                e.Graphics.DrawLine(SystemPens.ActiveBorder, rcDivider.Left, rcDivider.Top, rcDivider.Right, rcDivider.Top);
            }

            if (MessageIcon !=null)
            {
                e.Graphics.DrawImage(MessageIcon, rcMessageIcon);
                //e.Graphics.DrawRectangle(Pens.Red, rcMessageIcon.X, rcMessageIcon.Y, rcMessageIcon.Width-1, rcMessageIcon.Height-1);

            }
            if (Message != null)
            {
                TextRenderer.DrawText(e.Graphics, Message, MessageFont, rcMessage, Colors.MessageText, Color.Transparent, flags);
                //e.Graphics.DrawRectangle(Pens.Red, rcMessage.X, rcMessage.Y, rcMessage.Width - 1, rcMessage.Height - 1);
            }

            //Simple Border (not used)
            //e.Graphics.DrawRectangle(SystemPens.ActiveBorder, 0, 0, this.Width - 1, this.Height - 1);

            //Draw 2-pixel 3-D border.
            //Would use ControlPaint.DrawBorder3D(e.Graphics,  this.ClientRectangle, Border3DStyle.Raised), however for some reason,
            //the bottom & right lines come out as cyan!! So we just draw the border by hand. Perhaps because it's drawing on the edge
            //of the form and Pens draw to the right/bottom of the pixel?

            e.Graphics.DrawLine(SystemPens.ControlLight, this.ClientRectangle.Left, this.ClientRectangle.Top, this.ClientRectangle.Right - 1, this.ClientRectangle.Top);
            e.Graphics.DrawLine(SystemPens.ControlLightLight, this.ClientRectangle.Left + 1, this.ClientRectangle.Top+1, this.ClientRectangle.Right - 2, this.ClientRectangle.Top+1);

            e.Graphics.DrawLine(SystemPens.ControlLight, this.ClientRectangle.Left, this.ClientRectangle.Top, this.ClientRectangle.Left, this.ClientRectangle.Bottom - 1);
            e.Graphics.DrawLine(SystemPens.ControlLightLight, this.ClientRectangle.Left+1, this.ClientRectangle.Top+1, this.ClientRectangle.Left+1, this.ClientRectangle.Bottom - 2);

            e.Graphics.DrawLine(SystemPens.ControlDarkDark, this.ClientRectangle.Right - 1, this.ClientRectangle.Top, this.ClientRectangle.Right - 1, this.ClientRectangle.Bottom);
            e.Graphics.DrawLine(SystemPens.ControlDark, this.ClientRectangle.Right - 2, this.ClientRectangle.Top+1, this.ClientRectangle.Right - 2, this.ClientRectangle.Bottom);

            e.Graphics.DrawLine(SystemPens.ControlDark, this.ClientRectangle.Left + 1, this.ClientRectangle.Bottom -2, this.ClientRectangle.Right - 2, this.ClientRectangle.Bottom-2);
            e.Graphics.DrawLine(SystemPens.ControlDarkDark, this.ClientRectangle.Left, this.ClientRectangle.Bottom-1, this.ClientRectangle.Right, this.ClientRectangle.Bottom-1);
        }

        //Copy n' Paste content to the clipboard
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyData == (Keys.C|Keys.Control))
                Clipboard.SetData(DataFormats.Text, $"{(Caption==null?"":Caption+"\r\n")}{(MessageIcon==null?"": MessageIconString+" ")}{Message??""}");

            base.OnKeyUp(e);
        }

        #region Click n'Drag this Form
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")] private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
            base.OnMouseDown(e);
        }
        #endregion //  Click n'Drag this Form

        private static string[] GetButtonNames(Buttons b)
        {
            if (b == Buttons.None) return new string[0];

            var names = new List<string>(8);
            if ((b & Buttons.OK) != 0) names.Add("OK");
            if ((b & Buttons.Abort) != 0) names.Add("Abort");
            if ((b & Buttons.Retry) != 0) names.Add("Retry");
            if ((b & Buttons.Ignore) != 0) names.Add("Ignore");
            if ((b & Buttons.Yes) != 0) names.Add("Yes");
            if ((b & Buttons.No) != 0) names.Add("No");
            if ((b & Buttons.Cancel) != 0) names.Add("Cancel"); //Must be last for order in OKCancel, YesNoCancel, RetryCancel

            return names.ToArray();
        }

        private Image GetMessageIcon(Symbol icon, out string iconString)
        {
            // 'iconString' is prefix for pasting into the clipboard. Any string translations should occur here for prefix.
            switch (icon)
            {
                case Symbol.Error: iconString = "[Error]"; return resx.ErrorIcon;
                case Symbol.Question: iconString = "[Question]"; return resx.QuestionIcon;
                case Symbol.Warning: iconString = "[Warning]"; return resx.WarningIcon;
                case Symbol.Information: iconString = "[Information]"; return resx.InfoIcon;
                case Symbol.Wait: iconString = "[Wait]"; return resx.WaitIcon;
                default: iconString = ""; return null;
            }
        }

        private static System.Drawing.Icon GetAppIcon()
        {
            //These icons must be disposed after use.
            System.Drawing.Icon ico = null;
            FormCollection fc = System.Windows.Forms.Application.OpenForms;
            if (fc != null && fc.Count > 0) ico = fc[0].Icon == null ? null : new System.Drawing.Icon(fc[0].Icon, SystemInformation.SmallIconSize.Width, SystemInformation.SmallIconSize.Height);
            if (ico == null) ico = GDI.ExtractAssociatedIcon(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, 16);
            return ico;
        }

        /// <summary>
        /// Compute the dimensions of 3x2 rectangle box that will fit the supplied text.
        /// </summary>
        /// <param name="graphics">Graphics object used to measure string. Or NULL to use legacy TextRenderer (necessary to mirror Forms.Label control wrapping).</param>
        /// <param name="s">Supplied text. If string contains newlines, autowrap is disabled.</param>
        /// <param name="font">Font to use for measuring.</param>
        /// <param name="maxWidth"> If 0, then autowrap and fit text into a 3x2 rectangle. Should not contain newlines. Else autowrap at this width.</param>
        /// <returns>size of fitted box.</returns>
        private static Size ComputeTextDimensions(Graphics graphics, string s, Font font, int maxWidth = 0)
        {
            const TextFormatFlags flags = TextFormatFlags.HidePrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
            Func<float, int> ceil = (f) => (int)(f > (int)f ? f + 1 : f);

            if (s == null && s.Length == 0) return Size.Empty;

            if (s.Any(c => c == '\n')) maxWidth = int.MaxValue; //We don't autowrap if the string contains newlines.

            if (maxWidth > 0)
            {
                if (graphics != null) return Size.Ceiling(graphics.MeasureString(s, font, new SizeF(maxWidth, int.MaxValue), (StringFormat)null));
                else return TextRenderer.MeasureText(s, font, new Size(maxWidth, int.MaxValue), flags);
            }

            int width = 50;
            SizeF sizef = SizeF.Empty;
            SizeF prevsizef = SizeF.Empty;
            double ratio = 0;

            while (ratio < 3)
            {
                if (graphics != null) sizef = graphics.MeasureString(s, font, width);
                else sizef = TextRenderer.MeasureText(s, font, new Size(width, 99999), flags);
                if (sizef == prevsizef) break;
                ratio = sizef.Width / (double)sizef.Height;
                width += 25;
                prevsizef = sizef;
            }

            width = ceil(sizef.Width);
            for (int i = 0; i < 50; i++)
            {
                width--;
                SizeF size2;
                if (graphics != null) size2 = graphics.MeasureString(s, font, width);
                else size2 = TextRenderer.MeasureText(s, font, new Size(width, 99999), flags);
                if (size2.Height > sizef.Height) break;
            }

            return new Size(width + 1, ceil(sizef.Height));
        }

        private static IWin32Window GetOwner(IWin32Window owner)
        {
            //Bullit-proof that we have an owner. Default null == desktop.
            // 'owner' can be any Win32 windows-based control, but only the first *visible* Form may host another form (e.g. this popup).
            // In the end, 'owner' may still be null, but that just refers to the desktop.

            if (owner!=null)
            {
                var ctrl = owner as Control;
                if (ctrl!=null)
                {
                    Control p = ctrl;
                    while(!(p is Form))
                    {
                        p = p.Parent;
                        if (p == null) break;
                    }
                    if (p!=null)
                    {
                        var f = (Form)p;
                        while(!f.Visible)  //MiniMessageBox called in a Form constructor
                        {
                            f = f.Owner;
                            if (f == null) break;
                        }
                        owner = f;
                    }
                }
            }

            if (owner == null) owner = System.Windows.Forms.Form.ActiveForm;
            if (owner == null)
            {
                FormCollection fc = System.Windows.Forms.Application.OpenForms;
                if (fc != null && fc.Count > 0) owner = fc[fc.Count-1];
            }
            return owner;
        }

        /// <summary>
        /// We keep our own graphic resources here to make this file entirely self-contained so it may be copied an reused elsewhere without any dependencies.
        /// </summary>
        private class Resources : IDisposable
        {
            private Image __errorIcon = null;
            private Image __warningIcon = null;
            private Image __questionIcon = null;
            private Image __infoIcon = null;
            private Image __waitIcon = null;

            public Image ErrorIcon => __errorIcon == null ? (__errorIcon = Base64StringToBitmap(ErrorBase64)) : __errorIcon;
            public Image WarningIcon => __warningIcon == null ? (__warningIcon = Base64StringToBitmap(WarningBase64)) : __warningIcon;
            public Image QuestionIcon => __questionIcon == null ? (__questionIcon = Base64StringToBitmap(QuestionBase64)) : __questionIcon;
            public Image InfoIcon => __infoIcon == null ? (__infoIcon = Base64StringToBitmap(InfoBase64)) : __infoIcon;
            public Image WaitIcon => __waitIcon == null ? (__waitIcon = Base64StringToBitmap(WaitBase64)) : __waitIcon;

            private static Bitmap Base64StringToBitmap(string base64String)
            {
                byte[] byteBuffer = Convert.FromBase64String(base64String);
                MemoryStream memoryStream = new MemoryStream(byteBuffer);
                memoryStream.Position = 0;
                Bitmap bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream, false, true);
                memoryStream.Close();
                memoryStream = null;
                byteBuffer = null;
                return bmpReturn;
            }

            public void Dispose()
            {
                if (__errorIcon != null) { __errorIcon.Dispose(); __errorIcon = null; }
                if (__warningIcon != null) { __warningIcon.Dispose(); __warningIcon = null; }
                if (__questionIcon != null) { __questionIcon.Dispose(); __questionIcon = null; }
                if (__infoIcon != null) { __infoIcon.Dispose(); __infoIcon = null; }
                if (__waitIcon != null) { __waitIcon.Dispose(); __waitIcon = null; }
            }

            // Compressed 24x24 png's with Photoshop 'Save for Web'
            const string ErrorBase64 = @"
        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";

            const string WarningBase64 = @"
        iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8
        YQUAAAAZdEVYdFNvZnR3YXJlAEFkb2JlIEltYWdlUmVhZHlxyWU8AAAEKklEQVRIS7WUDUyUdRzHnwMO
        Tk+QeE3lRQIng9BDkFe58IZd1Jo3NByhpiLIoHSMXp6EXI4XAwIrAVtFJmK3lYc4KXeCOCOPEpO3iVCy
        E70yEOrAO+4F4duPu2dzbayR4nf77Nnze/6/7+////3/z58B8ESZNTifzFk2tnzHOvnZu4ePfDVMr+7W
        6DwqJfP9L/DbSUx1y/Hcy2nfcuH5kYt3cEBP+8+YrI0FFDI0nlGC5+Acxn1+fB2skP+IvmqgMQ5QrgfU
        J5GcXnCF+/x4ikx4ZZPuzlVcyAtFYVocPkiPwYUyMQY6W+HmLUrlhj2ieHa8H67cNOL6Aex5wZ+OhDOx
        GEkbfID7h1FUVjtKo2ysgx9BaTnFxdC0UltiUJQdB8bWAwzPA9nbaS+GxdCqlQgUycq54f9PbssC3G+r
        B6cNZ5OA6zKU5VIBnietYCly0mkvjFLgXhYUpy9TzN6PS5u7SqobzuD3euhPrQYGU1H5XjwZ0QoYIfbu
        jgCmtwKaaExrGyBLZpVc2ty0YnV8mO7PW5hofB7mthRgPBtHCzeQOWMhZ080/aT7gKFdtBcZuNreBaen
        /BIsyXPRCYWqH/3leNAcA6MqgQpEouqAN5nb0D4sR+l+WpV+OXA7EBjxBaaK8Na7dWou/b8lTc7N1v/6
        PfRf2sDQKMRkC10p9xgczSNzxhMuy5JwrCIE0FH8DjFMjDHQqBXw89+8l7OZXbYOzoKmS70PzKdDYJYz
        MDXzgXYyGGEgL10MVxdfhEW8BEVNMM2a4qNWc+u9tgpHKs9NkM1Cq9ss2rav8nNT+yEYP2NgOLcQ0222
        QAclzxRRMxjvdsIf1xZQyzhjPTFJcJenVlsFieTtWs7u33Ja8qzvTxdbYDruBFM9H+aLDsAvlDhTYJBB
        X7MT1oSvhShUjO42NzpFFJ8BNAm4E0uIcCiVKvD5HiLO9qFyDx6/bGraAXMNA/13izClsrOaXyPMDD7M
        96I9SIL/ynJUfkL3kWXWtBr4E1HEi5an0ViFzMyKDs7WKq+QxMTu81/DVE3mp4QwNQmoLTxrgU6CNvNu
        jwPy34nCR+Uy/D3qSGY0AawgxMQmIoN4k8jEjRut8PEJ3WIxt7F3XFRddWIUdSuBT8msgWgluokeopcY
        IAwE1+uHBBASYhvBEhXEx8Q3KCurGRUIhE8z9kI3L2W9AtpLJbjVUgiNqhBDHQUY6S3AX/2F0A4UY1xz
        CPeHSjChJXSlmJioII4RChgM56ktKqILen0fxsZuQqfrQVdnB1xdPddbVrHU6xnpKnFqXmDklv1BESls
        8NqtbEjEDlYUtYtdE7ObDY/JYCNjM9nodVls7LrXWbH4DTY+PouVSDLYhIQ0VirdySYmvkbPV+l9M7tx
        4/b8oKDgnWRtZynAaeb8LiAE84CQsIoa9gQB8w8H1pa5He6IrwAAAABJRU5ErkJggg==";

            const string QuestionBase64 = @"
        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
        F1KtUEX4PTODAAAAAElFTkSuQmCC";

            const string InfoBase64 = @"
        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
        dTfpywAAAABJRU5ErkJggg==";

            const string WaitBase64 = @"
        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
        1/MAAAAASUVORK5CYII=";
        }
    }
}
