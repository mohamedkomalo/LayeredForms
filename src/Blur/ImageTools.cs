/*
 * Original Code
 *      Author  : Mario Klingemann (https://github.com/Quasimondo)
 *      Website : http://incubator.quasimondo.com/
 *      Code    : http://incubator.quasimondo.com/processing/fast_blur_deluxe.php
 * 
 * Ported to C#
 *      Author  : Iain Ballard (https://github.com/i-e-b):
 *      Website : http://i-e-b.net/ 
 *      Code    : http://snippetsfor.net/csharp/StackBlur
 * 
 * Modified by Mohamed Kamal Kamaly to blur inplace
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace LayeredForms.Blur
{
    internal class ImageTools
    {
        public static void FastBlur(Bitmap sourceImage, int radius)
        {
            unsafe
            {
                var rct = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);

                BitmapData bits = sourceImage.LockBits(rct, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                int* source = (int*)bits.Scan0;

                if (radius < 1) return;

                int w = rct.Width;
                int h = rct.Height;
                int wm = w - 1;
                int hm = h - 1;
                int wh = w*h;
                int div = radius + radius + 1;
                var r = new int[wh];
                var g = new int[wh];
                var b = new int[wh];
                int rsum, gsum, bsum, x, y, i, p1, p2, yi;
                var vmin = new int[Max(w, h)];
                var vmax = new int[Max(w, h)];

                var dv = new int[256*div];
                for (i = 0; i < 256*div; i++)
                {
                    dv[i] = (i/div);
                }

                int yw = yi = 0;

                for (y = 0; y < h; y++)
                {
                    // blur horizontal
                    rsum = gsum = bsum = 0;
                    for (i = -radius; i <= radius; i++)
                    {
                        int p = source[yi + Min(wm, Max(i, 0))];
                        rsum += (p & 0xff0000) >> 16;
                        gsum += (p & 0x00ff00) >> 8;
                        bsum += p & 0x0000ff;
                    }
                    for (x = 0; x < w; x++)
                    {

                        r[yi] = dv[rsum];
                        g[yi] = dv[gsum];
                        b[yi] = dv[bsum];

                        if (y == 0)
                        {
                            vmin[x] = Min(x + radius + 1, wm);
                            vmax[x] = Max(x - radius, 0);
                        }
                        p1 = source[yw + vmin[x]];
                        p2 = source[yw + vmax[x]];

                        rsum += ((p1 & 0xff0000) - (p2 & 0xff0000)) >> 16;
                        gsum += ((p1 & 0x00ff00) - (p2 & 0x00ff00)) >> 8;
                        bsum += (p1 & 0x0000ff) - (p2 & 0x0000ff);
                        yi++;
                    }
                    yw += w;
                }

                for (x = 0; x < w; x++)
                {
                    // blur vertical
                    rsum = gsum = bsum = 0;
                    int yp = -radius*w;
                    for (i = -radius; i <= radius; i++)
                    {
                        yi = Max(0, yp) + x;
                        rsum += r[yi];
                        gsum += g[yi];
                        bsum += b[yi];
                        yp += w;
                    }
                    yi = x;
                    for (y = 0; y < h; y++)
                    {
                        source[yi] =
                            (int) (0xff000000u | (uint) (dv[rsum] << 16) | (uint) (dv[gsum] << 8) | (uint) dv[bsum]);
                        if (x == 0)
                        {
                            vmin[y] = Min(y + radius + 1, hm)*w;
                            vmax[y] = Max(y - radius, 0)*w;
                        }
                        p1 = x + vmin[y];
                        p2 = x + vmax[y];

                        rsum += r[p1] - r[p2];
                        gsum += g[p1] - g[p2];
                        bsum += b[p1] - b[p2];

                        yi += w;
                    }
                }

                sourceImage.UnlockBits(bits);
            }
        }

        private static int Min(int a, int b)
        {
            return Math.Min(a, b);
        }

        private static int Max(int a, int b)
        {
            return Math.Max(a, b);
        }
    }
}