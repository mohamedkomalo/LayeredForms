using System.Drawing;
using System.Windows.Forms;
using LayeredForms.Utilities;

namespace LayeredForms
{
    public class LayeredButton : LayeredImageButton
    {
        public enum SizingType
        {
            SizeToText,
            SizeToDefault,
            SizeToRightWidth
        }

        public int RightWidth;
        public SizingType SizeType = SizingType.SizeToText;

        public string Tag;
        public bool TextVisible = true;
        protected int TextWidth;
        private string _text;

        public LayeredButton(LayeredForm parent)
            : base(parent)
        {
        }

        public virtual Padding TextPadding { get; set; }

        private Rectangle TextBounds
        {
            get
            {
                Rectangle textBounds = new Rectangle();

                if (TextVerticalAlignment == VerticalAlignment.right && TextWidth < Size.Width)
                {
                    textBounds.X = RealX + (Size.Width - (TextWidth + TextPadding.Right));
                }
                else
                {
                    textBounds.X = RealX + TextPadding.Left;
                }

                textBounds.Y = RealY + TextPadding.Top;
                textBounds.Width = Size.Width - (TextPadding.Right + TextPadding.Left);
                textBounds.Height = Size.Height - (TextPadding.Bottom + TextPadding.Top);
                return textBounds;
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                TextWidth = TitleRenderer.TextWidth(Text, Font);
            }
        }

        public virtual StringFormat StringFormat { get; set; }

        public virtual SolidBrush TextColorBrush { get; set; }

        public virtual Font Font { get; set; }

        public virtual VerticalAlignment TextVerticalAlignment { get; set; }

        protected override void Draw(ImageWithDeviceContext destImage)
        {
            base.Draw(destImage);

            if (TextVisible)
            {
                DrawText(Opacity, destImage);
            }
        }

        private void DrawText(byte opacity, ImageWithDeviceContext destImage)
        {
            Rectangle textBounds = this.TextBounds;

            TitleRenderer.DrawText(Text, destImage,
                textBounds.Location, textBounds.Size,
                opacity, TextColorBrush,
                StringFormat, Font);
        }
    }
}