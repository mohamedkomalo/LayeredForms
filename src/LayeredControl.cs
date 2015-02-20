using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using LayeredForms.Utilities;

namespace LayeredForms
{
    public class LayeredControl : IDisposable
    {
        public event MouseEventHandler MouseEnter;

        public event MouseEventHandler MouseMove;

        public event EventHandler  MouseLeave;

        public event MouseEventHandler MouseDown;

        public event MouseEventHandler MouseUp;

        public event MouseEventHandler Click;

        public event EventHandler DoubleClick;

        protected bool Hovered;
        protected bool Hovering;

        protected bool Pressed;

        private bool _enabled = true;
        private bool _visible = true;

        private Point _location;
        private Size _size;

        private List<LayeredControl> _controls;

        private bool _disposedValue;

        public LayeredForm Parent { get; set; }

        public LayeredControl(LayeredForm parent)
        {
            _controls = new List<LayeredControl>();
            
            Opacity = 255;

            this.Parent = parent;

            if (parent != null)
                parent.Controls.Add(this);

            if(parent != null){
                MouseLeave += CursorHandlingOnMouseLeave;
                MouseMove += CursorHandlingOnMouseMove;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                foreach (LayeredControl c in Controls)
                {
                    c.Dispose();
                }

                _controls = null;

                Parent = null;
                Hovered = false;
                Pressed = false;

                _visible = false;
                _enabled = false;
            }
            _disposedValue = true;
        }

        public virtual bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public virtual bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public virtual Padding ContentPadding { get; set; }

        public virtual ImageWithDeviceContext BackgroundImage { get; set; }
        public virtual Padding BackgroundImageStretchExcludeMargin { get; set; }
        public byte Opacity { get; set; }

        public virtual VerticalAlignment VerticalAlignment { get; set; }
        
        public virtual Point Location
        {
            get { return _location; }
            set { _location = value; }
        }

        public virtual Size Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public Rectangle Bounds
        {
            get { return new Rectangle(Location, Size); }
        }

        protected int RealX
        {
            get
            {
                int realX = X;

                switch (VerticalAlignment)
                {
                    case VerticalAlignment.center:
                        realX = Parent.Width/2 + Width/2 - X;
                        break;
                    case VerticalAlignment.right:
                        realX = Parent.Width - (Width + X);
                        break;
                }

                return realX;
            }
        }

        protected int RealY
        {
            get { return Y; }
        }

        public int X
        {
            get { return Location.X; }
            set { Location = new Point(value, Y); }
        }

        public int Y
        {
            get { return Location.Y; }
            set { Location = new Point(X, value); }
        }

        public int Width
        {
            get { return Size.Width; }
            set { Size = new Size(value, Height); }
        }

        public int Height
        {
            get { return Bounds.Height; }
            set { Size = new Size(Width, value); }
        }

        public List<LayeredControl> Controls
        {
            get { return _controls; }
        }

        protected virtual void Draw(ImageWithDeviceContext destImage)
        {
            if (IsUpdateLocked() || !Visible)
                return;

            DrawBackground(destImage);

            DrawControls(destImage);
        }

        protected virtual void DrawBackground(ImageWithDeviceContext destImage)
        {
            if (BackgroundImage == null)
                return;

            ImageRenderer.StretchImageWithOutMargin(destImage, RealX, RealY, Size.Width, Size.Height,
                BackgroundImage, 0, BackgroundImage.Width, BackgroundImage.Height,
                BackgroundImageStretchExcludeMargin, Opacity);
        }

        protected virtual void DrawControls(ImageWithDeviceContext destImage)
        {
            try
            {
                foreach (LayeredControl c in Controls)
                {
                    c.Draw(destImage);
                }
            }
            catch (Exception)
            {
            }
        }

        public virtual void Update()
        {
            Parent.Update();
        }

        public bool IsPointInside(Point e)
        {
            if (!Enabled || !Visible) return false;

            return (e.X >= RealX + ContentPadding.Left &&
                    e.X <= RealX + (Size.Width - ContentPadding.Right) &&
                    e.Y >= RealY + ContentPadding.Top &&
                    e.Y <= RealY + (Size.Height - ContentPadding.Bottom));
        }

        private Cursor _cursor = Cursors.Default;
        private Cursor _originalParentCursor;

        public virtual Cursor Cursor
        {
            get { return _cursor; }
            set { _cursor = value; }
        }

        public void SuspendUpdate(bool suspendOrResume)
        {
            if (suspendOrResume)
            {
                SuspendUpdate();
            }
            else
            {
                ResumeUpdate();
            }
        }

        private int _lockCount;

        public void SuspendUpdate()
        {
            _lockCount++;
        }

        public void ResumeUpdate()
        {
            _lockCount--;
        }

        public bool IsUpdateLocked()
        {
            return _lockCount > 0;
        }

