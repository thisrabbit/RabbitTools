using System.Drawing;

namespace RabbitTools
{
    public static class ColorTable
    {
        public static Color BG = Color.FromArgb(230, 230, 230);

        public static Color Axis = Color.LightGray;
        public static SolidBrush AxisPointBrush = new SolidBrush(Color.DarkGray);
        
        public static SolidBrush WidthTintBrush = 
            new SolidBrush(Color.FromArgb(50, 237, 125, 49));
        public static SolidBrush WidthBrush =
            new SolidBrush(Color.FromArgb(200, 237, 125, 49));
        public static Color WidthLine = Color.FromArgb(150, 237, 125, 49);

        public static SolidBrush HeightTintBrush =
            new SolidBrush(Color.FromArgb(50, 68, 114, 196));
        public static SolidBrush HeightBrush =
            new SolidBrush(Color.FromArgb(200, 68, 114, 196));
        public static Color HeightLine = Color.FromArgb(150, 68, 114, 196);

        public static SolidBrush UniBrush =
            new SolidBrush(Color.FromArgb(200, 206, 49, 237));
        public static Color UniLine = Color.FromArgb(150, 206, 49, 237);
    }
}
