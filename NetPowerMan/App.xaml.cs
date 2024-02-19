using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NetPowerMan.Views;
using NetPowerMan.ViewModels;
using NetPowerMan.Models;
using NetPowerMan.Interfaces;
using NetPowerMan.Services;
using System.IO;
using System.Threading;

namespace NetPowerMan
{
    /// <summary>
    /// Logica di interazione per App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ShowMessage  _showMessage = new ShowMessage();
        private readonly ShowSettingsService _showSettingsService = new ShowSettingsService();
        private static System.Threading.Mutex _mutex = null;
        protected override void OnStartup(StartupEventArgs e)
        {            
            bool ShowMainWindow = true;
            bool ShowSettings = false;

            //Prevent multiple instance: guid must be defined in attributes
            //get the guid string of powermanager from attributes
            string mutexId = ((System.Runtime.InteropServices.GuidAttribute)System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false).GetValue(0)).Value.ToString();

            //try to initialize an mutex (mutex is a "locked thread") with the guid passed, if out bool == false, means there is already another
            //mutex initialized with same guid, so cannot initialize it. (in this case means another instance of powermanager is already running)
            _mutex = new System.Threading.Mutex(true, mutexId, out bool MutexIsCreated);

            //Arguments handler
            if (e.Args.Length > 0)
            {
                if (e.Args[0].Contains("-background")) ShowMainWindow = false;
                if (e.Args[0].Contains("SettingsWindow"))
                {
                    ShowMainWindow = true;
                    ShowSettings = true;                    
                }
            }
            else ShowMainWindow = true;

            //Retry again, add delay (waiting instance closed in case of restart app in admin mode)
            if (!MutexIsCreated && ShowSettings)
            {
                Task.Delay(2000).Wait();
                _mutex = new System.Threading.Mutex(true, mutexId, out MutexIsCreated);
            }

            if (!MutexIsCreated)
            {
                //Send custom message to all windows, only my app can interpreter this custom message.
                // use user32.dll
                SystemMessage.SendMessage((IntPtr)SystemMessage.HWND_BROADCAST, SystemMessage.WM_SHOWME, IntPtr.Zero, null);

                //MessageBox.Show("Already running, check tray icon.","Error!",MessageBoxButton.OK,MessageBoxImage.Information);

                //close this instance.
                Current.Shutdown();
            }
            else Exit += CloseMutexHandler;

            base.OnStartup(e);

            if (MutexIsCreated) 
            { 
                this.MainWindow = new MainWindow(Logger)
                {
                    Topmost = true,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                };

                Directory.SetCurrentDirectory(AppContext.BaseDirectory);

                this.MainWindow.DataContext = new MainViewModel(Logger, _showMessage, _showSettingsService, new MainModel(Logger, _showMessage, ShowMainWindow));

                if (ShowMainWindow) this.MainWindow.Show();
                else
                {
                    this.MainWindow.Show();
                    this.MainWindow.Hide();
                }
            }
        }
        protected virtual void CloseMutexHandler(object sender, EventArgs e)
        {
            _mutex?.Close();
        }
    }
}
