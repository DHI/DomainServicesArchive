namespace DHI.Services.Samples.Map
{
    using DHI.Services.Samples.Map.ViewModels;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public MainViewModel Dfs2 { get; } = new MainViewModel();

        public DfsuViewModel Dfsu { get; } = new DfsuViewModel();
    }
}