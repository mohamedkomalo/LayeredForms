using System;
using System.Drawing;
using LayeredForms.Utilities;

namespace LayeredForms
{
    public class LayeredLabel : LayeredControl
    {
        private string _text = "";

        public LayeredLabel(LayeredForm parent) : base(parent)
        {
        }

        protected override void DrawBackground(ImageWithDeviceContext destImage)
        {
            base.DrawBackground(destImage);

            if (Text.Length > 0)
            {

                Rectangle textBounds = Bounds;    

                TitleRenderer.DrawText(Text, destImage,
                    new Point(RealX, RealY), textBounds.Size,
                    Opacity, TextColorBrush,
                    StringFormat, Font);
            }
        }

        public virtual StringFormat StringFormat { get; set; }

        public virtual SolidBrush TextColorBrush { get; set; }

        public virtual Font Font { get; set; }

        public virtual String Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public override Size Size
        {
            get { return new Size(TitleRenderer.TextWidth(Text, Font), Font.Height); }
        }

    }
}
