using System;
using System.Drawing;
using WinApiWrappers;

namespace LayeredForms.Utilities
{
    public class ImageWithDeviceContext : IDeviceContext
    {
        private IntPtr _dc;
        private IntPtr _bitmap;
        private Size _size;

        private string _fileName;

        private bool _disposedValue;

        public ImageWithDeviceContext(string imageFile)
        {
            Bitmap tempBitmap = new Bitmap(imageFile);
            InitializeBitmap(tempBitmap.GetHbitmap(Color.Black), tempBitmap.Size);
            _fileName = imageFile;
            tempBitmap.Dispose();
        }

        public ImageWithDeviceContext(Bitmap bitmap)
        {
            InitializeBitmap(bitmap.GetHbitmap(Color.Black), bitmap.Size);
        }

        public ImageWithDeviceContext(Size size) : this(size.Width, size.Height)
        {
        }

        public ImageWithDeviceContext(int width, int height)
        {
            var desktop = TopLevelWindow.DesktopWindow;
            IntPtr screenDc = desktop.GetHdc();
            IntPtr emptyBitmap = NativeMethods.CreateCompatibleBitmap(screenDc, width, height);

            InitializeBitmap(emptyBitmap, new Size(width, height));
            desktop.ReleaseHdc();
        }

        public ImageWithDeviceContext(IntPtr hBitmap, Size size)
        {
            InitializeBitmap(hBitmap, size);
        }

        private void InitializeBitmap(IntPtr hBitmap, Size bitmapSize)
        {
            _dc = NativeMethods.CreateCompatibleDC(IntPtr.Zero);
            _bitmap = hBitmap;
            _size = bitmapSize;
            NativeMethods.SelectObject(_dc, _bitmap);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue) {
                if (disposing) {
                }
                NativeMethods.DeleteDC(_dc);
                NativeMethods.DeleteObject(_bitmap);
            }
            _disposedValue = true;
        }

        public void Clear()
        {
            Clear(Size);
        }

        public void Clear(Size size)
        {
            Clear(0, 0, size.Width, size.Height);
        }

        public void Clear(Rectangle retangle)
        {
            NativeMethods.BitBlt(Dc, retangle.X, retangle.Y, retangle.Width, retangle.Height, Dc, 0, 0, CopyPixelOperation.Blackness);
        }

        public void Clear(int left, int top, int width, int height)
        {
            NativeMethods.BitBlt(Dc, left, top, width, height, Dc, 0, 0, CopyPixelOperation.Blackness);
        }

        public void AlphaBlendImage(ImageWithDeviceContext sourceImage, Point srcLocation, Point destLocation, Size size, byte opacity)
        {
            AlphaBlendImage(sourceImage, new Rectangle(srcLocation, size), new Rectangle(destLocation, size), opacity);
        }

        public void AlphaBlendImage(ImageWithDeviceContext sourceImage, Size srcSize, byte opacity)
        {
            Rectangle bounds = new Rectangle(Point.Empty, srcSize);
            AlphaBlendImage(sourceImage, bounds, bounds,  opacity);
        }

        public void AlphaBlendImage(ImageWithDeviceContext sourceImage, Rectangle srcRectangle, byte opacity)
        {
            AlphaBlendImage(sourceImage, srcRectangle, new Rectangle(Point.Empty, srcRectangle.Size),  opacity);
        }

        public void AlphaBlendImage(ImageWithDeviceContext sourceImage, Rectangle srcRectangle, Point destLocation, byte opacity)
        {
            AlphaBlendImage(sourceImage, srcRectangle, new Rectangle(destLocation, srcRectangle.Size),  opacity);
        }

        public void AlphaBlendImage(ImageWithDeviceContext sourceImage, Rectangle srcRectangle, Rectangle destRectangle, byte opacity)
        {
            BlendFunction f = BlendFunction.Default;
            f.SourceConstantAlpha = opacity;
            NativeMethods.AlphaBlend(Dc, destRectangle.X, destRectangle.Y, destRectangle.Width, destRectangle.Height, sourceImage.Dc, srcRectangle.X, srcRectangle.Y, srcRectangle.Width, srcRectangle.Height,
                f);
        }

        public void BitBltImage(ImageWithDeviceContext sourceImage, Point srcLocation, Point destLocation, Size size)
        {
            NativeMethods.BitBlt(Dc, destLocation.X, destLocation.Y, size.Width, size.Height, sourceImage.Dc, srcLocation.X, srcLocation.Y, CopyPixelOperation.SourceCopy);
        }

        public void BitBltImage(ImageWithDeviceContext sourceImage, Size size)
        {
            NativeMethods.BitBlt(Dc, 0, 0, size.Width, size.Height, sourceImage.Dc, 0, 0, CopyPixelOperation.SourceCopy);
        }

        public void BitBltImage(ImageWithDeviceContext sourceImage, Rectangle destRectangle, Point srcLocation)
        {
            NativeMethods.BitBlt(Dc, destRectangle.X, destRectangle.Y, destRectangle.Width, destRectangle.Height, sourceImage.Dc, srcLocation.X, srcLocation.Y, CopyPixelOperation.SourceCopy);
        }

        public IntPtr Dc {
            get { return _dc; }
        }

        public IntPtr HBitmap {
            get { return _bitmap; }
        }

        public Size Size {
            get { return _size; }
        }

        public int Width {
            get { return Size.Width; }
        }

        public virtual int Height {
            get { return Size.Height; }
        }

        public virtual string FilePath {
            get { return _fileName; }
        }

        public IntPtr GetHdc()
        {
            return _dc;
        }

        public void ReleaseHdc()
        {
            //using an off-screen bitmap, so it has no need to be released
        }
    }
}
