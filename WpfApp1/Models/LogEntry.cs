using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using WpfApp1.Utils;

namespace WpfApp1.Models;

public sealed class LogEntry : INotifyPropertyChanged
{
    private string _serviceName = "";
    private string _className = "";
    private string _methodName = "";
    private string _message = "";

    private Brush _background = Brushes.Transparent;
    private Brush _foreground = Brushes.Black;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ServiceName
    {
        get => _serviceName;
        set => SetField(ref _serviceName, value);
    }

    public string ClassName
    {
        get => _className;
        set => SetField(ref _className, value);
    }

    public string MethodName
    {
        get => _methodName;
        set => SetField(ref _methodName, value);
    }

    public string Message
    {
        get => _message;
        set => SetField(ref _message, value);
    }

    public Brush Background
    {
        get => _background;
        private set => SetField(ref _background, value);
    }

    public Brush Foreground
    {
        get => _foreground;
        private set => SetField(ref _foreground, value);
    }

    public void ClearHighlight()
    {
        Background = Brushes.Transparent;
        Foreground = Brushes.Black;
    }

    public void ApplyHighlight(Color background)
    {
        Background = UiColorUtil.ToFrozenBrush(background);
        Foreground = UiColorUtil.GetReadableTextBrush(background);
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
