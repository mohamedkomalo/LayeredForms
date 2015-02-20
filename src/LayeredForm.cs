using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LayeredForms.Blur;
using LayeredForms.Utilities;
using WinApiWrappers;

namespace LayeredForms
{
    public class LayeredForm : LayeredControl, IWin32Window
    {
        private LayeredWindow Form { get; set; }

        private int _animationRequests;

        private const int AnimationUpdateIntervalMilliseconds = 1;

        private readonly Timer _animationTimer = new Timer { Interval = AnimationUpdateIntervalMilliseconds };
        private readonly object _animationRequestsLock = new object();
        private bool _backgroundBlur;

        private static readonly ImageWithDeviceContext SharedBufferImage = new ImageWithDeviceContext(new Size(SystemInformation.MaxWindowTrackSize.Width + 100, SystemInformation.MaxWindowTrackSize.Height + 100));

        public LayeredForm() : base(null)
        {
            Form = new LayeredWindow();

            Form.MouseMove += base.LayeredWindowMouseMove;
            Form.MouseLeave += base.LayeredWindowMouseLeave;
            Form.MouseDown += base.LayeredWindowMouseDown;
            Form.MouseUp += base.LayeredWindowMouseUp;
            Form.MouseDoubleClick += base.LayeredWindowDoubleClick;
            Form.Activated += (sender, args) => UpdateVisibleWindows();

            _animationTimer.Tick += (o, e) => Update();
        }

        protected override void Dispose(bool disposing)
        {
            if (Form.InvokeRequired)
            {
                Form.Invoke(new Action(Form.Dispose));
            }
            else
            {
                Form.Dispose();
            }

            _animationTimer.Enabled = false;
            _animationTimer.Dispose();

            base.Dispose(disposing);
        }

        public new Rectangle Bounds
        {
            get { return new Rectangle(Location, Size); }
        }

        public new Point Location
        {
            get;
            set;
        }

        public int Left
        {
            get { return Location.X; }
            set { Location = new Point(value, Top); }
        }

        public int Top
        {
            get { return Location.Y; }
            set { Location = new Point(Left, value); }
        }

        private ImageWithDeviceContext BufferImage
        {
            get { return SharedBufferImage; }
        }

        internal Point PointToClient(Point point)
        {
            return Form.PointToClient(point);
        }

        public override void Update()
        {
            if (Form.InvokeRequired)
            {
                Form.Invoke(new Action(UpdateInternal));
            }
            else
            {
                UpdateInternal();
            }
        }

        private void DrawCustomBackgroundBlur(IDeviceContext destImage)
        {
            Rectangle blurScreenBounds = new Rectangle(Location, Size);
            blurScreenBounds.X += ContentPadding.Left;
            blurScreenBounds.Y += ContentPadding.Top;
            blurScreenBounds.Width -= ContentPadding.Right + ContentPadding.Left;
            blurScreenBounds.Height -= ContentPadding.Bottom + ContentPadding.Top;

            BlurDrawerProvider.GetBlurDrawer().DrawBlurFromScreen(PaintBehindWindow, destImage, blurScreenBounds,  new Point(ContentPadding.Left, ContentPadding.Top), BackgroundCustomBlurStrength);
        }

        public IntPtr ExcludeWindowBehindBlur { get; set; }

        private void UpdateInternal()
        {
            if (IsUpdateLocked() || !Visible)
                return;

            BufferImage.Clear(Bounds.Size);

            bool aeroEnabled = DesktopWindowManager.IsAeroEnabled() && !DesktopWindowManager.IsAeroOpaque();
            if (BackgroundBlur && !aeroEnabled)
            {
                DrawCustomBackgroundBlur(BufferImage);
            }

            Draw(BufferImage);

            if (Height == 0)
            {
                Height = 1;
            }

            Point location = Bounds.Location;
            Size size = Bounds.Size;
            IntPtr hdcSrc = BufferImage.Dc;
            Point point = Point.Empty;
            Form.UpdateLayered(BufferImage.Dc, ref location, ref size, hdcSrc, ref point, 0);

            if (BackgroundBlur && aeroEnabled)
            {
                var blurBounds = new Rectangle(ContentPadding.Left, ContentPadding.Top, Width - (ContentPadding.Right),
                    Height - (ContentPadding.Bottom));
                DesktopWindowManager.EnableBlurBehindWindow(this, blurBounds);
            }
        }

