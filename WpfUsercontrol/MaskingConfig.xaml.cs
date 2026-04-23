using System.Windows;
using WpfUsercontrol.ViewModels;

namespace WpfUsercontrol
{
    public partial class MaskingConfig : Window
    {
        public MaskingConfig()
        {
            InitializeComponent();
            DataContext = new MaskingConfigViewModel();
        }
    }
}
