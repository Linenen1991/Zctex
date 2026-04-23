using System.Collections.ObjectModel;
using System.Windows.Media;

namespace WpfUsercontrol.ViewModels
{
    public sealed class ServerLogEntryViewModel : ViewModelBase
    {
        private string _userComment;
        private string _message;
        private ObservableCollection<TextSegment> _messageSegments;

        public ServerLogEntryViewModel()
        {
            _messageSegments = new ObservableCollection<TextSegment>
            {
                new TextSegment(string.Empty, Brushes.Black)
            };
        }

        public string UserComment
        {
            get => _userComment;
            set => SetProperty(ref _userComment, value);
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

        public ObservableCollection<TextSegment> MessageSegments
        {
            get => _messageSegments;
            set => SetProperty(ref _messageSegments, value);
        }
    }
}