        private void CursorHandlingOnMouseMove(object sender, MouseEventArgs e)
        {
            if (Parent.Cursor != Cursor)
            {
                _originalParentCursor = Parent.Cursor;
                Parent.Cursor = Cursor;
            }
        }

        private void CursorHandlingOnMouseLeave(object sender, EventArgs args)
        {
            _originalParentCursor = Cursors.Default;
            if (Parent.Cursor != _originalParentCursor)
            {
                Parent.Cursor = _originalParentCursor;
            }
        }


        /**
         * ******************************************************
         */

        private static bool CheckMouseMove(LayeredControl control, MouseEventArgs e)
        {
            var isMouseInside = control.IsPointInside(e.Location);

            if (isMouseInside & !control.Hovered)
            {
                control.Hovered = true;
                control.OnMouseEnter(e);
            }
            else if (!isMouseInside & control.Hovered)
            {
                control.Hovered = false;
                control.OnMouseLeave();
            }
            else if (control.Hovered)
            {
                control.OnMouseMove(e);
            }

            if(e.Button == MouseButtons.None){
                CheckMouseUp(control, e);
            }

            return false;
        }

        private static bool CheckMouseDown(LayeredControl control, MouseEventArgs e)
        {
            bool isMouseInside = control.IsPointInside(e.Location);

            if (isMouseInside & !control.Pressed)
            {
                control.Pressed = true;

                control.OnMouseDown(e);

                _pressedControl = control;

                return true;
            }
            return false;
        }


        private static bool CheckMouseUp(LayeredControl control, MouseEventArgs e)
        {
            bool isMouseInside = control.IsPointInside(e.Location);

            if (control.Pressed)
            {
                control.Pressed = false;
                control.OnMouseUp(e);

                if (isMouseInside)
                {
                    control.OnClick(e);
                }

                _pressedControl = null;

                return true;
            }
            return false;
        }

        private static bool CheckDoubleClick(LayeredControl control, MouseEventArgs e)
        {
            bool isPointInside = control.IsPointInside(e.Location);
            if (isPointInside)
            {
                control.Pressed = false;
                _pressedControl = null;

                control.OnDoubleClick();
                return true;
            }
            return false;
        }

        private static bool CheckMouseLeave(LayeredControl control, EventArgs e)
        {
            control.Hovered = false;
            return false;
        }

        protected bool TraverseControlsInDepthFirst<T>(object sender, T e, Func<LayeredControl, T, bool> visitFunc) where T : EventArgs
        {
            for (int i = Controls.Count - 1; i >= 0; i += -1)
            {
                if (Controls[i].TraverseControlsInDepthFirst(sender, e, visitFunc))
                    return true;
            }

            return visitFunc(this, e);
        }

        protected void LayeredWindowMouseMove(object sender, MouseEventArgs e)
        {
            if (_pressedControl != null)
            {
                CheckMouseMove(_pressedControl, e);
            }
            else
            {
                TraverseControlsInDepthFirst(sender, e, CheckMouseMove);   
            }
        }

        protected void LayeredWindowMouseDown(object sender, MouseEventArgs e)
        {
            TraverseControlsInDepthFirst(sender, e, CheckMouseDown);
        }

        protected void LayeredWindowMouseUp(object sender, MouseEventArgs e)
        {
            TraverseControlsInDepthFirst(sender, e, CheckMouseUp);
        }

        protected void LayeredWindowDoubleClick(object sender, MouseEventArgs e)
        {
            TraverseControlsInDepthFirst(sender, e, CheckDoubleClick);
        }

        protected void LayeredWindowMouseLeave(object sender, EventArgs e)
        {
            TraverseControlsInDepthFirst(sender, e, CheckMouseLeave);
        }

        private static LayeredControl _pressedControl;



        private void OnMouseDown(MouseEventArgs e)
        {
            if (MouseDown != null)
            {
                MouseDown(this, e);
            }
        }

        protected virtual void OnMouseEnter(MouseEventArgs e)
        {
            if (MouseEnter != null)
            {
                MouseEnter(this, e);
            }
        }

        protected virtual void OnMouseLeave()
        {
            if (MouseLeave != null)
            {
                MouseLeave(this, new EventArgs());
            }
        }

        protected virtual void OnMouseMove(MouseEventArgs e)
        {
            if (MouseMove != null)
            {
                MouseMove(this, e);
            }
        }

        protected virtual void OnDoubleClick()
        {
            if (DoubleClick != null)
            {
                DoubleClick(this, new EventArgs());
            }
        }

        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            if (MouseUp != null)
            {
                MouseUp(this, e);
            }
        }

        protected virtual void OnClick(MouseEventArgs e)
        {
            if (Click != null)
            {
                Click(this, e);
            }
        }

    }
}