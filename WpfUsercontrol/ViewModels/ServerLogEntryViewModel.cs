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
        private string _logInformation;
        private string _originalLogInformation;
        private ObservableCollection<TextSegment> _messageSegments;
        private ObservableCollection<TextSegment> _logInformationSegments;

        public ServerLogEntryViewModel()
        {
            _messageSegments = new ObservableCollection<TextSegment>
            {
                new TextSegment(string.Empty, Brushes.Black)
            };
            _logInformationSegments = new ObservableCollection<TextSegment>
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
            set
            {
                if (SetProperty(ref _message, value))
                {
                    MessageSegments = new ObservableCollection<TextSegment>
                    {
                        new TextSegment(_message ?? string.Empty, Brushes.Black)
                    };
                }
            }
        }

        public string LogInformation
        {
            get => _logInformation;
            set => SetProperty(ref _logInformation, value);
        }

        public string OriginalLogInformation
        {
            get => _originalLogInformation;
            set => SetProperty(ref _originalLogInformation, value);
        }

        public ObservableCollection<TextSegment> MessageSegments
        {
            get => _messageSegments;
            set => SetProperty(ref _messageSegments, value);
        }

        public ObservableCollection<TextSegment> LogInformationSegments
        {
            get => _logInformationSegments;
            set => SetProperty(ref _logInformationSegments, value);
        }
    }
}
