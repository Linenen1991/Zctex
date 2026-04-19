using System.Collections.ObjectModel;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Utils;

namespace WpfApp1;

public partial class MainWindow : Window
{
    public ObservableCollection<FilterRule> FilterRules { get; } = new();

    public ObservableRangeCollection<LogEntry> LogEntries { get; } = new();

    public ObservableRangeCollection<LogEntry> EventEntries { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }
}
