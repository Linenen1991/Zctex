using System.Windows.Media;
using DrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace WpfApp1.Utils;

internal static class UiColorUtil
{
    public static string ToRgbHex(MediaColor color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    public static SolidColorBrush ToFrozenBrush(MediaColor color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    public static SolidColorBrush GetReadableTextBrush(MediaColor background)
    {
        var luminance = (0.2126 * background.R) + (0.7152 * background.G) + (0.0722 * background.B);
        var text = luminance < 140 ? Colors.White : Colors.Black;
        return ToFrozenBrush(text);
    }

    public static MediaColor FromDrawingColor(DrawingColor color) =>
        MediaColor.FromArgb(color.A, color.R, color.G, color.B);

    public static DrawingColor ToDrawingColor(MediaColor color) =>
        DrawingColor.FromArgb(color.A, color.R, color.G, color.B);
}
