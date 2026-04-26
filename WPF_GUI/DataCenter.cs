using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_GUI
{
    public class LogsData
    {
        private string _serviceName;
        private string _className;
        private string _methodName;
        private string _blockBelong;
        private DateTime _timestamp;

        public string ServiceName { get => _serviceName; set => _serviceName = value; }
        public string ClassName { get => _className; set => _className = value; }
        public string MethodName { get => _methodName; set => _methodName = value; }
        public string BlockBelong { get => _blockBelong; set => _blockBelong = value; }
        public DateTime Timestamp { get => _timestamp; set => _timestamp = value; }
    }
    internal class DataCenter
    {
        public List<LogsData> GetPage(int pageIndex)
        {
            return new List<LogsData>();
        }

        public List<LogsData> GetFileted(int pageIndex)
        {
            return new List<LogsData>();
        }

        public List<LogsData> GetEventCodeOnly()
        {
            return new List<LogsData>();
        }
    }
}
