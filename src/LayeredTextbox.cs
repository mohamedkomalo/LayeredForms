using System;
using System.Drawing;
using System.Windows.Forms;

namespace LayeredForms
{
    public delegate void EnterPressedEventHandler();

    public delegate void ExitTextChangeEventHandler();

    public delegate void StartTextChangeEventHandler();

    public delegate void TextChangedEventHandler();

    public class LayeredTextbox : LayeredButton
    {
        private LayeredTextboxWindow _textBox;

        public LayeredTextbox(LayeredForm parent) : base(parent)
        {
            Click += Me_Clicked;
            MyLayeredTextboxWindow = new LayeredTextboxWindow();
        }

        public virtual Color BackColor { get; set; }

        public virtual Color ForeColor { get; set; }

        protected LayeredTextboxWindow MyLayeredTextboxWindow
        {
            get { return _textBox; }
            set
            {
                if (_textBox != null)
                {
                    _textBox.EnterPressed -= OnEnterPressed;
                    _textBox.TextChanged -= OnTextChange;
                }
                _textBox = value;
                if (_textBox != null)
                {
                    _textBox.EnterPressed += OnEnterPressed;
                    _textBox.TextChanged += OnTextChange;
                }
            }
        }

        private void OnEnterPressed()
        {
            if (HoverState == ButtonState.Pressed)
            {
                Text = MyLayeredTextboxWindow.TextBoxControl.Text;
                if (EnterPressed != null)
                {
                    EnterPressed();
                }
            }
        }


        private void OnTextChange(object sender, EventArgs e)
        {
            if (HoverState == ButtonState.Pressed)
            {
                Text = MyLayeredTextboxWindow.TextBoxControl.Text;
                if (TextChanged != null)
                {
                    TextChanged();
                }
            }
        }

        private void Me_Clicked(object sender, MouseEventArgs e)
        {
            if (StartTextChange != null)
            {
                StartTextChange();
            }
            HoverState = ButtonState.Pressed;
            AdjustBoxSize();
            MyLayeredTextboxWindow.TextBoxControl.Text = Text;
            MyLayeredTextboxWindow.Show(Font, ForeColor, BackColor, TextVerticalAlignment);
            MyLayeredTextboxWindow.Deactivate += TextBox_DeActivated;
        }

        public void AdjustBoxSize()
        {
            MyLayeredTextboxWindow.PrepareElement(Font, ForeColor, BackColor, TextVerticalAlignment);
            MyLayeredTextboxWindow.Bounds = new Rectangle(Parent.Left + RealX + ContentPadding.Left,
                Parent.Top + RealY + ContentPadding.Top, Size.Width - (ContentPadding.Right + ContentPadding.Left),
                Size.Height - (ContentPadding.Bottom + ContentPadding.Top));
        }

        private void TextBox_DeActivated(object sender, EventArgs e)
        {
            MyLayeredTextboxWindow.Deactivate -= TextBox_DeActivated;
            Text = MyLayeredTextboxWindow.TextBoxControl.Text;
            HoverState = ButtonState.Hover;
            if (ExitTextChange != null)
            {
                ExitTextChange();
            }
            Parent.Update();
        }

        public event EnterPressedEventHandler EnterPressed;

        public event TextChangedEventHandler TextChanged;

        public event StartTextChangeEventHandler StartTextChange;

        public event ExitTextChangeEventHandler ExitTextChange;

        public override Cursor Cursor
        {
            get { return Cursors.IBeam; }
        }
    }
}