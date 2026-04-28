using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace WPF_GUI
{
    public partial class MainWindow : Window
    {
        private readonly List<LogsData> _allData;
        private readonly ObservableCollection<EventData> _events;

        public MainWindow()
        {
            InitializeComponent();

            _allData = new List<LogsData>();

            _events = new ObservableCollection<EventData>
            {
                new EventData { eventdata = "EVT-001", detailmessage = "First detail message." },
                new EventData { eventdata = "EVT-002", detailmessage = "Longer detail message to test horizontal scrolling. 0123456789 0123456789 0123456789 0123456789" },
                new EventData { eventdata = "EVT-003", detailmessage = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." },
            };

            EventsPair.ItemsSource = _events;
        }
    }
}
