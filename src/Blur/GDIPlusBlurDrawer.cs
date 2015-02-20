using System;
using System.Drawing;
using LayeredForms.Utilities;

namespace LayeredForms.Blur
{
    
    // ReSharper disable once InconsistentNaming
    internal class GDIPlusBlurDrawer : IBlurDrawer
    {
        public void DrawBlurFromScreen(Action<IDeviceContext, Rectangle> sourcePainterFunc, IDeviceContext destImage, Rectangle screenBounds, Point destLocation, int blurStrength)
        {
            if (screenBounds.Size.Width < 1 || screenBounds.Size.Height < 1)
                return;
            try {
                Rectangle srcRect = new Rectangle(Point.Empty, screenBounds.Size);
                Rectangle distRect = new Rectangle(0, 0, srcRect.Width / blurStrength, srcRect.Height / blurStrength);

                using (GraphicsImage stretchImage = new GraphicsImage(srcRect.Size))
                {
                    using (GraphicsImage shrinkImage = new GraphicsImage(distRect.Size))
                    {
                        sourcePainterFunc(shrinkImage.Graphics, screenBounds);
                        shrinkImage.Graphics.ReleaseHdc();

                        shrinkImage.Graphics.DrawImage(stretchImage.Image, distRect, srcRect, GraphicsUnit.Pixel);

                        srcRect.Location = destLocation;
                        Graphics g = Graphics.FromHdc(destImage.GetHdc());
                        g.DrawImage(shrinkImage.Image, srcRect, distRect, GraphicsUnit.Pixel);
                        g.Dispose();
                        destImage.ReleaseHdc();
                    }
                }
            } catch
            {
            }
        }
    }
}