        public bool BackgroundBlur
        {
            get { return _backgroundBlur; }
            set
            {
                _backgroundBlur = value;

                if (!_backgroundBlur && DesktopWindowManager.IsAeroEnabled())
                {
                    DesktopWindowManager.DisableBlurBehindWindow(this);
                }
            }
        }

        public int BackgroundCustomBlurStrength { get; set; }


        public IntPtr Handle
        {
            get { return Form.Handle; }
        }


        public override Cursor Cursor
        {
            get { return Form.Cursor; }
            set { Form.Cursor = value; }
        }

        public bool Enabled
        {
            get { return Form.Enabled; }
            set { Form.Enabled = value; }
        }

        public bool Visible
        {
            get { return Form.Visible; }
            set { Form.Visible = value; }
        }

        public void RequestAnimationLoop()
        {
            lock (_animationRequestsLock)
            {
                _animationRequests++;

                if (!_animationTimer.Enabled)
                {
                    _animationTimer.Start();
                }
            }
        }

        public void EndAnimationLoopRequest()
        {
            lock (_animationRequestsLock)
            {
                _animationRequests--;

                if (_animationRequests < 0)
                    _animationRequests = 0;

                if (_animationRequests == 0)
                {
                    _animationTimer.Stop();
                }
            }
        }



        private void PaintBehindWindow(IDeviceContext destDc, Rectangle screenBounds)
        {

            if (!DesktopWindowManager.IsAeroEnabled())
            {
                TopLevelWindow desktopWindow = TopLevelWindow.DesktopWindow;
                IntPtr desktopDc = desktopWindow.GetHdc();

                NativeMethods.BitBlt(destDc.GetHdc(), 0, 0, screenBounds.Width, screenBounds.Height, desktopDc,
                    screenBounds.X, screenBounds.Y, CopyPixelOperation.SourceCopy);

                desktopWindow.ReleaseHdc();

                return;
            }

            IntPtr dc = destDc.GetHdc();

            for (int index = _visisbleWindows.Count - 1; index > -1; index--)
            {
                var window = _visisbleWindows[index];

                if (window.Handle == ExcludeWindowBehindBlur) continue;

                Rectangle windowBounds = window.Bounds;

                var inter = Rectangle.Intersect(screenBounds, windowBounds);

                if (inter == Rectangle.Empty) continue;

                var localPoint = inter.Location;
                NativeMethods.ScreenToClient(window.Handle, ref localPoint);

                var myLocalPoint = new Point(Math.Abs(screenBounds.X - inter.X), Math.Abs(screenBounds.Y - inter.Y));

                IntPtr windowDc = window.GetHdc();

                if (index == _visisbleWindows.Count - 2)
                {
                    NativeMethods.AlphaBlend(dc, myLocalPoint.X, myLocalPoint.Y, inter.Width, inter.Height, windowDc,
                        localPoint.X, localPoint.Y, inter.Width, inter.Height, BlendFunction.Default);
                }
                else
                {
                    NativeMethods.BitBlt(dc, myLocalPoint.X, myLocalPoint.Y, inter.Width, inter.Height, windowDc,
                        localPoint.X, localPoint.Y, CopyPixelOperation.SourceCopy);
                }

                window.ReleaseHdc();
            }
            destDc.ReleaseHdc();
        }

        private static List<TopLevelWindow> _visisbleWindows;

        private static void UpdateVisibleWindows()
        {
            _visisbleWindows = TopLevelWindow.Windows.FindAll(w => !w.Minimized && w.Visible && ((w.ExStyles & (int)WindowExtendedStyles.Layered) != (int)WindowExtendedStyles.Layered));
        }
    }
}