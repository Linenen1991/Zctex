using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using WpfApp1.Utils;

namespace WpfApp1.Models;

public sealed class FilterRule : INotifyPropertyChanged
{
    private string _serviceNamePattern = "";
    private string _classNamePattern = "";
    private string _methodNamePattern = "";
    private string _messagePattern = "";
    private Color _matchColor = Colors.LightGoldenrodYellow;

    private SolidColorBrush _matchBrush = UiColorUtil.ToFrozenBrush(Colors.LightGoldenrodYellow);
    private SolidColorBrush _matchForeground = UiColorUtil.GetReadableTextBrush(Colors.LightGoldenrodYellow);
    private string _matchColorHex = UiColorUtil.ToRgbHex(Colors.LightGoldenrodYellow);

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ServiceNamePattern
    {
        get => _serviceNamePattern;
        set => SetField(ref _serviceNamePattern, value);
    }

    public string ClassNamePattern
    {
        get => _classNamePattern;
        set => SetField(ref _classNamePattern, value);
    }

    public string MethodNamePattern
    {
        get => _methodNamePattern;
        set => SetField(ref _methodNamePattern, value);
    }

    public string MessagePattern
    {
        get => _messagePattern;
        set => SetField(ref _messagePattern, value);
    }

    public Color MatchColor
    {
        get => _matchColor;
        set
        {
            if (!SetField(ref _matchColor, value))
            {
                return;
            }

            _matchColorHex = UiColorUtil.ToRgbHex(value);
            _matchBrush = UiColorUtil.ToFrozenBrush(value);
            _matchForeground = UiColorUtil.GetReadableTextBrush(value);

            OnPropertyChanged(nameof(MatchColorHex));
            OnPropertyChanged(nameof(MatchBrush));
            OnPropertyChanged(nameof(MatchForeground));
        }
    }

    public string MatchColorHex => _matchColorHex;

    public SolidColorBrush MatchBrush => _matchBrush;

    public SolidColorBrush MatchForeground => _matchForeground;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
