using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinApiWrappers;

namespace LayeredForms
{
    public class LayeredTextboxWindow : Form
    {
        private const int MaxDropHeight = 250;
        private ComboBox _withEventsFieldTextBoxControl;

        public LayeredTextboxWindow()
        {
            Deactivate += On_DeActivate;
            FormBorderStyle = FormBorderStyle.None;

            TextBoxControl = new ComboBox();
            TextBoxControl.AutoCompleteMode = AutoCompleteMode.Suggest;
            TextBoxControl.AutoCompleteSource = AutoCompleteSource.FileSystem;
            TextBoxControl.Sorted = true;

            Controls.Add(TextBoxControl);
        }

        public ComboBox TextBoxControl
        {
            get { return _withEventsFieldTextBoxControl; }
            set
            {
                if (_withEventsFieldTextBoxControl != null)
                {
                    _withEventsFieldTextBoxControl.KeyDown -= Me_EnterPressed;
                    _withEventsFieldTextBoxControl.TextChanged -= TextControl_TextChanged;
                }
                _withEventsFieldTextBoxControl = value;
                if (_withEventsFieldTextBoxControl != null)
                {
                    _withEventsFieldTextBoxControl.KeyDown += Me_EnterPressed;
                    _withEventsFieldTextBoxControl.TextChanged += TextControl_TextChanged;
                }
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                unchecked
                {
                    cp.Style =
                        (int)
                            (WindowStyles.Popup | WindowStyles.ChildWindow | WindowStyles.ClipChildren |
                             WindowStyles.ClipSiblings);
                }
                cp.ExStyle = (int) (WindowExtendedStyles.ToolWindow | WindowExtendedStyles.TopMost);
                return cp;
            }
        }

        public new Rectangle Bounds
        {
            get { return base.Bounds; }
            set
            {
                base.Bounds = value;
                TopLevelWindow.FromHandle(Handle).Bounds = value;

                TextBoxControl.Location = Point.Empty;
                TextBoxControl.Size = Size;

                TopLevelWindow editControl = TopLevelWindow.FromHandle(TextBoxControl.Handle).FindControl("Edit");
                Rectangle editBounds = editControl.Bounds;
                editBounds.Location = PointToClient(editBounds.Location);
                TextBoxControl.Left = -editBounds.Left;
                TextBoxControl.Top = -editBounds.Top;
                TextBoxControl.Width = TextBoxControl.Width + (TextBoxControl.Width - editBounds.Width);
                TextBoxControl.Height = TextBoxControl.Height + (TextBoxControl.Height - editBounds.Height);
            }
        }

        public event EnterPressedEventHandler EnterPressed;

        public void On_DeActivate(object sender, EventArgs e)
        {
            Hide();
        }

        public void Show(Font font, Color foreColor, Color backColor, VerticalAlignment textVerticalAlignment)
        {
            PrepareElement(font, foreColor, backColor, textVerticalAlignment);
            base.Show();
            TextBoxControl.SelectAll();
        }

        public void Me_EnterPressed(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (EnterPressed != null)
                {
                    EnterPressed();
                }
            }
        }

        public void TextControl_TextChanged(object sender, EventArgs e)
        {
            OnTextChanged(e);
        }

        public void PrepareElement(Font font, Color foreColor, Color backColor, VerticalAlignment textVerticalAlignment)
        {
            if (textVerticalAlignment == VerticalAlignment.left)
            {
                RightToLeftLayout = false;
                TextBoxControl.RightToLeft = RightToLeft.No;
            }
            else
            {
                RightToLeftLayout = true;
                TextBoxControl.RightToLeft = RightToLeft.Yes;
            }

            TextBoxControl.Font = font;
            TextBoxControl.ForeColor = foreColor;
            TextBoxControl.BackColor = backColor;
        }


        public void ShowDropDown(string path, Font font, Color foreColor, Color backColor,
            VerticalAlignment textVerticalAlignment)
        {
            if (Directory.Exists(path))
            {
                PrepareElement(font, foreColor, backColor, textVerticalAlignment);
                TextBoxControl.Items.Clear();
                TextBoxControl.Items.AddRange(Directory.GetDirectories(path));
                TextBoxControl.Items.AddRange(Directory.GetFiles(path));

                int newHeight = TextBoxControl.ItemHeight*(TextBoxControl.Items.Count);
                if (newHeight > MaxDropHeight)
                {
                    newHeight = MaxDropHeight;
                }
                else if (newHeight == 0)
                {
                    newHeight = 100;
                }
                TextBoxControl.DropDownHeight = newHeight;
                TextBoxControl.DroppedDown = true;
                TextBoxControl.SelectionChangeCommitted += OnSelectedIndexChanged;
            }
        }

        public void OnSelectedIndexChanged(object sender, EventArgs e)
        {
            TextBoxControl.Text = TextBoxControl.SelectedItem.ToString();
            if (EnterPressed != null)
            {
                EnterPressed();
            }
            TextBoxControl.SelectionChangeCommitted -= OnSelectedIndexChanged;
        }
    }
}