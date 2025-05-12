using DJ_WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DJ_WPF.View
{
    public partial class ConfigurationView : System.Windows.Controls.UserControl
    {
        public ConfigurationView()
        {
            InitializeComponent();
            DataContext = new ConfigurationViewModel();
        }
    }
}
