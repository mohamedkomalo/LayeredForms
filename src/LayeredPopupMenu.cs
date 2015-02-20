using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using LayeredForms.Utilities;
using WinApiWrappers;

namespace LayeredForms
{
    public delegate void ItemClickedEventHandler(object sender, string path);

    public class LayeredPopupMenu : IDisposable
    {
        private const int MaxListCount = 18;
        private const int MaxWidth = 300;
        private const int MinWidth = 230;

        private static readonly StringFormat DefaultStringFormat = new StringFormat(StringFormatFlags.NoWrap)
        {
            Trimming = StringTrimming.EllipsisCharacter
        };

        private readonly VScrollBar _vScrollBar;
        private readonly PopupForm _form;

        private readonly List<LayeredPopupMenuItem> _items;
        private Rectangle _bounds;
        private Font _font;
        private Font _fontBold;

        public event EventHandler PopupMenuClosed;

        public event ItemClickedEventHandler ItemClicked;

        public LayeredPopupMenu()
        {
            _form = new PopupForm(this);

            _items = new List<LayeredPopupMenuItem>();

            _vScrollBar = new VScrollBar
            {
                Enabled = true,
                Minimum = 0,
                Value = 0,
                SmallChange = 1,
                LargeChange = 1
            };

            _vScrollBar.ValueChanged += ScrollBarOnValueChanged;

            _form.Controls.Add(_vScrollBar);
        }

        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        public Point ItemIconLocation { get; set; }

        public Padding ItemBackgroundImageStretchMargin { get; set; }

        public Padding ItemContent { get; set; }

        public int ItemContentRight
        {
            get { return _vScrollBar.Visible ? _vScrollBar.Width : 0; }
        }

        public ImageWithDeviceContext ItemBackgroundImage { get; set; }

        public ImageWithDeviceContext ItemHoveredBackgroundImage { get; set; }

        public ImageWithDeviceContext BackgroundImage { get; set; }

        public SolidBrush ForeColorBrush { get; set; }

        public Padding StretchMargin { get; set; }

        public VerticalAlignment ScrollbarAlighnemt { get; set; }

        public Padding ContentMargin { get; set; }

        public List<LayeredPopupMenuItem> Items
        {
            get { return _items; }
        }

        public int Left
        {
            get { return _bounds.X; }
            set { _bounds.X = value; }
        }

        public int Top
        {
            get { return _bounds.Y; }
            set { _bounds.Y = value; }
        }

        public int Width
        {
            get { return _bounds.Width; }
            set { _bounds.Width = value; }
        }

        public int Height
        {
            get { return _bounds.Height; }
            set { _bounds.Height = value; }
        }

        public Size Size
        {
            get { return _bounds.Size; }
            set { _bounds.Size = value; }
        }

        public Point Location
        {
            get { return _bounds.Location; }
            set { _bounds.Location = value; }
        }

        private int FirstScrollableItemIndex
        {
            get { return _vScrollBar.Value; }
        }

        private int LastScrollableItemIndex
        {
            get
            {
                int last = _vScrollBar.Value + MaxListCount - 1;

                if (last < MaxListCount)
                {
                    last = Items.Count - 1;
                }
                return last;
            }
        }

        public Font Font
        {
            get { return _font; }
            set
            {
                if (_font != null) _font.Dispose();

                if (_fontBold != null) _fontBold.Dispose();

                _font = value;
                _fontBold = new Font(value, FontStyle.Bold);
            }
        }

        private Font FontBold
        {
            get { return _fontBold; }
        }

        public bool Visible
        {
            get { return _form.Visible; }
            set { _form.Visible = value; }
        }

        private int ItemHeight
        {
            get { return ItemBackgroundImage.Height; }
        }

        protected void Dispose(bool disposing)
        {
            _form.Dispose();
        }

        public void Show()
        {
            if (Items.Count > MaxListCount)
            {
                _vScrollBar.Maximum = Items.Count - MaxListCount;
                _vScrollBar.Visible = true;
                _vScrollBar.Value = 0;
            }
            else
            {
                _vScrollBar.Visible = false;
            }

            Update();

            UpdateBounds();

            _form.Show();
            _form.Activate();
        }

        private void UpdateBounds()
        {
            int count = Items.Count > MaxListCount ? MaxListCount : Items.Count;

            Height = ItemHeight * count + ContentMargin.Top + ContentMargin.Bottom;

            Width = Width > MaxWidth ? MaxWidth : MinWidth;
            Width += ContentMargin.Right + ContentMargin.Left;

            _form.Bounds = _bounds;

            UpdateScrollBarBounds();
        }

        private void UpdateScrollBarBounds()
        {
            _vScrollBar.Top = ContentMargin.Top;
            if (ScrollbarAlighnemt == VerticalAlignment.left)
            {
                _vScrollBar.Left = ContentMargin.Left;
            }
            else
            {
                _vScrollBar.Left = Width - (_vScrollBar.Width + ContentMargin.Right);
            }
            _vScrollBar.Height = Height - (ContentMargin.Top + ContentMargin.Bottom);
        }

