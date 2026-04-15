using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using WpfApp1.Models;
using WpfApp1.Utils;
using WinForms = System.Windows.Forms;

namespace WpfApp1;

public partial class MainWindow : Window
{
    public ObservableCollection<FilterRule> FilterRules { get; } = new();

    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        CommitGridEdits();

        foreach (var entry in LogEntries)
        {
            entry.ClearHighlight();

            foreach (var rule in FilterRules)
            {
                if (string.Equals(rule.ClassNamePattern, entry.ClassName, StringComparison.Ordinal))
                {
                    entry.ApplyHighlight(rule.MatchColor);
                }
            }
        }
    }

    private void RemoveSelected_Click(object sender, RoutedEventArgs e) => RemoveSelectedRules();

    private void FilterGrid_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete)
        {
            return;
        }

        if (RemoveSelectedRules() > 0)
        {
            e.Handled = true;
        }
    }

    private void RemoveRule_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FilterRule rule })
        {
            return;
        }

        FilterRules.Remove(rule);
    }

    private void PickColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FilterRule rule })
        {
            return;
        }

        using var dialog = new WinForms.ColorDialog
        {
            FullOpen = true,
            Color = UiColorUtil.ToDrawingColor(rule.MatchColor)
        };

        var ownerHandle = new WindowInteropHelper(this).Handle;
        var result = ownerHandle == IntPtr.Zero
            ? dialog.ShowDialog()
            : dialog.ShowDialog(new Win32Window(ownerHandle));

        if (result == WinForms.DialogResult.OK)
        {
            rule.MatchColor = UiColorUtil.FromDrawingColor(dialog.Color);
        }
    }

    private int RemoveSelectedRules()
    {
        var items = FilterGrid.SelectedItems
            .OfType<FilterRule>()
            .Distinct()
            .ToList();

        foreach (var item in items)
        {
            FilterRules.Remove(item);
        }

        return items.Count;
    }

    private void CommitGridEdits()
    {
        FilterGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        FilterGrid.CommitEdit(DataGridEditingUnit.Row, true);
    }

    private sealed class Win32Window : WinForms.IWin32Window
    {
        public Win32Window(IntPtr handle) => Handle = handle;

        public IntPtr Handle { get; }
    }
}
