using System;
using System.Drawing;
using System.Windows.Forms;

namespace LayeredForms.Utilities
{
    // TODO: move LayeredForms.Rendering namespace to a seperate reusable componenet
    public class ImageRenderer
    {
        internal static void StretchImageWithOutMargin(IDeviceContext destImage, int destX, int destY, int destWidth, int destHeight, ImageWithDeviceContext srcImage, int srcY, int srcWidth, int srcHeight, Padding stretchExcludeMargin, byte opacity)
        {
            BlendFunction blend = BlendFunction.Default;
            blend.SourceConstantAlpha = opacity;
            IntPtr destDc = destImage.GetHdc();

            if (stretchExcludeMargin.Left > 0)
            {
                NativeMethods.AlphaBlend(destDc, destX, destY + stretchExcludeMargin.Top, stretchExcludeMargin.Left, destHeight - (stretchExcludeMargin.Top + stretchExcludeMargin.Bottom), srcImage.Dc, 0, srcY + stretchExcludeMargin.Top, stretchExcludeMargin.Left, srcHeight - (stretchExcludeMargin.Top + stretchExcludeMargin.Bottom),
                    blend);
            }

            if (stretchExcludeMargin.Right > 0)
            {
                NativeMethods.AlphaBlend(destDc, destX + (destWidth - stretchExcludeMargin.Right), destY + stretchExcludeMargin.Top, stretchExcludeMargin.Right, destHeight - (stretchExcludeMargin.Bottom + stretchExcludeMargin.Top), srcImage.Dc, srcWidth - stretchExcludeMargin.Right, srcY + stretchExcludeMargin.Top, stretchExcludeMargin.Right, srcHeight - (stretchExcludeMargin.Bottom + stretchExcludeMargin.Top),
                    blend);
            }

            if (stretchExcludeMargin.Top > 0)
            {
                NativeMethods.AlphaBlend(destDc, destX + stretchExcludeMargin.Left, destY, destWidth - (stretchExcludeMargin.Left + stretchExcludeMargin.Right), stretchExcludeMargin.Top, srcImage.Dc, stretchExcludeMargin.Left, srcY, srcWidth - (stretchExcludeMargin.Left + stretchExcludeMargin.Right), stretchExcludeMargin.Top,
                    blend);
                if (stretchExcludeMargin.Left > 0)
                {
                    NativeMethods.AlphaBlend(destDc, destX, destY, stretchExcludeMargin.Left, stretchExcludeMargin.Top, srcImage.Dc, 0, srcY, stretchExcludeMargin.Left, stretchExcludeMargin.Top,
                        blend);
                }

                if (stretchExcludeMargin.Right > 0)
                {
                    NativeMethods.AlphaBlend(destDc, (destWidth - stretchExcludeMargin.Right) + destX, destY, stretchExcludeMargin.Right, stretchExcludeMargin.Top, srcImage.Dc, srcWidth - stretchExcludeMargin.Right, srcY, stretchExcludeMargin.Right, stretchExcludeMargin.Top,
                        blend);
                }
            }

            if (stretchExcludeMargin.Bottom > 0)
            {
//                WinAPI.AlphaBlend(DestDC, DestX, DestY + (DestHeight - stretchExcludeMargin.Bottom), DestWidth, stretchExcludeMargin.Bottom, srcImage.DC, 0, SrcY + (srcHeight - stretchExcludeMargin.Bottom), srcWidth, stretchExcludeMargin.Bottom,
//                    Blend);

                NativeMethods.AlphaBlend(destDc, destX + stretchExcludeMargin.Left, destY + (destHeight - stretchExcludeMargin.Bottom),
                                          destWidth - (stretchExcludeMargin.Left + stretchExcludeMargin.Right),
                                          stretchExcludeMargin.Top,
                                  srcImage.Dc,
                                           stretchExcludeMargin.Left,
                                           srcY + (srcHeight - stretchExcludeMargin.Bottom),
                                           srcWidth - (stretchExcludeMargin.Left + stretchExcludeMargin.Right),
                                           stretchExcludeMargin.Bottom,
                    blend);

                if (stretchExcludeMargin.Left > 0)
                {
                    NativeMethods.AlphaBlend(destDc, destX, destY + (destHeight - stretchExcludeMargin.Bottom), stretchExcludeMargin.Left, stretchExcludeMargin.Bottom, srcImage.Dc, 0, srcY + (srcHeight - stretchExcludeMargin.Bottom), stretchExcludeMargin.Left, stretchExcludeMargin.Bottom,
                        blend);
                }

                if (stretchExcludeMargin.Right > 0)
                {
                    NativeMethods.AlphaBlend(destDc, (destWidth - stretchExcludeMargin.Right) + destX, destY + (destHeight - stretchExcludeMargin.Bottom), stretchExcludeMargin.Right, stretchExcludeMargin.Bottom, srcImage.Dc, srcWidth - stretchExcludeMargin.Right, srcY + (srcHeight - stretchExcludeMargin.Bottom), stretchExcludeMargin.Right, stretchExcludeMargin.Bottom,
                        blend);
                }
            }

            NativeMethods.AlphaBlend(destDc, destX + stretchExcludeMargin.Left, destY + stretchExcludeMargin.Top, destWidth - (stretchExcludeMargin.Left + stretchExcludeMargin.Right), destHeight - (stretchExcludeMargin.Top + stretchExcludeMargin.Bottom), srcImage.Dc, stretchExcludeMargin.Left, srcY + stretchExcludeMargin.Top, srcWidth - (stretchExcludeMargin.Left + stretchExcludeMargin.Right), srcHeight - (stretchExcludeMargin.Top + stretchExcludeMargin.Bottom),
                blend);
            destImage.ReleaseHdc();
        }

