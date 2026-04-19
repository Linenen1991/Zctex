using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using WpfApp1.Models;
using WpfApp1.Utils;
using WinForms = System.Windows.Forms;

namespace WpfApp1.Controls;

public partial class AsyncElasticPanel : UserControl
{
    public ElasticQueryViewModel Query { get; } = new();

    public static readonly DependencyProperty FilterRulesProperty = DependencyProperty.Register(
        nameof(FilterRules),
        typeof(IList<FilterRule>),
        typeof(AsyncElasticPanel),
        new PropertyMetadata(null));

    public static readonly DependencyProperty LogEntriesProperty = DependencyProperty.Register(
        nameof(LogEntries),
        typeof(IList<LogEntry>),
        typeof(AsyncElasticPanel),
        new PropertyMetadata(null));

    public static readonly DependencyProperty EventEntriesProperty = DependencyProperty.Register(
        nameof(EventEntries),
        typeof(IList<LogEntry>),
        typeof(AsyncElasticPanel),
        new PropertyMetadata(null));

    public IList<FilterRule>? FilterRules
    {
        get => (IList<FilterRule>?)GetValue(FilterRulesProperty);
        set => SetValue(FilterRulesProperty, value);
    }

    public IList<LogEntry>? LogEntries
    {
        get => (IList<LogEntry>?)GetValue(LogEntriesProperty);
        set => SetValue(LogEntriesProperty, value);
    }

    public IList<LogEntry>? EventEntries
    {
        get => (IList<LogEntry>?)GetValue(EventEntriesProperty);
        set => SetValue(EventEntriesProperty, value);
    }

    public AsyncElasticPanel()
    {
        InitializeComponent();

        FilterRules ??= new ObservableCollection<FilterRule>();
        LogEntries ??= new ObservableRangeCollection<LogEntry>();
        EventEntries ??= new ObservableRangeCollection<LogEntry>();
    }

    private void MainTabs_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (sender is not System.Windows.Controls.TabControl tabControl)
        {
            return;
        }

        if (!ReferenceEquals(e.Source, tabControl))
        {
            return;
        }

        if (tabControl.SelectedItem is not System.Windows.Controls.TabItem tabItem)
        {
            return;
        }

        var header = tabItem.Header?.ToString();
        if (string.Equals(header, "LogList", StringComparison.Ordinal) ||
            string.Equals(header, "EventList", StringComparison.Ordinal))
        {
            ApplyColors();
        }
    }

    private void RemoveSelected_Click(object sender, RoutedEventArgs e) => RemoveSelectedRules();

    private async void QueryButton_Click(object sender, RoutedEventArgs e)
    {
        var target = LogEntries;
        if (target is null)
        {
            return;
        }

        Query.BindTargets(
            targetEntries: target,
            getFilterRules: () => (FilterRules ?? Array.Empty<FilterRule>()).ToArray());

        await Query.StartOrCancelAsync();
    }

    private void PrevPage_Click(object sender, RoutedEventArgs e) => Query.PrevPage();

    private void NextPage_Click(object sender, RoutedEventArgs e) => Query.NextPage();

    private void LogList_AddRule_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.MenuItem menuItem)
        {
            return;
        }

        if (menuItem.CommandParameter is not LogEntry entry)
        {
            return;
        }

        var filterRules = FilterRules;
        if (filterRules is null)
        {
            return;
        }

        var rule = new FilterRule();
        var mode = menuItem.Tag?.ToString();
        switch (mode)
        {
            case "All":
                rule.ServiceNamePattern = entry.ServiceName ?? "";
                rule.ClassNamePattern = entry.ClassName ?? "";
                rule.MethodNamePattern = entry.MethodName ?? "";
                rule.MessagePattern = entry.Message ?? "";
                break;
            case "ServiceName":
                rule.ServiceNamePattern = entry.ServiceName ?? "";
                break;
            case "ClassName":
                rule.ClassNamePattern = entry.ClassName ?? "";
                break;
            case "MethodName":
                rule.MethodNamePattern = entry.MethodName ?? "";
                break;
            case "Message":
                rule.MessagePattern = entry.Message ?? "";
                break;
            default:
                rule.ClassNamePattern = entry.ClassName ?? "";
                break;
        }

        filterRules.Add(rule);
        ApplyColors();
    }

    private void LogListItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListViewItem item)
        {
            item.IsSelected = true;
            item.Focus();
        }
    }

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

        FilterRules?.Remove(rule);
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

        var hostWindow = Window.GetWindow(this);
        var ownerHandle = hostWindow is null ? IntPtr.Zero : new WindowInteropHelper(hostWindow).Handle;
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
        var filterRules = FilterRules;
        if (filterRules is null)
        {
            return 0;
        }

        var items = FilterGrid.SelectedItems
            .OfType<FilterRule>()
            .Distinct()
            .ToList();

        foreach (var item in items)
        {
            filterRules.Remove(item);
        }

        return items.Count;
    }

    private void CommitGridEdits()
    {
        FilterGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        FilterGrid.CommitEdit(DataGridEditingUnit.Row, true);
    }

    private void ApplyColors()
    {
        CommitGridEdits();

        var filterRules = FilterRules;
        if (filterRules is null)
        {
            return;
        }

        var logEntries = LogEntries;
        if (logEntries is not null)
        {
            ApplyColorsFor(logEntries, filterRules);
        }
    }

    private static void ApplyColorsFor(IList<LogEntry> entries, IList<FilterRule> rules)
    {
        foreach (var entry in entries)
        {
            entry.ClearHighlight();

            foreach (var rule in rules)
            {
                if (string.Equals(rule.ClassNamePattern, entry.ClassName, StringComparison.Ordinal))
                {
                    entry.ApplyHighlight(rule.MatchColor);
                }
            }
        }
    }

    private sealed class Win32Window : WinForms.IWin32Window
    {
        public Win32Window(IntPtr handle) => Handle = handle;

        public IntPtr Handle { get; }
    }
}
