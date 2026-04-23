using System.Windows.Media;

namespace WpfUsercontrol.ViewModels
{
    public sealed class TextSegment
    {
        public TextSegment(string text, Brush foreground)
        {
            Text = text;
            Foreground = foreground;
        }

        public string Text { get; }
        public Brush Foreground { get; }
    }
}
