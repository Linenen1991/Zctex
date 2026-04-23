using System.Windows.Media;

namespace WpfUsercontrol.ViewModels
{
    public sealed class TextSegment
    {
        public TextSegment(string text, Brush foreground, Brush background = null)
        {
            Text = text;
            Foreground = foreground;
            Background = background;
        }

        public string Text { get; }
        public Brush Foreground { get; }
        public Brush Background { get; }
    }
}
