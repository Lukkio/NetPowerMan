using NetPowerMan.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using NetPowerMan.Models;
using System.Collections.ObjectModel;
using System.Windows;
using NetPowerMan.Services;
using System.Diagnostics.Eventing.Reader;
using Microsoft.Win32;
using System.Windows.Data;
using System.Threading;

namespace NetPowerMan.ViewModels
{
    internal class MainViewModel : IViewModel
    {
        #region Variables
        private readonly ILogger _logger;
        private readonly IShowMessage _showMessage;
        private readonly IShowSettings _showSettings;
        //Event to view
        public delegate void DeviceStatus(string DeviceName, string DeviceStatus);
        public event DeviceStatus DeviceStatusChanged;
        MainModel _mainmodel;
        private string _title;
        public string Title 
        {
            get { return _title; }
            set 
            { 
                _title = value;
                NotifyPropertyChanged();
            }
        }
        private string _pendingStatus;
        public string PendingStatus
        {
            get { return _pendingStatus; }
            set
            {
                _pendingStatus = value;
                NotifyPropertyChanged();
            }
        }
        //private Duration _speed;
        //public Duration Speed
        //{
        //    get { return _speed; }
        //    set
        //    {
        //        _speed = value;
        //        NotifyPropertyChanged();
        //    }
        //}
        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                NotifyPropertyChanged();
            }
        }
        private bool _isSimul = false;
        public bool IsSimul
        {
            get { return _isSimul; }
            set
            {
                _isSimul = value;
                NotifyPropertyChanged();
            }
        }
        private string _background;
        public string Background
        {
            get { return _background; }
            set
            {
                _background = value;
                NotifyPropertyChanged();
            }
        }
        private Thickness labelmargin = new Thickness(77, 20, 00, 55);
        public Thickness Labelmargin
        {
            get { return labelmargin; }
            set
            {
                labelmargin = value;
                NotifyPropertyChanged();
            }
        }
        public RelayCommand MainCommand { get; private set; }
        public RelayCommand SimulCommand { get; private set; }
        public RelayCommand OpenSettings { get; private set; }
        public ObservableCollection<DeviceViewModel> Devices { get; set; } = new ObservableCollection<DeviceViewModel>();
        public List<DeviceModel> DeviceModels { get; private set; }
        public CancellationTokenSource DisposeLabelDot { get; set; } = new CancellationTokenSource();
        #endregion
        public MainViewModel(ILogger logger, IShowMessage showMessage, IShowSettings showsettings, MainModel mainModel)
        {      
            _logger = logger;
            _mainmodel = mainModel;
            _showMessage = showMessage;
            _showSettings = showsettings;
            _mainmodel.PropertyChanged += MainModel_PropertyChanged;
            MainCommand = new RelayCommand(MainCommandRequest);
            SimulCommand = new RelayCommand(SimulCommandRequest);
            OpenSettings = new RelayCommand(OpenSettingsCommandRequest);    
            Title = Environment.MachineName + " @ " + Environment.UserName;

            if (_mainmodel.SettingsInConfigFile.TryGetValue("Background", out _background))
                Background = _background;
            else Background = "#FFFFFFFF";

            ObservableCollection<DeviceViewModel> DevicesTemp = new ObservableCollection<DeviceViewModel>(_mainmodel.DevicesInConfigFile.Select(p => new DeviceViewModel(_logger, p)).ToList());
            Devices.CollectionChanged += Devices_CollectionChanged;
            foreach (var device in DevicesTemp) Devices.Add(device);

            //Devices = new ObservableCollection<DeviceViewModel>(_mainmodel.DevicesInConfigFile.Select(p => new DeviceViewModel(_logger,p)).ToList());
            Devices.CollectionChanged += Devices_CollectionChanged;

            if(Devices.Count > 0) 
            { 
                DeviceViewModel temp = Devices.Last();
                if (temp != null) temp.LastItemCorner = new CornerRadius(0, 0, 10, 10);
            }
            
            if(_mainmodel.SubscribeSessionEnding) SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            logger.Info("Init MainViewModel Done");
            PendingStatus = string.Empty;
            //Speed = new Duration(TimeSpan.FromSeconds(10));
            StartPing("Init");
        }
        private void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {      
            if (e.OldItems != null)
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                    item.PropertyChanged -= DeviceViewModelItems_PropertyChanged;
            }
            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                    item.PropertyChanged += DeviceViewModelItems_PropertyChanged;
            }
        }
        private void DeviceViewModelItems_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains("NotifyDeviceStatusChanged")) 
            {
                DeviceViewModel deviceModel = sender as DeviceViewModel;
                if(_mainmodel.GlobalSettings.NotifyStatus)
                    //invoke event in the view
                    DeviceStatusChanged?.Invoke(deviceModel.Name, deviceModel.DeviceStatus);
            }
        }
        private void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Recupera notifica dal main model, aggiorna viewmodel di conseguenza notifica la view
            //model -> viewmodel -> view
            //if(e.PropertyName == nameof(Title))
            //Title = _mainmodel.Title;
            if (e.PropertyName == nameof(MainModel.SubscribeSessionEnding))
            {
                //bisogna aggiornare l'event SessionEnding
                if (_mainmodel.SubscribeSessionEnding)
                    SystemEvents.SessionEnding += SystemEvents_SessionEnding;
                else SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
            }
        }
        private void MainCommandRequest(object param) 
        {
            string command = param as string;

            //hide the button & show animation
            IsVisible = false;
            if (command == "MainShutDown") 
            {
                Labelmargin = new Thickness(65, 0, 00, 55);
                WaitingLabelTask("Shutdown"); 
            }

            if (command == "MainReboot")
            {
                Labelmargin = new Thickness(85, 0, 00, 55);
                WaitingLabelTask("Reboot");
            }

            foreach (DeviceViewModel deviceViewModel in Devices) 
            {
                deviceViewModel.ButtonOrCommandExecute(command);
            }

#pragma warning disable CS4014 // Non è possibile attendere la chiamata, pertanto l'esecuzione del metodo corrente continuerà prima del completamento della chiamata
            MonitorAllDevices(param);
#pragma warning restore CS4014 // Non è possibile attendere la chiamata, pertanto l'esecuzione del metodo corrente continuerà prima del completamento della chiamata

        }
        private void SimulCommandRequest(object param) 
        { 
        }
        private void OpenSettingsCommandRequest(object param) 
        {
            _showSettings.OpenWindow(new SettingsViewModel(_logger, _showMessage, _mainmodel.SettingsInConfigFile,_mainmodel.DevicesInConfigFile, Devices, _mainmodel.GlobalSettings));
        }
        public void StartPing(string command)
        {
            if(command.Contains("Init") && _mainmodel.Show) foreach(var device in Devices)  device.StartPing();  
            
            if(!_mainmodel.Show && command.Contains("Init") && _mainmodel.GlobalSettings.NotifyStatus)
                foreach (var device in Devices) device.StartPing();

            if (command.Contains("Resume")) foreach (var device in Devices) device.StartPing();
        }
        public void StopPing()
        {
            if(!_mainmodel.GlobalSettings.NotifyStatus)
                foreach (var device in Devices) { device.StopPing(); };
        }
        public void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            string createText = "SessionEnding(): e.Reason=" + e.Reason.ToString() + "\n\r";
            _logger.Info(createText);
            if (e.Reason == SessionEndReasons.SystemShutdown)
            {
                e.Cancel = true;
                //Resume ping if off da implementare
                foreach (var device in Devices)
                {
                        device.StartPing();
                }

                Task.Delay(TimeSpan.FromSeconds(2.5)).Wait();

                MainCommandRequest("ForceShutDown");
            }
        }
        private void WaitingLabelTask(string label) 
        {
            DisposeLabelDot = new CancellationTokenSource();
            Task.Run(() => 
            {
                int position = 0;
                string Dot = string.Empty;
                while (!DisposeLabelDot.IsCancellationRequested) 
                {
                    switch(position) 
                    {
                        case 0:
                            Dot = ".";
                            break;
                        case 1:
                            Dot = "..";
                            break;
                        case 2:
                            Dot = "...";
                            break;
                        case 3:
                            Dot = " ..";
                            break;
                        case 4:
                            Dot = ". .";
                            break;
                        case 5:
                            Dot = ".. ";
                            break;
                    }

                    position++;
                    if (position > 5) position = 0;
                    PendingStatus = label + Dot;
                    Task.Delay(1000).Wait();
                }
            });
        }
        private async Task MonitorAllDevices(object sender) 
        {
            string B = string.Empty;
            if (sender.GetType() == typeof(string)) B = sender as string;

            if (B.Contains("MainShutDown"))
            {
                var a2 = WaitAllDevices(false);
                await Task.WhenAll(a2);
                if (a2.Result == 3) 
                {
                    _logger.Error("Shutdown error, Timeout");
                    _showMessage.ShowMessageError("Shutdown error, Timeout", "Error"); 
                }
                if (a2.Result == 2) 
                {
                    
                    PendingStatus = " Goodbye...";
                    _logger.Info("MonitorAllDevices: Shutdown NOW!");
                    //_mainmodel.ShutDownThis();
                    await Task.Delay(1000);
                    DisposeLabelDot.Cancel();
                    //Application.Current.Shutdown();
                } //Shutdown
            }
        }
        public Task<int> WaitAllDevices(bool IsReboot) 
        {
            return Task.Run(
            () =>
            {
                int loop = 1;
                int Timeout = 0;
                bool IsDone = false;
                const int Success = 1;
                const int Failure = 3;
                const int NotPossible = 4;

                while (loop == 1)
                {
                    foreach (var dev in Devices) 
                    {
                        switch (dev._devicemodel.DeviceRemoteResult) 
                        {
                            case Success:
                                if ((!dev._devicemodel._deviceIsOnline) || (IsReboot)) 
                                {
                                    _logger.Info("Success: " + dev._devicemodel._deviceIsOnline.ToString() + dev._devicemodel.Name);
                                    IsDone = true;
                                }
                                else IsDone = false;
                                break; 
                            case Failure:
                                IsDone = false;
                                _logger.Error("MainViewModel:WaitAllDevices" + dev._devicemodel.Name + " Fail:" + IsReboot.ToString());
                                break;
                            case NotPossible:
                                IsDone = true;
                                _logger.Info("NotPossible: " + dev._devicemodel._deviceIsOnline.ToString() + dev._devicemodel.Name);
                                break;
                            default:
                                IsDone = false;
                                break;
                        }
                        if(!IsDone) break;
                    }
                    if(IsDone) loop = 2;
                    Timeout++;
                    if (Timeout > 30) loop = 3;
                    Task.Delay(1000).Wait();
                }
                return loop;
            });
        }
    }
}
