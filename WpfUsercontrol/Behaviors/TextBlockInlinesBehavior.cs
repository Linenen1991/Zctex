using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WpfUsercontrol.ViewModels;

namespace WpfUsercontrol.Behaviors
{
    public static class TextBlockInlinesBehavior
    {
        public static readonly DependencyProperty SegmentsProperty =
            DependencyProperty.RegisterAttached(
                "Segments",
                typeof(IEnumerable<TextSegment>),
                typeof(TextBlockInlinesBehavior),
                new PropertyMetadata(null, OnSegmentsChanged));

        public static IEnumerable<TextSegment> GetSegments(DependencyObject obj)
        {
            return (IEnumerable<TextSegment>)obj.GetValue(SegmentsProperty);
        }

        public static void SetSegments(DependencyObject obj, IEnumerable<TextSegment> value)
        {
            obj.SetValue(SegmentsProperty, value);
        }

        private static void OnSegmentsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = d as TextBlock;
            if (textBlock == null)
            {
                return;
            }

            textBlock.Inlines.Clear();

            var segments = e.NewValue as IEnumerable<TextSegment>;
            if (segments == null)
            {
                return;
            }

            foreach (var segment in segments)
            {
                var run = new Run(segment.Text ?? string.Empty)
                {
                    Foreground = segment.Foreground
                };
                textBlock.Inlines.Add(run);
            }
        }
    }
}
