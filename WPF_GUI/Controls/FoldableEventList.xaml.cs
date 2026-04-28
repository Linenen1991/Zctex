using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace WPF_GUI.Controls
{
    public partial class FoldableEventList : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(FoldableEventList),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register(
                nameof(IsExpanded),
                typeof(bool),
                typeof(FoldableEventList),
                new PropertyMetadata(false, OnLayoutPropertyChanged));

        public static readonly DependencyProperty CollapsedListWidthProperty =
            DependencyProperty.Register(
                nameof(CollapsedListWidth),
                typeof(double),
                typeof(FoldableEventList),
                new PropertyMetadata(200d, OnLayoutPropertyChanged));

        public static readonly DependencyProperty ExpandedListWidthProperty =
            DependencyProperty.Register(
                nameof(ExpandedListWidth),
                typeof(double),
                typeof(FoldableEventList),
                new PropertyMetadata(500d, OnLayoutPropertyChanged));

        public FoldableEventList()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public double CollapsedListWidth
        {
            get => (double)GetValue(CollapsedListWidthProperty);
            set => SetValue(CollapsedListWidthProperty, value);
        }

        public double ExpandedListWidth
        {
            get => (double)GetValue(ExpandedListWidthProperty);
            set => SetValue(ExpandedListWidthProperty, value);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyExpandedState();
        }

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FoldableEventList control)
            {
                control.ApplyExpandedState();
            }
        }

        private void ApplyExpandedState()
        {
            if (ListAreaColumn == null || DetailColumn == null)
            {
                return;
            }

            ListAreaColumn.Width = new GridLength(IsExpanded ? ExpandedListWidth : CollapsedListWidth);
            DetailColumn.Width = IsExpanded ? double.NaN : 0d;
        }
    }
}
