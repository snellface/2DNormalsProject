using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;

namespace _2DNormalCalculator
{
    public partial class XNAFrame : UserControl
    {
        public enum eRefreshMode
        {
            Always,
            OnPanelPaint,
        }

        private GraphicsDevice mDevice;
        public GraphicsDevice Device
        {
            get { return mDevice; }
        }
        private eRefreshMode mRefreshMode = eRefreshMode.Always;
        public eRefreshMode RefreshMode
        {
            get { return mRefreshMode; }
            set
            {
                mRefreshMode = value;
            }
        }
        private Microsoft.Xna.Framework.Graphics.Color mBackColor = Microsoft.Xna.Framework.Graphics.Color.HotPink;

        public void Begin()
        {
            mDevice.Clear(Color.Black);
        }

        public void Present()
        {
            mDevice.Present();
        }

        #region Events
        public delegate void GraphicsDeviceDelegate(GraphicsDevice pDevice);
        public delegate void EmptyEventHandler();
        public event GraphicsDeviceDelegate OnFrameRender = null;
        public event GraphicsDeviceDelegate OnFrameMove = null;
        public event EmptyEventHandler DeviceResetting = null;
        public event GraphicsDeviceDelegate DeviceReset = null;
        #endregion

        public XNAFrame()
        {
            InitializeComponent();
        }

        #region XNA methods

        private void XNAFrame_Load(object sender, EventArgs e)
        {
            CreateGraphicsDevice();

            ResetGraphicsDevice();
        }

        private void CreateGraphicsDevice()
        {
            // Create Presentation Parameters
            PresentationParameters pp = new PresentationParameters();
            pp.BackBufferCount = 1;
            pp.IsFullScreen = false;
            pp.SwapEffect = SwapEffect.Discard;
            pp.BackBufferWidth = this.Width;
            pp.BackBufferHeight = this.Height;
            pp.AutoDepthStencilFormat = DepthFormat.Depth24Stencil8;
            pp.EnableAutoDepthStencil = true;
            pp.PresentationInterval = PresentInterval.Default;
            pp.BackBufferFormat = SurfaceFormat.Unknown;
            pp.MultiSampleType = MultiSampleType.None;

            // Create device
            mDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, DeviceType.Hardware, this.Handle, pp);
        }

        private void ResetGraphicsDevice()
        {
            // Avoid entering until panelViewport is setup and device created
            if (mDevice == null || this.Width == 0 || this.Height == 0)
                return;

            if (this.DeviceResetting != null)
                this.DeviceResetting();

            // Reset device
            mDevice.PresentationParameters.BackBufferWidth = this.Width;
            mDevice.PresentationParameters.BackBufferHeight = this.Height;
            mDevice.Reset();

            if (this.DeviceReset != null)
                this.DeviceReset(this.mDevice);
        }

        public void Render()
        {
            if (this.OnFrameMove != null)
                this.OnFrameMove(this.mDevice);

            mDevice.Clear(this.mBackColor);

            if (this.OnFrameRender != null)
                this.OnFrameRender(this.mDevice);

            mDevice.Present();

        }

        private void OnViewportResize(object sender, EventArgs e)
        {
            ResetGraphicsDevice();
        }

        private void OnVieweportPaint(object sender, PaintEventArgs e)
        {
            if (this.mRefreshMode != eRefreshMode.Always)
                this.Render();
        }

        private void panelViewport_BackColorChanged(object sender, EventArgs e)
        {
            this.mBackColor = new Microsoft.Xna.Framework.Graphics.Color(this.BackColor.R, this.BackColor.G, this.BackColor.B);
        }
        #endregion
    }
}
