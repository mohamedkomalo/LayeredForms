using System.Drawing;

namespace LayeredForms.Utilities
{
    public class TitleRenderer
    {
        internal static void DrawText(string text, IDeviceContext destImage,
                                Point location, Size size, byte opacity,
                                SolidBrush textColorBrush, StringFormat stringFormat, Font font)
        {
            GraphicsImage textBuffer = new GraphicsImage(size.Width, size.Height);
            Rectangle drawBounds = new Rectangle(Point.Empty, size);
            SolidBrush brush = textColorBrush;
            Color oldColor = brush.Color;

            if (opacity != 255 && opacity < oldColor.A) {
                brush.Color = Color.FromArgb(opacity, brush.Color);
            }

            textBuffer.Graphics.DrawString(text, font, brush, drawBounds, stringFormat);

            drawBounds.Location = location;
            Graphics g = Graphics.FromHdc(destImage.GetHdc());
            g.DrawImage(textBuffer.Image, drawBounds);
            g.Dispose();
            destImage.ReleaseHdc();

            brush.Color = oldColor;
            textBuffer.Dispose();
        }

        public static int TextWidth(string text, Font font)
        {
            if (text.Length > 0)
            {
                // TextRenderering.MeasureText returns smaller width and Graphics.MeasureString returns a larger width
                return MeasureDisplayStringWidth(WidthCalculateGraphics, text, font);
            }
            return 0;
        }

        /*
         * Src
         * http://www.codeproject.com/Articles/2118/Bypass-Graphics-MeasureString-limitations#premain1
         */

        private static int MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
        {
            var format = new StringFormat();
            
            var rect = new RectangleF(0, 0, 1000, 1000);
            CharacterRange[] ranges = { new CharacterRange(0, text.Length) };

            format.SetMeasurableCharacterRanges(ranges);

            Region[] regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            rect = regions[0].GetBounds(graphics);

            return (int) (rect.Right + 1.0f);
        }

        private static readonly Graphics WidthCalculateGraphics = Graphics.FromImage(new Bitmap(1, 1));
    }
}