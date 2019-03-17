using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HDR2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException1;
        }

        private void App_DispatcherUnhandledException1(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            LogPanel.Log($"App: [Unhandled Exception]\n{e.Exception}");
            // Prevent default unhandled exception processing
            e.Handled = true;
        }
    }
}
