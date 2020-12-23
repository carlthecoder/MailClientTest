using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace DeveloperTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += OnUnhandledExceptionOccurred;
        }

        private void OnUnhandledExceptionOccurred(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"\n{e.Exception}" +
                $"\n{e.Exception.Message}" +
                $"\n{e.Exception.StackTrace}");

            e.Handled = true;
        }
    }
}
