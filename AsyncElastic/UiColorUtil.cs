using System.Drawing;

namespace AsyncElastic;

internal static class UiColorUtil
{
    public static string ToRgbHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    public static Color GetReadableTextColor(Color background)
    {
        // Relative luminance heuristic; good enough for UI.
        var luminance = (0.2126 * background.R) + (0.7152 * background.G) + (0.0722 * background.B);
        return luminance < 140 ? Color.White : Color.Black;
    }
}

