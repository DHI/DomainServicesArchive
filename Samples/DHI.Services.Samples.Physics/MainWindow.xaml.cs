namespace DHI.Services.Samples.Physics
{
    using DHI.Services.Samples.Physics.ViewModels;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new PhysicsViewModel();
        }
    }
}