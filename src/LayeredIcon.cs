using System;
using System.Drawing;
using LayeredForms.Utilities;

namespace LayeredForms
{
    public class LayeredIcon : LayeredControl
    {

        public Bitmap IconBitmap;
        private GraphicsImage _iconBuffer;

        public LayeredIcon(LayeredForm parent)
            : base(parent)
        {
        }

        protected override void Draw(ImageWithDeviceContext destImage)
        {
            try {
                if (IconBitmap != null) {
                    Rectangle drawBounds = default(Rectangle);
                    drawBounds.Size = Size;

                    _iconBuffer.Graphics.Clear(Color.Transparent);
                    _iconBuffer.Graphics.DrawImage(IconBitmap, drawBounds);

                    drawBounds.Location = new Point(RealX, RealY);
                    Graphics g = Graphics.FromHdc(destImage.GetHdc());
                    g.DrawImage(_iconBuffer.Image, drawBounds);
                    g.Dispose();
                    destImage.ReleaseHdc();
                }
            } catch (Exception) {
            }
        }

        public override Size Size
        {
            get { return base.Size; }
            set
            {
                base.Size = value;
                if (_iconBuffer != null)
                {
                    _iconBuffer.Dispose();
                }
                _iconBuffer = new GraphicsImage(value);
            }
        }

        public Icon Icon {
            set
            {
                if (IconBitmap != null)
                    IconBitmap.Dispose();
                if (value != null)
                {
                    IconBitmap = value.ToBitmap();
                }
                else
                {
                    IconBitmap = null;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            IDisposable obj = IconBitmap;
            if (obj != null) {
                obj.Dispose();
                obj = null;
            }
            base.Dispose(disposing);
        }
    }
}
