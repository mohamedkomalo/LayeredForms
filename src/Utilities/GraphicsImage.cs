using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace LayeredForms.Utilities
{
    internal class GraphicsImage : IDisposable
    {
        public Graphics Graphics;
        public Bitmap Image;

        private bool _disposedValue = false;
        public GraphicsImage(Size size) : this(size.Width, size.Height)
        {
        }

        public GraphicsImage(int width, int height)
        {
            Image = new Bitmap(width, height);
            Graphics = Graphics.FromImage(Image);
            Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            if (SystemInformation.FontSmoothingType == 2) {
                Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue) {
                if (Image != null)
                {
                    Image.Dispose();
                    Image = null;
                }
                if (Graphics != null)
                {
                    Graphics.Dispose();
                    Graphics = null;
                }
            }
            _disposedValue = true;
        }

        #region " IDisposable Support "
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
