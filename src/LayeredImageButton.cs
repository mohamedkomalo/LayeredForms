using System;
using System.Windows.Forms;
using LayeredForms.Utilities;

namespace LayeredForms
{
    public class LayeredImageButton : LayeredControl
    {

        protected int HoverAlpha;
        protected int HoverSpeed;
        protected int HoverEnd;

        private bool _animationRequested;

        private ButtonState _hoverState = ButtonState.Hover;
        private ButtonState _forceState = ButtonState.None;

        private const int HoverIn = 1;
        private const int HoverOut = -1;
        private const int DefaultHoverSpeed = 28;

        public LayeredImageButton(LayeredForm parent)
            : base(parent)
        {
            MouseUp += OnMouseUp;
            MouseDown += OnMouseDown;
            MouseLeave += OnMouseLeave;
            MouseEnter += OnMouseEnter;
        }

        protected override void DrawBackground(ImageWithDeviceContext destImage)
        {
            if (!Enabled)
            {
                base.BackgroundImage = DisabledBackgroundImage;
            }
            else if (_forceState == ButtonState.Hover || (Hovered && !_animationRequested))
            {
                base.BackgroundImage = HoverBackgroundImage;
            }
            else if (_forceState == ButtonState.Pressed)
            {
                base.BackgroundImage = PressedBackgroundImage;
            }
            else
                base.BackgroundImage = BackgroundImage;

            base.DrawBackground(destImage);


            if (_animationRequested)
            {
                byte originalOpacity = Opacity;

                HoverAlpha += HoverSpeed;

                if (HoverAlpha < 0 || HoverAlpha > 255) HoverAlpha = HoverEnd;

                Opacity = (byte)HoverAlpha;

                if (_hoverState == ButtonState.Hover)
                    base.BackgroundImage = HoverBackgroundImage;
                else if (_hoverState == ButtonState.Pressed)
                    base.BackgroundImage = PressedBackgroundImage;

                base.DrawBackground(destImage);

                if (HoverAlpha == HoverEnd)
                {
                    Parent.EndAnimationLoopRequest();
                    _animationRequested = false;
                }

                Opacity = originalOpacity;
            }

            base.BackgroundImage = BackgroundImage;

        }

        private ImageWithDeviceContext _backgroundImage;
        public new virtual ImageWithDeviceContext BackgroundImage
        {
            get { return _backgroundImage; }
            set
            {
                _backgroundImage = value;
                base.BackgroundImage = value;
            }
        }

        public virtual ImageWithDeviceContext HoverBackgroundImage { get; set; }
        public virtual ImageWithDeviceContext PressedBackgroundImage { get; set; }
        public virtual ImageWithDeviceContext DisabledBackgroundImage { get; set; }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;

            HoverAlpha = 0;
            HoverEnd = 255;
            StartHovering(HoverIn);
        }

        private void OnMouseLeave(object sender, EventArgs args)
        {
            if (!Enabled) return;

            HoverAlpha = 255;
            HoverEnd = 0;
            StartHovering(HoverOut);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;

            _hoverState = ButtonState.Pressed;
            StartHovering(HoverIn);
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (!Enabled) return;

            _hoverState = ButtonState.Hover;

            if (IsPointInside(e.Location))
            {
                StartHovering(HoverIn);
            }
        }

        public void StartHovering(int type)
        {
            if (!Enabled) return;

            HoverSpeed = DefaultHoverSpeed * type;

            if (!_animationRequested)
            {
                _animationRequested = true;
                Parent.RequestAnimationLoop();
            }
        }

        private ButtonState State
        {
            get
            {
                ButtonState functionReturnValue = default(ButtonState);
                if (_forceState != ButtonState.None)
                {
                    return _forceState;
                }

                functionReturnValue = ButtonState.Disabled;
                if (Enabled)
                {
                    functionReturnValue = ButtonState.Normal;
                    if (!_animationRequested)
                    {
                        if (Hovered & !Pressed)
                            functionReturnValue = ButtonState.Hover;
                        if (Pressed & Hovered)
                            functionReturnValue = ButtonState.Pressed;
                    }
                }
                return functionReturnValue;
            }
        }

        public ButtonState ForceState
        {
            get { return _forceState; }
            set { _forceState = value; }
        }

        public ButtonState HoverState
        {
            get { return _hoverState; }
            set { _hoverState = value; }
        }
    }
}
