using System;
using System.Drawing;

namespace LayeredForms.Blur
{
    public class FastBlurDrawer : IBlurDrawer
    {

        public void DrawBlurFromScreen(Action<IDeviceContext, Rectangle> sourcePainterFunc, IDeviceContext destImage, Rectangle screenBounds, Point destLocation, int blurStrength)
        {
            if (screenBounds.Size.Width < 1 || screenBounds.Size.Height < 1)
                return;
            try
            {
                using(var blurImage = new Bitmap(screenBounds.Width, screenBounds.Height))
                {
                    using (var blurImageGraphics = Graphics.FromImage(blurImage))
                    {
                        sourcePainterFunc(blurImageGraphics, screenBounds);
                    }

                    ImageTools.FastBlur(blurImage, blurStrength);

                    using (Graphics destGraphics = Graphics.FromHdc(destImage.GetHdc()))
                    {
                        destGraphics.DrawImage(blurImage, new Rectangle(destLocation, screenBounds.Size),
                                                          new Rectangle(Point.Empty, screenBounds.Size), GraphicsUnit.Pixel);
                    }

                }

            }
            catch
            {
            }
        }
    }
}