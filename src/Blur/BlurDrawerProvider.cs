namespace LayeredForms.Blur
{
    internal class BlurDrawerProvider
    {
        private static IBlurDrawer _drawer;

        public static IBlurDrawer GetBlurDrawer()
        {
            return _drawer ?? new FastBlurDrawer();
        }
    }
}
