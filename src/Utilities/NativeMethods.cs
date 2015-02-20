using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LayeredForms.Utilities
{
    [Flags]
    internal enum AlphaFormatFlags : byte
    {
        Over = 0x0,
        Alpha = 0x1
    }

    internal struct BlendFunction
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
        public BlendFunction(AlphaFormatFlags op, byte flags, byte alpha, AlphaFormatFlags format)
        {
            BlendOp = (byte)op;
            BlendFlags = flags;
            SourceConstantAlpha = alpha;
            AlphaFormat = (byte)format;
        }

        public static readonly BlendFunction Default = new BlendFunction(AlphaFormatFlags.Over, 0, 255, AlphaFormatFlags.Alpha);
    }

    internal static class NativeMethods
    {
        [DllImport("gdi32.dll", EntryPoint = "GdiAlphaBlend", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool AlphaBlend(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest,
            int nHeightDest, IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            BlendFunction blendFunction);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc,
            int nXSrc, int nYSrc, CopyPixelOperation dwRop);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDc);


        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hDc);

        [DllImport("gdi32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int DestroyIcon(IntPtr hIcon);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest,
            int nHeightDest, IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            CopyPixelOperation dwRop);

        [DllImport("Msimg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //Public Declare Auto Function SystemParametersInfo Lib "user32" Alias "SystemParametersInfoA" (ByVal uAction As Integer, ByVal uParam As Integer, ByVal lpvParam As Integer, ByVal fuWinIni As Integer) As Integer
        public static extern bool TransparentBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest,
            int nHeightDest, IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            int crTransparent);

        // ================================================================================================


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hDc, IntPtr hObject);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);
    }
}