using System.Windows;

namespace Consulgo.QrCode4Cs.Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new QrViewModel();
        }
    }
}
