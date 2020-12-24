using DeveloperTest.ViewModel;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
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

namespace DeveloperTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IMainWindowViewModel ViewModel { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainWindowViewModel();  // Normally I would use Dependency Injection / Ioc, but it's not compatible with .Net 4.5.2

            DataContext = ViewModel;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            MailGrid.Columns[0].Visibility = Visibility.Hidden;
            MailGrid.Columns[1].Visibility = Visibility.Hidden;
        }
    }
}