        private void ScrollBarOnValueChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void HandlePaintBackground(PaintEventArgs e)
        {
            var bufferImage = new ImageWithDeviceContext(Size);
            Graphics g = Graphics.FromHdc(bufferImage.Dc);

            DrawBackground(bufferImage);

            int last = LastScrollableItemIndex;

            var mousePos = _form.PointToClient(Cursor.Position);
            var currentHoverred = mousePos.Y / ItemHeight;

            int relativeIndex = 0;
            for (int i = FirstScrollableItemIndex; i <= last; i++)
            {
                LayeredPopupMenuItem item = Items[i];
                DrawItem(bufferImage, g, item, relativeIndex, currentHoverred == relativeIndex);

                relativeIndex++;
            }

            NativeMethods.BitBlt(e.Graphics.GetHdc(), 0, 0, Width, Height, bufferImage.Dc, 0, 0, CopyPixelOperation.SourceCopy);
            e.Graphics.ReleaseHdc();

            bufferImage.Dispose();
            g.Dispose();
        }

        protected virtual void DrawBackground(ImageWithDeviceContext bufferImage)
        {
            ImageRenderer.StretchImageWithOutMargin(bufferImage, 0, 0, Width, Height, BackgroundImage, 0,
                BackgroundImage.Width, BackgroundImage.Height, StretchMargin, 255);
        }

        protected virtual void DrawItem(ImageWithDeviceContext destImage, Graphics g, LayeredPopupMenuItem item, int index, bool hovered)
        {
            var bounds = new Rectangle();
            bounds.Width = Width - (ContentMargin.Right + ContentMargin.Left + ItemContentRight);
            bounds.Height = ItemBackgroundImage.Height;

            bounds.X = ContentMargin.Left;
            bounds.Y = ContentMargin.Top + index*bounds.Height;

            DrawImage(destImage, item, bounds, hovered);

            Rectangle textBounds = bounds;
            textBounds.X += ItemContent.Left;
            textBounds.Y += ItemContent.Top;
            textBounds.Width -= ItemContent.Left + ItemContent.Right + ItemContentRight;
            textBounds.Height -= ItemContent.Top + ItemContent.Bottom;

            DrawText(g, item, textBounds);

            if (hovered && item.HoveredImage != null)
            {
                g.DrawImage(item.HoveredImage, ItemIconLocation.X + bounds.X, ItemIconLocation.Y + bounds.Y);
            }
            else if (item.Image != null)
            {
                g.DrawImage(item.Image, ItemIconLocation.X + bounds.X, ItemIconLocation.Y + bounds.Y);
            }
        }

        private void DrawText(Graphics g, LayeredPopupMenuItem item, Rectangle textBounds)
        {
            Font font = item.Bold ? FontBold : Font;

            g.TextContrast = 0;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            g.DrawString(item.Text, font, ForeColorBrush, textBounds, DefaultStringFormat);
        }

        private void DrawImage(IDeviceContext destImage, LayeredPopupMenuItem item, Rectangle bounds, bool hovered)
        {
            ImageWithDeviceContext itemImage = ItemBackgroundImage;

            if (item.Checked || hovered)
            {
                itemImage = ItemHoveredBackgroundImage;
            }

            ImageRenderer.StretchImageWithOutMargin(destImage, bounds.X, bounds.Y, bounds.Width, bounds.Height,
                itemImage, 0, itemImage.Width, itemImage.Height,
                StretchMargin, 255);
        }

        private void HandleDeactivate(object sender, EventArgs e)
        {
            _form.Hide();
            OnPopupMenuClosed();
        }

        private void HandleMouseLeave(EventArgs e)
        {
            Update();
        }

        private void HandleMouseMove(MouseEventArgs e)
        {
            Update();
        }

        private void HandleMouseClick(MouseEventArgs e)
        {
            var mousePos = _form.PointToClient(Cursor.Position);
            var currentHoverred = mousePos.Y / ItemHeight;
            LayeredPopupMenuItem clicked = Items[currentHoverred];

            OnItemClicked((String)clicked.Tag);
        }

        protected virtual void OnItemClicked(string path)
        {
            ItemClickedEventHandler handler = ItemClicked;
            if (handler != null) handler(this, path);
        }

        private void Update()
        {
            _form.Invalidate();
        }

        protected virtual void OnPopupMenuClosed()
        {
            EventHandler handler = PopupMenuClosed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private class PopupForm : Form
        {
            private readonly LayeredPopupMenu _menu;

            public PopupForm(LayeredPopupMenu menu)
            {
                _menu = menu;
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ClassName = "#32768";
                    cp.Style = unchecked((int) WindowStyles.Popup) |
                               Convert.ToInt32(WindowStyles.ClipChildren | WindowStyles.ClipSiblings);
                    cp.ExStyle = Convert.ToInt32(WindowExtendedStyles.ToolWindow | WindowExtendedStyles.TopMost);
                    return cp;
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                _menu.HandlePaintBackground(e);
            }

            protected override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);
                _menu.HandleMouseClick(e);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                _menu.HandleMouseMove(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                _menu.HandleMouseLeave(e);
            }

            protected override void OnDeactivate(EventArgs e)
            {
                base.OnDeactivate(e);
                _menu.HandleDeactivate(null, e);
            }
        }
    }
}