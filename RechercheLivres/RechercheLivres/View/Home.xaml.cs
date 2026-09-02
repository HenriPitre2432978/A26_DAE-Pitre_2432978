using RechercheLivres.View;
using RechercheLivres.ViewModels;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RechercheLivres.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Home : Window
    {
        public Home()
        {
            InitializeComponent();
            DataContext = new MainVM();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is MainVM vm)
            {
                bool canClose = vm.Close();
                if (!canClose)
                    e.Cancel = true;
            }
        }
    }
}