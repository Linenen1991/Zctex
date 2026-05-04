using System.Collections.ObjectModel;
using System.Windows.Media;

namespace WpfUsercontrol.ViewModels
{
    public sealed class ServerLogEntryViewModel : ViewModelBase
    {
        private string _message;
        private string _serviceName;
        private string _className;
        private string _methodName;
        private ObservableCollection<TextSegment> _messageSegments;
        private string _originalMessage;
        private string _maskedMessage;

        public ServerLogEntryViewModel()
        {
            _messageSegments = new ObservableCollection<TextSegment>
            {
                new TextSegment(string.Empty, Brushes.Black)
            };
        }

        public string ServiceName
        {
            get => _serviceName;
            set => SetProperty(ref _serviceName, value);
        }

        public string ClassName
        {
            get => _className;
            set => SetProperty(ref _className, value);
        }

        public string MethodName
        {
            get => _methodName;
            set => SetProperty(ref _methodName, value);
        }


        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public string OriginalMessage
        {
            get => _originalMessage;
            set => SetProperty(ref _originalMessage, value);
        }

        public string MaskedMessage
        {
            get => _maskedMessage;
            set => SetProperty(ref _maskedMessage, value);
        }

        public ObservableCollection<TextSegment> MessageSegments
        {
            get => _messageSegments;
            set => SetProperty(ref _messageSegments, value);
        }
    }
}
