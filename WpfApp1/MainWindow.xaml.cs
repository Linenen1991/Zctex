using System.Collections.ObjectModel;
using System.Windows;
using WpfApp1.Models;

namespace WpfApp1;

public partial class MainWindow : Window
{
    public ObservableCollection<FilterRule> FilterRules { get; } = new();

    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    public ObservableCollection<LogEntry> EventEntries { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }
}
