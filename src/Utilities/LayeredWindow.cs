using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WinApiWrappers;

namespace LayeredForms.Utilities
{
    internal class LayeredWindow : Form
    {
        public LayeredWindow()
        {
            FormBorderStyle = FormBorderStyle.None;
        }

        public bool UpdateLayered(IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, int crKey)
        {
            BlendFunction f = BlendFunction.Default;
            return NativeMethods.UpdateLayeredWindow(Handle, hdcDst, ref pptDst, ref psize, hdcSrc, ref pprSrc, crKey, ref f,
                    (int)NativeMethods.UpdateLayeredWindowFlags.Alpha);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style = unchecked((int) (WindowStyles.Popup));
                cp.ExStyle =
                    Convert.ToInt32(WindowExtendedStyles.ToolWindow | WindowExtendedStyles.Layered | WindowExtendedStyles.NoActivate |
                                    WindowExtendedStyles.NoParentnotify | WindowExtendedStyles.NoInheritlayout);
                return cp;
            }
        }

        private static class NativeMethods
        {
            [Flags]
            public enum UpdateLayeredWindowFlags : int
            {
                ColorKey = 0x1,
                Alpha = 0x2,
                Opaque = 0x4
            }

            [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, int crKey, ref BlendFunction pblend, int dwFlags);
        }
    }
}