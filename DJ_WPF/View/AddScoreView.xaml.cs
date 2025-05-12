using DJ_WPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;



namespace DJ_WPF.View
{
    
    public partial class AddScoreView : System.Windows.Controls.UserControl
    {
        public AddScoreView()
        {
            InitializeComponent();
            DataContext = new AddScoreViewModel();
        }

        
    }
}