        public static void MaskOutCorners(ImageWithDeviceContext destImage, int destWidth,
                        Size frameSize, IDeviceContext maskOutBitmap, Padding stretchExcludeMargins, int maskOutColor, int nYOriginSrc)
        {
            // Doesn't maskout all the image, just the right and left exclude margins
            if (maskOutBitmap != null)
            {
                IntPtr destDc = destImage.GetHdc();
                IntPtr srcDc = maskOutBitmap.GetHdc();
                if (stretchExcludeMargins.Left > 0)
                {
                    NativeMethods.TransparentBlt(destDc, 0, 0, stretchExcludeMargins.Left, frameSize.Height, srcDc, 0,
                        nYOriginSrc, stretchExcludeMargins.Left, frameSize.Height,
                        maskOutColor);

                    //BitBlt(DestDC, 0, 0, StretchRect.Left, FrameHeight, _
                    //           MaskOutBitmap.DC, 0, Me.State(State), CopyPixelOperation.SourceAnd)
                }
                if (stretchExcludeMargins.Right > 0)
                {
                    NativeMethods.TransparentBlt(destDc, (destWidth - stretchExcludeMargins.Right) + 0, 0,
                        stretchExcludeMargins.Right, frameSize.Height, srcDc, frameSize.Width - (stretchExcludeMargins.Right),
                        nYOriginSrc, stretchExcludeMargins.Right, frameSize.Height,
                        maskOutColor);

                    //BitBlt(DestDC, (DestWidth - StretchRect.Right) + 0, _
                    //            0, StretchRect.Right, FrameHeight, _
                    //           MaskOutBitmap.DC, FrameWidth - (StretchRect.Right), _
                    //            Me.State(State), StretchRect.Right, FrameHeight, MaskOutColor)
                }

                //TransparentBlt(DestDC, _
                //           StretchRect.Left, StretchRect.Top, _
                //           DestWidth - (StretchRect.Left + StretchRect.Right), _
                //           FrameHeight, _
                //           Image.DC, _
                //           StretchRect.Left, Me.State(State) + +StretchRect.Top, _
                //           FrameWidth - (StretchRect.Left + StretchRect.Right), _
                //           FrameHeight, _
                //           MaskOutColor)
                destImage.ReleaseHdc();
                maskOutBitmap.ReleaseHdc();
            }
        }

    }
}
