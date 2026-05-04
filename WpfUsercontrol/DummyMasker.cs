using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfUsercontrol
{
    internal static class DummyMasker
    {
        const int MaxCount = 5000;
        private static Dictionary<string, Dictionary<string, string>> maskCollection { get; } = 
            new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, int> maskCount = new Dictionary<string, int>();

        private static string GetNext(string fixedCan)
        {
            if(!maskCount.ContainsKey(fixedCan))
            {
                maskCount.Add(fixedCan, 0);
            }
            maskCount[fixedCan]++;
            if (maskCount[fixedCan] > MaxCount)
            {
                maskCount[fixedCan] = 1;
            }
            return fixedCan + "_" + maskCount[fixedCan];
        }

        public static string  GetMasking(string fixedCan,string name)
        {
            if (!maskCollection.ContainsKey(fixedCan))
                maskCollection.Add(fixedCan, new Dictionary<string, string>());
            if (!maskCollection[fixedCan].ContainsKey(name))
                maskCollection[fixedCan].Add(name, GetNext(fixedCan));
            return maskCollection[fixedCan][name];
        }

        internal static void Reset()
        {
            maskCollection.Clear();
            maskCount.Clear();
        }
    }
}
