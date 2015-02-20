using System;
using System.Drawing;

namespace LayeredForms.Blur
{
    internal interface IBlurDrawer
    {
        void DrawBlurFromScreen(Action<IDeviceContext, Rectangle> sourcePainterFunc, IDeviceContext destImage, Rectangle screenBounds, Point destLocation, int blurStrength);
    }
}