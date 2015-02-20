using System;
using System.Drawing;

namespace LayeredForms
{

    public class LayeredPopupMenuItem
    {
        public LayeredPopupMenuItem()
        {

        }

        public LayeredPopupMenuItem(string text, Object tag, Image image)
        {
            Image = image;
            Text = text;
            Tag = tag;
        }

        public Image Image { get; set; }
        public Image HoveredImage { get; set; }
        public String Text { get; set; }
        public bool Checked { get; set; }
        public bool Bold { get; set; }
        public Object Tag { get; set; }
    }
}
