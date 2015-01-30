using System;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using DevComponents.DotNetBar.Rendering;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace DevComponents.DotNetBar
{
    /// <summary>
    /// Represents the form in Office 2007 style with custom borders and caption.
    /// </summary>
    public class Office2007Form : Office2007RibbonForm
    {
        #region Private Variables
        private int m_BorderWidth = 2;
        private int m_CornerSize = 5;
        private GenericItemContainer m_Caption = null;
        private ColorScheme m_ColorScheme = null;
        private LabelItem m_TitleLabel = null;
        private SystemCaptionItem m_SystemIcon = null;
        private SystemCaptionItem m_SystemButtons = null;
        private Timer m_MouseTimer = null;
        private Font m_CaptionFont = null;
        private string m_TitleText = "";
        #endregion

        #region Constructor
        public Office2007Form()
            : base()
        { 
            m_BorderWidth = SystemInformation.Border3DSize.Width + NativeFunctions.BorderMultiplierFactor;
            this.DockPadding.All = 0;

            m_Caption = new GenericItemContainer();
            m_Caption.GlobalItem = false;
            m_Caption.ContainerControl = this;
            m_Caption.WrapItems = false;
            m_Caption.EventHeight = false;
            m_Caption.UseMoreItemsButton = false;
            m_Caption.Stretch = true;
            m_Caption.Displayed = true;
            m_Caption.SystemContainer = true;
            m_Caption.PaddingTop = 0;
            m_Caption.PaddingBottom = 0;
            m_Caption.PaddingLeft = 2;
            m_Caption.ItemSpacing = 1;
            m_Caption.SetOwner(this);
            m_Caption.Style = eDotNetBarStyle.Office2007;
            m_Caption.ToolbarItemsAlign = eContainerVerticalAlignment.Top;

            m_SystemIcon = new SystemCaptionItem();
            m_SystemIcon.Enabled = false;
            m_Caption.SubItems.Add(m_SystemIcon);
            m_SystemIcon.IsSystemIcon = true;
            m_SystemIcon.Icon = this.Icon;

            m_TitleLabel = new LabelItem();
            m_TitleLabel.Font = SystemInformation.MenuFont;
            m_TitleLabel.Stretch = true;
            m_TitleLabel.TextLineAlignment = StringAlignment.Center;
            m_TitleLabel.Text = this.Text;
            m_TitleLabel.PaddingLeft = 3;
            m_TitleLabel.PaddingRight = 3;
            m_Caption.SubItems.Add(m_TitleLabel);

            m_SystemButtons = new SystemCaptionItem();
            //m_SystemButtons.ItemAlignment = eItemAlignment.Far;
            m_SystemButtons.Click += new EventHandler(SystemButtons_Click);
            m_SystemButtons.MouseEnter += new EventHandler(SystemButtons_MouseEnter);
            m_Caption.SubItems.Add(m_SystemButtons);
        }
        #endregion

        #region Internal Implementation
        /// <summary>
        /// Gets or sets the rich text displayed in form caption instead of the Form.Text property. This property supports text-markup.
        /// You can use <font color="SysCaptionTextExtra"> markup to instruct the markup renderer to use Office 2007 system caption extra text color which
        /// changes depending on the currently selected color table. Note that when using this property you should manage also the Form.Text property since
        /// that is the text that will be displayed in Windows task-bar and elsewhere where system Form.Text property is used.
        /// You can also use the hyperlinks as part of the text markup and handle the TitleTextMarkupLinkClick event to be notified when they are clicked.
        /// Setting this property to an empty string (default value) indicates that Form.Text property will be used for form caption.
        /// </summary>
        [Browsable(true), DefaultValue(""), Editor(typeof(Design.TextMarkupUIEditor), typeof(System.Drawing.Design.UITypeEditor)), EditorBrowsable(EditorBrowsableState.Always), Category("Appearance"), Description("Indicates text displayed in form caption instead of the Form.Text property.")]
        public string TitleText
        {
            get { return m_TitleText; }
            set 
            {
                if (value == null) value = "";
                
                m_TitleText = value;
                if (value == "")
                    m_TitleLabel.Text = this.Text;
                else
                    m_TitleLabel.Text = value;
                if (this.IsHandleCreated) this.RecalcSize();
                this.InvalidateNonClient(false);
            }
        }

        /// <summary>
        /// Gets whether custom form styling is enabled.
        /// </summary>
        /// <returns>true if custom styling is enabled otherwise false.</returns>
        protected override bool IsCustomFormStyleEnabled()
        {
            return base.IsCustomFormStyleEnabled() && (!(WinApi.IsGlassEnabled && EnableGlass && !DesignMode) || this.IsMdiChild);
        }

        /// <summary>
        /// Gets or sets the font for the form caption text when CaptionVisible=true. Default value is NULL which means that system font is used.
        /// </summary>
        [Browsable(true), DefaultValue(null), Category("Caption"), Description("Indicates font for the form caption text when CaptionVisible=true.")]
        public Font CaptionFont
        {
            get { return m_CaptionFont; }
            set
            {
                m_CaptionFont = value;
                OnCaptionFontChanged();
            }
        }

        private void OnCaptionFontChanged()
        {
            if (m_TitleLabel != null)
            {
                if (m_CaptionFont != null)
                    m_TitleLabel.Font = m_CaptionFont;
                else
                {
#if FRAMEWORK20
                    m_TitleLabel.Font = SystemFonts.CaptionFont;
#else
                    m_TitleLabel.Font = SystemInformation.MenuFont;
#endif
                }
                this.Invalidate(m_Caption.DisplayRectangle);
            }
            InvalidateNonClient(false);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            InvalidateNonClient(false);
        }

        protected override bool WindowsMessageSetIcon(ref Message m)
        {
            UpdateSystemButton();
            return base.WindowsMessageSetIcon(ref m);
        }
        #endregion

        #region Non-client area handling
        /// <summary>
        /// Invalidates non client area of the form.
        /// </summary>
        /// <param name="invalidateForm">Indicates whether complete form is invalidated.</param>
        public void InvalidateNonClient(bool invalidateForm)
        {
            if (!BarFunctions.IsHandleValid(this)) return;
            const int RDW_INVALIDATE = 0x0001;
            const int RDW_FRAME = 0x0400;
            NativeFunctions.RECT r = new NativeFunctions.RECT(0, 0, this.Width, m_Caption.HeightInternal);
            if(invalidateForm)
                r = new NativeFunctions.RECT(0, 0, this.Width, this.Height);
            NativeFunctions.RedrawWindow(this.Handle, ref r, IntPtr.Zero, RDW_INVALIDATE | RDW_FRAME);
        }

        protected override Rectangle ReduceDisplayRectangle(Rectangle r)
        {
            return r;
        }
        protected override void AdjustBounds(ref int width, ref int height, BoundsSpecified specified)
        {
            return;
            //if(System.Environment.OSVersion.Version.Major == 6 && !IsCustomFormStyleEnabled())
            //    return;

            //WinApi.RECT rect = new WinApi.RECT(0, 0, width, height);
            //CreateParams params1 = this.CreateParams;
            //WinApi.AdjustWindowRectEx(ref rect, params1.Style, false, params1.ExStyle);

            //if ((specified & BoundsSpecified.Height) == BoundsSpecified.Height)
            //{
            //    if (System.Environment.OSVersion.Version.Major == 6 && this.FormBorderStyle != FormBorderStyle.Sizable && !this.DesignMode)
            //        height -= (rect.Height - height - m_BorderWidth * 2 - (SystemInformation.CaptionHeight + 9));
            //    else if (!this.ControlBox && this.FormBorderStyle == FormBorderStyle.FixedDialog && !this.DesignMode)
            //        height -= (rect.Height - height - m_BorderWidth * 2 - 1);
            //    else if (!this.ControlBox && this.FormBorderStyle == FormBorderStyle.Fixed3D && !this.DesignMode)
            //        height -= (rect.Height - height - m_BorderWidth * 2) + 2;
            //    else if (!this.ControlBox && this.FormBorderStyle == FormBorderStyle.FixedSingle && !this.DesignMode)
            //        height -= (rect.Height - height - m_BorderWidth * 2) + 2;
            //    else
            //        height -= (rect.Height - height - m_BorderWidth * 2 - (SystemInformation.CaptionHeight + 1) /*- (System.Environment.OSVersion.Version.Major == 6 ? SystemInformation.Border3DSize.Height : 0)*/);
            //}

            //if ((specified & BoundsSpecified.Width) == BoundsSpecified.Width)
            //{
            //    if (System.Environment.OSVersion.Version.Major == 6 && FormBorderStyle != FormBorderStyle.Sizable && !this.DesignMode)
            //        width -= (rect.Width - width - m_BorderWidth * 2 - 9);
            //    else if (!this.ControlBox && this.FormBorderStyle == FormBorderStyle.FixedDialog && !this.DesignMode)
            //        width -= (rect.Width - width - m_BorderWidth * 2 - 2) ;
            //    else if (!this.ControlBox && this.FormBorderStyle == FormBorderStyle.Fixed3D && !this.DesignMode)
            //        width -= (rect.Width - width - m_BorderWidth * 2)+2;
            //    else if (!this.ControlBox && this.FormBorderStyle == FormBorderStyle.FixedSingle && !this.DesignMode)
            //        width -= (rect.Width - width - m_BorderWidth * 2) + 2;
            //    else
            //        width -= (rect.Width - width - m_BorderWidth * 2 /*- (System.Environment.OSVersion.Version.Major == 6 ? SystemInformation.Border3DSize.Width * 2 : 0)*/);
            //}
        }

        /// <summary>
        /// Gets whether Vista glass effect extension over the ribbon control caption is enabled.
        /// </summary>
        protected override bool EnableGlassExtend
        {
            get
            {
                return false;
            }
        }

        protected override bool WindowsMessageNCCalcSize(ref Message m)
        {
            //return true;
            // Default implementation of ribbon form does not have non-client area at all...
            if (m.WParam == IntPtr.Zero)
            {
                WinApi.RECT r = (WinApi.RECT)Marshal.PtrToStructure(m.LParam, typeof(WinApi.RECT));

                WinApi.RECT newClientRect = WinApi.RECT.FromRectangle(GetClientRectangle(r.ToRectangle()));

                Marshal.StructureToPtr(newClientRect, m.LParam, false);
                m.Result = IntPtr.Zero;
            }
            else
            {
                WinApi.NCCALCSIZE_PARAMS csp;
                csp = (WinApi.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam, typeof(WinApi.NCCALCSIZE_PARAMS));

                WinApi.WINDOWPOS pos = (WinApi.WINDOWPOS)Marshal.PtrToStructure(csp.lppos, typeof(WinApi.WINDOWPOS));

                WinApi.RECT newClientRect = WinApi.RECT.FromRectangle(GetClientRectangle(new Rectangle(pos.x, pos.y, pos.cx, pos.cy)));
                csp.rgrc0 = newClientRect;
                csp.rgrc1 = newClientRect;
                Marshal.StructureToPtr(csp, m.LParam, false);
                m.Result = new IntPtr((int)WinApi.WindowsMessages.WVR_VALIDRECTS);
            }

            return false;
        }

        /// <summary>
        /// Called when WM_NCACTIVATE message is received.
        /// </summary>
        /// <param name="m">Reference to message data.</param>
        /// <returns>Return true to call base form implementation otherwise return false.</returns>
        protected override bool WindowsMessageNCActivate(ref Message m)
        {
            if (m.WParam != IntPtr.Zero)
                this.NonClientActive = true;
            else
                this.NonClientActive = false;
            CreateCashedCaptionBitmap();
            BaseWndProc(ref m);
            DrawCashedCaptionBitmap();
            //m.Result = new IntPtr(1);
            //bool ret = base.WindowsMessageNCActivate(ref m);
            PaintNonClientAreaBuffered();
            Redraw();
            //if (this.IsMdiContainer)
            //{
            //    this.Invalidate(new Rectangle(0, 0, this.Width, 32), true);
            //}
            //else
            //{
            //    this.Invalidate(new Rectangle(0, 0, this.Width, 2), true);
            //}
            //this.Update();
            return false;
        }

        #region Cashed Drawing of caption
        private BufferedBitmap m_CashedCaptionBitmap = null;
        private Point m_CashedCaptionBitmapLocation = Point.Empty;
        private void CreateCashedCaptionBitmap()
        {
            if (m_CashedCaptionBitmap != null)
            {
                m_CashedCaptionBitmap.Dispose();
                m_CashedCaptionBitmap = null;
            }
            if (m_Caption.DisplayRectangle.Width <= 0 || m_Caption.DisplayRectangle.Height <= 0 || !this.IsHandleCreated || this.IsDisposed)
                return;

            IntPtr dc = WinApi.GetWindowDC(this.Handle);
            try
            {
                m_CashedCaptionBitmap = new BufferedBitmap(dc, new Rectangle(m_CashedCaptionBitmapLocation.X, m_CashedCaptionBitmapLocation.Y,
                    m_Caption.DisplayRectangle.Width, m_Caption.DisplayRectangle.Height));
            }
            finally
            {
                WinApi.ReleaseDC(this.Handle, dc);
            }

            PaintFormCaptionTo(m_CashedCaptionBitmap.Graphics);
        }
        private void DrawCashedCaptionBitmap()
        {
            if (m_CashedCaptionBitmap == null)
                return;
            IntPtr dc = WinApi.GetWindowDC(this.Handle);
            using (Graphics g = Graphics.FromHdc(dc))
            {
                m_CashedCaptionBitmap.Render(g);
            }
            WinApi.ReleaseDC(this.Handle, dc);
            
            m_CashedCaptionBitmap.Dispose();
            m_CashedCaptionBitmap = null;
        }
        #endregion

        protected override Rectangle GetClientRectangle(Rectangle rect)
        {
            NonClientInfo nci = GetNonClientInfo();
            return new Rectangle(rect.X + nci.LeftBorder, rect.Y + nci.CaptionTotalHeight, 
                rect.Width - (nci.LeftBorder + nci.RightBorder), rect.Height - (nci.CaptionTotalHeight + nci.BottomBorder));

            //WinApi.RECT r = new WinApi.RECT(0, 0, rect.Width, rect.Height);
            //CreateParams params1 = this.CreateParams;
            //WinApi.AdjustWindowRectEx(ref r, params1.Style, false, params1.ExStyle);

            //return new Rectangle(Math.Abs(r.Left), Math.Abs(r.Top), rect.Width - (r.Width - rect.Width), rect.Height - (r.Height - rect.Height));

            
            //if (this.WindowState == FormWindowState.Maximized && this.IsMdiChild && System.Environment.OSVersion.Version.Major == 6)
            //{
            //    rect.Inflate(-(m_BorderWidth + SystemInformation.Border3DSize.Width + 3), -(m_BorderWidth + SystemInformation.Border3DSize.Height + 2));
            //    rect.Y += SystemInformation.CaptionHeight + 1;
            //    rect.Height -= SystemInformation.CaptionHeight + 1;
            //}
            //else
            //{
            //    rect.Inflate(-m_BorderWidth, -m_BorderWidth);
            //    rect.Y += SystemInformation.CaptionHeight + 1;
            //    rect.Height -= SystemInformation.CaptionHeight + 1;
            //}
            //return rect;
        }

        /// <summary>
        /// Gets the form path for the give input bounds.
        /// </summary>
        /// <param name="bounds">Reprents the form bounds.</param>
        /// <returns></returns>
        protected override GraphicsPath GetFormPath(Rectangle bounds)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle r = bounds;

            ArcData arc = ElementStyleDisplay.GetCornerArc(r, m_CornerSize, eCornerArc.TopLeft);
            path.AddArc(arc.X, arc.Y, arc.Width, arc.Height, arc.StartAngle, arc.SweepAngle);

            arc = ElementStyleDisplay.GetCornerArc(r, m_CornerSize, eCornerArc.TopRight);
            path.AddArc(arc.X, arc.Y, arc.Width, arc.Height, arc.StartAngle, arc.SweepAngle);

            path.AddLine(r.Right, r.Bottom - 3, r.Right, r.Bottom);
            path.AddLine(r.Right, r.Bottom, r.Right-3, r.Bottom);

            path.AddLine(r.X + 3, r.Bottom, r.X, r.Bottom);
            path.AddLine(r.X, r.Bottom, r.X, r.Bottom - 3);
            
            path.CloseAllFigures();

            return path;
        }

        private Point GetMousePosition()
        {
            if (this.IsDisposed)
                return Point.Empty;
            Point p = this.PointToClient(Control.MousePosition);
            p.X += m_BorderWidth;
            p.Y += m_Caption.HeightInternal; // SystemInformation.CaptionHeight;

            return p;
        }

        protected override bool WindowsMessageNCMouseMove(ref Message m)
        {
            Point p = GetMousePosition();

            m_Caption.InternalMouseMove(new MouseEventArgs(Control.MouseButtons, 0, p.X, p.Y, 0));

            if (m_SystemIcon.Tooltip != "" && m_Caption.HotSubItem == m_SystemIcon)
            {
                SetupHoverTimer();
            }

            if (Control.MouseButtons == MouseButtons.Left)
            {
                BaseItem item = m_Caption.ItemAtLocation(p.X, p.Y);
                if (item == m_TitleLabel)
                {
                    if (this.WindowState == FormWindowState.Normal)
                    {
                        const int HTCAPTION = 2;
                        byte[] bx = BitConverter.GetBytes(p.X);
                        byte[] by = BitConverter.GetBytes(p.Y);
                        byte[] blp = new byte[] { bx[0], bx[1], by[0], by[1] };
                        int lParam = BitConverter.ToInt32(blp, 0);
                        this.Capture = false;
                        NativeFunctions.SendMessage(this.Handle, NativeFunctions.WM_SYSCOMMAND, NativeFunctions.SC_MOVE + HTCAPTION, lParam);
                        return false;
                    }
                }
            }

            return true;
        }

        protected override bool WindowsMessageNCMouseLeave(ref Message m)
        {
            StopHoverTimer();
            m_Caption.InternalMouseLeave();
            return true;
        }

        private Timer m_HoverTimer = null;
        private void SetupHoverTimer()
        {
            if (m_HoverTimer != null)
            {
                m_HoverTimer.Stop();
                m_HoverTimer.Start();
                return;
            }
            m_HoverTimer = new Timer();
#if FRAMEWORK20
            m_HoverTimer.Interval = Math.Max(500, SystemInformation.MouseHoverTime);
#else
			m_HoverTimer.Interval = 600;
#endif
            m_HoverTimer.Tick += new EventHandler(HoverTimer_Tick);
            m_HoverTimer.Start();
        }

        private void HoverTimer_Tick(object sender, EventArgs e)
        {
            m_HoverTimer.Stop();
            if (m_SystemIcon != null && m_SystemIcon.Tooltip != "" && m_Caption != null && m_Caption.HotSubItem == m_SystemIcon)
            {
                m_Caption.InternalMouseHover();
            }
            StopHoverTimer();
        }

        private void StopHoverTimer()
        {
            Timer t = m_HoverTimer;
            if (m_HoverTimer != null)
            {
                m_HoverTimer = null;
                t.Stop();
                t.Tick -= new EventHandler(HoverTimer_Tick);
            }
        }

        protected override bool WindowsMessageNCHitTest(ref Message m)
        {
            // Get position being tested...
            int x = WinApi.LOWORD(m.LParam);
            int y = WinApi.HIWORD(m.LParam);

            Point p = PointToClient(new Point(x, y));
            p.X += m_BorderWidth;
            p.Y += m_BorderWidth + SystemInformation.CaptionHeight;

            int borderSize = m_BorderWidth;// SystemInformation.Border3DSize.Height;
            int cornerPadding = 2;

            if (IsSizable)
            {
                #if FRAMEWORK20
                if (this.RightToLeftLayout && this.RightToLeft == RightToLeft.Yes)
                #else
				if (this.RightToLeft == RightToLeft.Yes)
                #endif
                {
                    // Top-left corner
                    Rectangle r = new Rectangle(this.Width - (m_CornerSize + cornerPadding), 0, (m_CornerSize + cornerPadding), (m_CornerSize + cornerPadding));
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.TopLeftSizeableCorner);
                        return false;
                    }
                    // Top Right
                    r = new Rectangle(0, 0, (m_CornerSize + cornerPadding), (m_CornerSize + cornerPadding));
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.TopRightSizeableCorner);
                        return false;
                    }
                    // Bottom Left
                    r = new Rectangle(this.Width - (m_CornerSize + cornerPadding), this.Height - (m_CornerSize + cornerPadding), (m_CornerSize + cornerPadding), (m_CornerSize + cornerPadding));
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.BottomLeftSizeableCorner);
                        return false;
                    }
                    // Bottom Right
                    r = new Rectangle(0, this.Height - (m_CornerSize + cornerPadding), (m_CornerSize + cornerPadding), (m_CornerSize + cornerPadding));
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.BottomRightSizeableCorner);
                        return false;
                    }

                    // Left border
                    r = new Rectangle(this.Width - borderSize, 0, borderSize, this.Height);
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.LeftSizeableBorder);
                        return false;
                    }
                    // Right border
                    r = new Rectangle(0, 0, borderSize, this.Height);
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.RightSizeableBorder);
                        return false;
                    }
                }
                else
                {
                    // Top-left corner
                    Rectangle r = new Rectangle(0, 0, m_CornerSize + cornerPadding, m_CornerSize + cornerPadding);
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.TopLeftSizeableCorner);
                        return false;
                    }
                    // Top Right
                    r = new Rectangle(this.Width - (m_CornerSize + cornerPadding), 0, m_CornerSize + cornerPadding, m_CornerSize + cornerPadding);
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.TopRightSizeableCorner);
                        return false;
                    }
                    // Bottom Left
                    r = new Rectangle(0, this.Height - m_CornerSize, m_CornerSize + cornerPadding, m_CornerSize + cornerPadding);
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.BottomLeftSizeableCorner);
                        return false;
                    }
                    // Bottom Right
                    r = new Rectangle(this.Width - m_CornerSize, this.Height - m_CornerSize, m_CornerSize + cornerPadding, m_CornerSize + cornerPadding);
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.BottomRightSizeableCorner);
                        return false;
                    }

                    // Top border
                    r = new Rectangle(0, 0, this.Width, borderSize);
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.TopSizeableBorder);
                        return false;
                    }
                    // Bottom border
                    r = new Rectangle(0, this.Height - borderSize, this.Width, borderSize);
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.BottomSizeableBorder);
                        return false;
                    }
                    // Left border
                    r = new Rectangle(0, 0, borderSize, this.Height);
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.LeftSizeableBorder);
                        return false;
                    }
                    // Right border
                    r = new Rectangle(this.Width - borderSize, 0, borderSize, this.Height);
                    if (r.Contains(p))
                    {
                        m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.RightSizeableBorder);
                        return false;
                    }
                }
            }

            BaseItem item=m_Caption.ItemAtLocation(p.X, p.Y);
            if (item == m_TitleLabel)
            {
                m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.TitleBar);
                return false;
            }
            else if (item == m_SystemIcon)
            {
                m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.Menu);
                return false;
            }
            else if (item == m_SystemButtons)
            {
                SystemButton btn = m_SystemButtons.GetButton(p.X, p.Y);
                if (btn == SystemButton.Minimize)
                    m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.MinimizeButton);
                else if (btn == SystemButton.Maximize)
                    m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.MaximizeButton);
                else if (btn == SystemButton.Close)
                    m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.CloseButton);
                else if (btn == SystemButton.Restore)
                    m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.ReduceButton);
                else if (btn == SystemButton.Help)
                    m.Result = new IntPtr((int)WinApi.WindowHitTestRegions.HelpButton);

                return false;
            }

            return true;

        }

        protected override bool WindowsMessageNCLButtonDown(ref Message m)
        {
            if (m.WParam.ToInt32() == (int)WinApi.WindowHitTestRegions.TopSizeableBorder ||
                m.WParam.ToInt32() == (int)WinApi.WindowHitTestRegions.TopRightSizeableCorner) return true;
            Point p = GetMousePosition();
            m_Caption.InternalMouseDown(new MouseEventArgs(MouseButtons.Left, 0, p.X, p.Y, 0));

            BaseItem item = m_Caption.ItemAtLocation(p.X, p.Y);
            if (item!=null && (item == m_SystemButtons || item == m_TitleLabel || p.X>=m_SystemButtons.DisplayRectangle.X && p.X<=m_SystemButtons.DisplayRectangle.Right))
            {
                if (!this.NonClientActive)
                    this.Activate();
                return false;
            }

            //if (item == m_SystemIcon)
            //{
            //    p = new Point(m_SystemIcon.Bounds.X - m_BorderWidth, m_SystemIcon.Bounds.Bottom - SystemInformation.CaptionHeight);
            //    Point ps = PointToScreen(p);
            //    const int TPM_RETURNCMD = 0x0100;
            //    byte[] bx = BitConverter.GetBytes(p.X);
            //    byte[] by = BitConverter.GetBytes(p.Y);
            //    byte[] blp = new byte[] { bx[0], bx[1], by[0], by[1] };
            //    int lParam = BitConverter.ToInt32(blp, 0);
            //    this.Capture = false;
            //    NativeFunctions.SendMessage(this.Handle, NativeFunctions.WM_SYSCOMMAND, NativeFunctions.TrackPopupMenu(
            //        NativeFunctions.GetSystemMenu(this.Handle, false), TPM_RETURNCMD, ps.X, ps.Y, 0, this.Handle, IntPtr.Zero), lParam);
            //    return true;
            //}

            return true;
        }

        protected override bool WindowsMessageNCLButtonUp(ref Message m)
        {
            Point p = GetMousePosition();
            BaseItem item = m_Caption.ItemAtLocation(p.X, p.Y);

            m_Caption.InternalMouseUp(new MouseEventArgs(MouseButtons.Left, 0, p.X, p.Y, 0));

            if (item == m_SystemButtons)
                return false;

            return true;
        }

        private void SystemButtons_Click(object sender, EventArgs e)
        {
            SystemCaptionItem sci = sender as SystemCaptionItem;
            Form frm = this;

            if (sci.MouseDownButton == SystemButton.Minimize)
            {
                NativeFunctions.PostMessage(frm.Handle.ToInt32(), NativeFunctions.WM_SYSCOMMAND, NativeFunctions.SC_MINIMIZE, 0);
                m_Caption.InternalMouseLeave();
            }
            else if (sci.MouseDownButton == SystemButton.Maximize)
            {
                NativeFunctions.PostMessage(frm.Handle.ToInt32(), NativeFunctions.WM_SYSCOMMAND, NativeFunctions.SC_MAXIMIZE, 0);
                m_Caption.InternalMouseLeave();
            }
            else if (sci.MouseDownButton == SystemButton.Restore)
            {
                NativeFunctions.PostMessage(frm.Handle.ToInt32(), NativeFunctions.WM_SYSCOMMAND, NativeFunctions.SC_RESTORE, 0);
                m_Caption.InternalMouseLeave();
            }
            else if (sci.MouseDownButton == SystemButton.Close)
            {
                NativeFunctions.PostMessage(frm.Handle.ToInt32(), NativeFunctions.WM_SYSCOMMAND, NativeFunctions.SC_CLOSE, 0);
                m_Caption.InternalMouseLeave();
            }
            else if (sci.MouseDownButton == SystemButton.Help)
            {
                NativeFunctions.PostMessage(frm.Handle.ToInt32(), NativeFunctions.WM_SYSCOMMAND, NativeFunctions.SC_CONTEXTHELP, 0);
                m_Caption.InternalMouseLeave();
            }
        }

        private void SystemButtons_MouseEnter(object sender, EventArgs e)
        {
            if (m_MouseTimer == null)
                SetupMouseTimer();
        }

        private void SetupMouseTimer()
        {
            m_MouseTimer = new Timer();
            m_MouseTimer.Interval = 200;
            m_MouseTimer.Tick += new EventHandler(MouseTimer_Tick);
            m_MouseTimer.Start();
        }

        private void MouseTimer_Tick(object sender, EventArgs e)
        {
            Point p = GetMousePosition();
            if (m_SystemButtons != null && !m_SystemButtons.DisplayRectangle.Contains(p))
            {
                m_MouseTimer.Enabled = false;
                StopMouseTimer();

                m_SystemButtons.InternalMouseLeave();
            }
        }

        private void StopMouseTimer()
        {
            if (m_MouseTimer != null)
            {
                m_MouseTimer.Enabled = false;
                m_MouseTimer.Dispose();
                m_MouseTimer = null;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (!this.Visible)
                StopMouseTimer();
            base.OnVisibleChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            this.StopMouseTimer();
            base.OnClosed(e);
        }

        protected override void OnResize(EventArgs e)
        {
            RecalcSize();

            base.OnResize(e);
        }

        private class NonClientInfo
        {
            public int CaptionTotalHeight = 0;
            public int BottomBorder = 0;
            public int LeftBorder = 0;
            public int RightBorder = 0;
        }

        private NonClientInfo GetNonClientInfo()
        {
            WinApi.RECT rect = new WinApi.RECT(0, 0, 200, 200);
            CreateParams params1 = this.CreateParams;
            WinApi.AdjustWindowRectEx(ref rect, params1.Style, false, params1.ExStyle);

            NonClientInfo n = new NonClientInfo();
            n.CaptionTotalHeight = Math.Abs(rect.Top);
            n.BottomBorder = rect.Height - 200 - n.CaptionTotalHeight;
            n.LeftBorder = Math.Abs(rect.Left);
            n.RightBorder = rect.Width - 200 - n.LeftBorder;

            return n;
        }

        private void RecalcSize()
        {
            if (m_Caption != null)
            {
                NonClientInfo nci = GetNonClientInfo();
                m_Caption.LeftInternal = 1;
                m_Caption.TopInternal =0;
                m_Caption.PaddingLeft = nci.LeftBorder;
                m_Caption.PaddingRight = nci.RightBorder;
                m_Caption.PaddingTop = nci.BottomBorder;
                m_Caption.WidthInternal = this.Width - 1;
                m_Caption.HeightInternal = nci.CaptionTotalHeight;
                m_Caption.IsRightToLeft = (this.RightToLeft == RightToLeft.Yes);
                
                if (m_TitleLabel != null)
                {
                    m_TitleLabel.Height = m_Caption.HeightInternal - nci.BottomBorder-2;
                    if (m_TitleText != "")
                        m_TitleLabel.Width = 0;
                    else
                        m_TitleLabel.Width = 16;
                    m_TitleLabel.TextLineAlignment = StringAlignment.Center;
                }

                m_Caption.RecalcSize();
                m_Caption.HeightInternal = nci.CaptionTotalHeight; // SystemInformation.CaptionHeight + m_BorderWidth;
                if (m_SystemIcon != null)
                    m_SystemIcon.TopInternal += (m_Caption.HeightInternal - m_SystemIcon.HeightInternal - nci.BottomBorder - 2) / 2;

                //if (m_SystemButtons != null)
                //    m_SystemButtons.TopInternal = nci.BottomBorder;
            }

            if (this.WindowState == FormWindowState.Maximized || this.WindowState == FormWindowState.Minimized)
                m_SystemButtons.RestoreEnabled = true;
            else
                m_SystemButtons.RestoreEnabled = false;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            m_Caption.InternalMouseLeave();
            base.OnMouseLeave(e);
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            StopMouseTimer();
            m_Caption.InternalMouseLeave();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            UpdateFormText();
        }

        /// <summary>
        /// Updates the form title bar text. Usually calling this method is not necessary but under certain conditions when form is used as MDI parent form
        /// calling it to update combined text is necessary.
        /// </summary>
        public void UpdateFormText()
        {
            bool changed = false;
            if (m_TitleLabel != null && m_TitleText == "")
            {
                changed = m_TitleLabel.Text != this.Text;
                m_TitleLabel.Text = this.Text;
            }
            if (changed)
            {
                if (this.IsMdiChild && this.MdiParent is Office2007RibbonForm)
                {
                    ((Office2007RibbonForm)this.MdiParent).InvalidateRibbonCaption();
                }
                InvalidateNonClient(false);
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            if (e.Control is MdiClient)
            {
                e.Control.ControlRemoved += new ControlEventHandler(MdiClientControlRemoved);
                e.Control.ControlAdded += new ControlEventHandler(MdiClientControlAdded);
            }
            base.OnControlAdded(e);
        }

        private void MdiClientControlRemoved(object sender, ControlEventArgs e)
        {
            e.Control.Resize -= new EventHandler(MdiChildResize);
        }
        private void MdiClientControlAdded(object sender, ControlEventArgs e)
        {
            e.Control.Resize += new EventHandler(MdiChildResize);
        }
        
        protected override void OnMdiChildActivate(EventArgs e)
        {
            UpdateFormText();
            base.OnMdiChildActivate(e);
        }

        private void MdiChildResize(object sender, EventArgs e)
        {
            UpdateFormText();
        }

        protected override void OnStyleChanged(EventArgs e)
        {
            UpdateSystemButton();
            InvalidateNonClient(false);
            base.OnStyleChanged(e);
        }

        protected override void OnSystemColorsChanged(EventArgs e)
        {
            OnCaptionFontChanged();
            base.OnSystemColorsChanged(e);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            UpdateSystemButton();
            base.OnHandleCreated(e);
        }

        private void UpdateSystemButton()
        {
            if (m_SystemButtons != null)
            {
                m_SystemButtons.MinimizeVisible = this.MinimizeBox;
                m_SystemButtons.RestoreMaximizeVisible = this.MaximizeBox;

                m_SystemButtons.Visible = this.ControlBox;
                m_SystemIcon.Visible = this.ControlBox;

                m_SystemButtons.HelpVisible = this.HelpButton;
            }

#if FRAMEWORK20
            if (m_SystemIcon != null)
            {
                m_SystemIcon.Visible = this.ShowIcon;
            }
#endif
            if (this.FormBorderStyle == FormBorderStyle.FixedDialog)
                m_SystemIcon.Visible = false;

            if (this.IsHandleCreated)
                RecalcSize();
        }

        /// <summary>
        /// Gets or sets the icon for the form.
        /// </summary>
        [Browsable(true)]
        public new Icon Icon
        {
            get { return base.Icon; }
            set
            {
                base.Icon = value;
                m_SystemIcon.Icon = value;
                UpdateSystemButton();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the form enables auto scrolling.
        /// </summary>
        [Browsable(false), DefaultValue(false)]
        public override bool AutoScroll
        {
            get { return base.AutoScroll; }
            set { base.AutoScroll = value; }
        }

        protected bool CloseEnabled
        {
            get
            {
                return m_SystemButtons.CloseEnabled;
            }
            set
            {
                m_SystemButtons.CloseEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the Office 2007 Renderer global Color Table. Setting this property will affect all controls in application that are using Office 2007 global renderer.
        /// </summary>
        [Browsable(false), Category("Appearance"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Description("Indicates the Office 2007 Renderer global Color Table.")]
        public Rendering.eOffice2007ColorScheme Office2007ColorTable
        {
            get
            {
                Rendering.Office2007Renderer r = this.GetRenderer() as Rendering.Office2007Renderer;
                if (r != null)
                    return r.ColorTable.InitialColorScheme;
                return Rendering.eOffice2007ColorScheme.Blue;
            }
            set
            {
#if (FRAMEWORK20)
                RibbonPredefinedColorSchemes.ChangeOffice2007ColorTable(value);
#else
                RibbonPredefinedColorSchemes.ChangeOffice2007ColorTable(this, value);
#endif
            }
        }
        #endregion

        #region Non-client area painting
        /// <summary>
        /// Gets whether client border is painted in OnPaint method.
        /// </summary>
        protected override bool PaintClientBorder
        {
            get { return false; }
        }
        /// <summary>
        /// Gets whether ribbon control caption is painted
        /// </summary>
        protected override bool PaintRibbonCaption
        {
            get { return false; }
        }

        /// <summary>
        /// Paints the non-client area of the form.
        /// </summary>
        protected override bool WindowsMessageNCPaint(ref Message m)
        {
            m.Result = IntPtr.Zero;

            if (this.IsHandleCreated && !this.IsDisposed && this.Handle!=IntPtr.Zero && this.EnableCustomStyle)
            {
                PaintNonClientAreaBuffered();
            }

            return false;
        }

        //protected override bool WindowsMessageNCDblClk(ref Message m)
        //{
        //    Point p = GetMousePosition();
        //    if(m_SystemIcon.DisplayRectangle.Contains(p))
        //        NativeFunctions.PostMessage(this.Handle.ToInt32(), NativeFunctions.WM_SYSCOMMAND, NativeFunctions.SC_CLOSE, 0);
        //    return true;
        //}

        protected virtual void PaintNonClientAreaBuffered()
        {
            IntPtr dc = WinApi.GetWindowDC(this.Handle);
            try
            {
                using (Graphics g = Graphics.FromHdc(dc))
                {
                    PaintNonClientAreaBuffered(g);
                }
            }
            finally
            {
                WinApi.ReleaseDC(this.Handle, dc);
            }
        }

        protected virtual void PaintNonClientAreaBuffered(Graphics targetGraphics)
        {
            BufferedBitmap bmp = new BufferedBitmap(targetGraphics, new Rectangle(0,0,this.Width, this.Height));
            try
            {
                bmp.Graphics.SetClip(this.GetClientRectangle(new Rectangle(0, 0, this.Width, this.Height)), CombineMode.Exclude);
                PaintNonClientArea(bmp.Graphics);

                bmp.Render(targetGraphics, this.GetClientRectangle(new Rectangle(0, 0, this.Width, this.Height)));
            }
            finally
            {
                bmp.Dispose();
            }
        }

        protected virtual void PaintNonClientArea(Graphics g)
        {
            NonClientInfo nci = GetNonClientInfo();
            PaintFormBorder(g, nci.LeftBorder);
            Rectangle r = new Rectangle(1, 1, this.Width - 3, this.Height - 1);
            #if FRAMEWORK20
            if (this.RightToLeftLayout) r = new Rectangle(1, 1, this.Width - 2, this.Height - 1);
            #endif
            GraphicsPath path = GetFormPath(r);
            Region reg = new Region();
            reg.MakeEmpty();
            reg.Union(path);
            // Widen path for the border...
            path.Widen(SystemPens.Control);
            Region r2 = new Region(path);
            reg.Union(path);
            g.SetClip(reg, CombineMode.Replace);

            PaintFormCaptionTo(g);

            g.ResetClip();
        }

        private bool m_AntiAlias = true;
        /// <summary>
        /// Gets or sets whether anti-alias smoothing is used while painting form caption. Default value is true.
        /// </summary>
        [DefaultValue(true), Browsable(true), Category("Appearance"), Description("Gets or sets whether anti-aliasing is used while painting form caption.")]
        public bool CaptionAntiAlias
        {
            get { return m_AntiAlias; }
            set
            {
                if (m_AntiAlias != value)
                {
                    m_AntiAlias = value;
                }
            }
        }

        private void PaintFormCaptionTo(Graphics g)
        {
            ItemPaintArgs p = GetItemPaintArgs(g);
            if (p.Renderer != null)
            {
                if (p.Renderer is Office2007Renderer)
                {
                    if (NonClientActive)
                        m_TitleLabel.ForeColor = ((Office2007Renderer)p.Renderer).ColorTable.Form.Active.CaptionText;
                    else
                        m_TitleLabel.ForeColor = ((Office2007Renderer)p.Renderer).ColorTable.Form.Inactive.CaptionText;
                }
                
                p.Renderer.DrawFormCaptionBackground(new FormCaptionRendererEventArgs(g, m_Caption.DisplayRectangle, this));
            }

            SmoothingMode sm = g.SmoothingMode;
            System.Drawing.Text.TextRenderingHint th = g.TextRenderingHint;

            if (m_AntiAlias)
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = DisplayHelp.AntiAliasTextRenderingHint;
            }

            m_Caption.Paint(p);

            if (m_AntiAlias)
            {
                g.SmoothingMode = sm;
                g.TextRenderingHint = th;
            }
        }

        private ItemPaintArgs GetItemPaintArgs(Graphics g)
        {
            ItemPaintArgs pa = new ItemPaintArgs(null, this, g, GetColorScheme());
            pa.Renderer = this.GetRenderer();
            return pa;
        }

        protected virtual ColorScheme GetColorScheme()
        {
            if (m_ColorScheme == null)
            {
                if (GlobalManager.Renderer is Office2007Renderer)
                    return ((Office2007Renderer)GlobalManager.Renderer).ColorTable.LegacyColors;
                return new ColorScheme(eDotNetBarStyle.Office2007);
            }
            else
                return m_ColorScheme;
        }

        private Rendering.BaseRenderer m_DefaultRenderer = null;
        private Rendering.BaseRenderer m_Renderer = null;
        private eRenderMode m_RenderMode = eRenderMode.Global;
        /// <summary>
        /// Returns the renderer control will be rendered with.
        /// </summary>
        /// <returns>The current renderer.</returns>
        public virtual Rendering.BaseRenderer GetRenderer()
        {
            if (m_RenderMode == eRenderMode.Global && Rendering.GlobalManager.Renderer != null)
                return Rendering.GlobalManager.Renderer;
            else if (m_RenderMode == eRenderMode.Custom && m_Renderer != null)
                return m_Renderer;

            if (m_DefaultRenderer == null)
                m_DefaultRenderer = new Rendering.Office2007Renderer();

            return m_Renderer;
        }

        /// <summary>
        /// Gets or sets the redering mode used by control. Default value is eRenderMode.Global which means that static GlobalManager.Renderer is used. If set to Custom then Renderer property must
        /// also be set to the custom renderer that will be used.
        /// </summary>
        [Browsable(false), DefaultValue(eRenderMode.Global)]
        public eRenderMode RenderMode
        {
            get { return m_RenderMode; }
            set
            {
                if (m_RenderMode != value)
                {
                    m_RenderMode = value;
                    this.Invalidate(true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the custom renderer used by the items on this control. RenderMode property must also be set to eRenderMode.Custom in order renderer
        /// specified here to be used.
        /// </summary>
        [Browsable(false), DefaultValue(null)]
        public DevComponents.DotNetBar.Rendering.BaseRenderer Renderer
        {
            get
            {
                return m_Renderer;
            }
            set { m_Renderer = value; }
        }

        /// <summary>
        /// Gets or sets the tooltip for the form system icon.
        /// </summary>
        [Browsable(true), DefaultValue(""), Description("Indicates tooltip for the form system icon."), Localizable(true)]
        public string IconTooltip
        {
            get { return m_SystemIcon.Tooltip; }
            set { m_SystemIcon.Tooltip = value; }
        }

        ///// <summary>
        ///// Gets the array of LinearGradientColorTable objects that describe the border colors. The colors with index 0 is used as the outer most
        ///// border.
        ///// </summary>
        ///// <returns>Array of LinearGradientColorTable</returns>
        //protected override LinearGradientColorTable[] GetBorderColors()
        //{
        //    LinearGradientColorTable[] colors = base.GetBorderColors();

        //    RECT rect = new RECT(0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height);
        //    CreateParams params1 = this.CreateParams;
        //    AdjustWindowRectEx(ref rect, params1.Style, false, params1.ExStyle);

        //    int borderWidth = Math.Abs(rect.Left);
        //    if (colors.Length < borderWidth)
        //    {
        //        LinearGradientColorTable[] cn = new LinearGradientColorTable[borderWidth];
        //        LinearGradientColorTable innerColor = colors[colors.Length-1];
        //        colors.CopyTo(cn, 0);
        //        for (int i = colors.Length; i < borderWidth; i++)
        //            cn[i] = new LinearGradientColorTable(innerColor.Start, innerColor.End, innerColor.GradientAngle);
        //        colors = cn;
        //    }

        //    return colors;
        //}
        #endregion

        #region Windows API
        
        #endregion
    }
}
