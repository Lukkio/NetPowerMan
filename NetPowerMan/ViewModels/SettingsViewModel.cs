using NetPowerMan.Interfaces;
using NetPowerMan.Models;
using NetPowerMan.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetPowerMan.ViewModels
{
    internal class SettingsViewModel : IViewModel
    {
        #region Variables
        public bool _hideShutDown;
        public bool HideShutDown 
        {  get => _hideShutDown;
            set 
            {  
                _hideShutDown = value;
                NotifyPropertyChanged();
            }
        }
        public bool _hideReboot;
        public bool HideReboot
        {
            get => _hideReboot;
            set
            {
                _hideReboot = value;
                NotifyPropertyChanged();
            }
        }
        public bool _startup;
        public bool StartUp
        {
            get => _startup;
            set
            {
                _startup = value;
                NotifyPropertyChanged();
            }
        }
        public bool _notifyChecked;
        public bool NotifyChecked
        {
            get => _notifyChecked;
            set
            {
                _notifyChecked = value;
                NotifyPropertyChanged();
            }
        }
        public bool _sessionEndingDetect;
        public bool SessionEndingDetect
        {
            get => _sessionEndingDetect;
            set
            {
                _sessionEndingDetect = value;
                NotifyPropertyChanged();
            }
        }

        private Dictionary<string, string> _settingsInConfigFile;
        private List<DeviceModel> _devicesInConfigFile;
        public List<DeviceModel> DevicesInConfigFile
        {
            get => _devicesInConfigFile;
            set
            {
                _devicesInConfigFile = value;
                NotifyPropertyChanged();
            }
        }
        private ObservableCollection<DeviceViewModel> _deviceViewModels;
        public ObservableCollection<DeviceViewModel> DeviceViewModels
        {
            get => _deviceViewModels;
            set
            {
                _deviceViewModels = value;
                NotifyPropertyChanged();
            }
        }
        private ObservableCollection<DeviceViewModel> _newdeviceViewModels;
        public ObservableCollection<DeviceViewModel> NewDeviceViewModels
        {
            get => _newdeviceViewModels;
            set
            {
                _newdeviceViewModels = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<DeviceModel> _devicesInConfigFileOb;
        public ObservableCollection<DeviceModel> DevicesInConfigFileOb
        {
            get => _devicesInConfigFileOb;
            set
            {
                _devicesInConfigFileOb = value;
                NotifyPropertyChanged();
            }
        }
        public static ObservableCollection<ButtonBehaviour> queryModeList = new ObservableCollection<ButtonBehaviour>();

        private WindowsRegistry WindowsRegistry;
        public RelayCommand SaveDevices { get; private set; }
        public RelayCommand RemoveDevices { get; private set; }
        public RelayCommand AddDevices { get; private set; }
        public RelayCommand WriteRegistry { get; private set; }
        public RelayCommand SaveSettings { get; private set; }
        private readonly ILogger _logger;
        private readonly IShowMessage _showMessage;
        private SettingsService _settingsService;
        public GlobalSettingsModel GlobalSettings;
        #endregion
        public SettingsViewModel(ILogger logger, IShowMessage showMessage, Dictionary<string, string> SettingsInConfigFile, List<DeviceModel> DevicesInConfigFile, ObservableCollection<DeviceViewModel> deviceViewModels, GlobalSettingsModel globalSettingsModel) 
        {
            _showMessage = showMessage;
            _logger = logger;
            _settingsInConfigFile = SettingsInConfigFile;
            this.DevicesInConfigFile = DevicesInConfigFile;
            DeviceViewModels = deviceViewModels;
            _settingsService = new SettingsService(_logger, _showMessage);
            GlobalSettings = globalSettingsModel;
            WindowsRegistry = new WindowsRegistry(_logger, _showMessage);
            SessionEndingDetect = GlobalSettings.SessionEnding;
            NotifyChecked = GlobalSettings.NotifyStatus;

            //if (SettingsInConfigFile.TryGetValue("SessionEndingDetect", out string value))
            //{
            //    if(bool.TryParse(value, out bool result)) SessionEndingDetect = result;
            //}

            //if (SettingsInConfigFile.TryGetValue("NotifyChecked", out string value2))
            //{
            //    if (bool.TryParse(value2, out bool result2)) NotifyChecked = result2;
            //}

            HideShutDown = WindowsRegistry.ReadRegistryPowerButton(@"SOFTWARE\Microsoft\PolicyManager\default\Start\HideShutDown", "value");
            HideReboot = WindowsRegistry.ReadRegistryPowerButton(@"SOFTWARE\Microsoft\PolicyManager\default\Start\HideRestart", "value");
            StartUp = WindowsRegistry.ReadRegistryStartup(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "NetPowerMan");

            DevicesInConfigFileOb = new ObservableCollection<DeviceModel>();

            foreach (DeviceModel device in DevicesInConfigFile) 
            {
                DevicesInConfigFileOb.Add(device);
            }

            queryModeList.Add(new ButtonBehaviour { ID = 0, Name = "WMI" });
            queryModeList.Add(new ButtonBehaviour { ID = 1, Name = "PowerShell" });
            queryModeList.Add(new ButtonBehaviour { ID = 2, Name = "SystemShutdownEx" });
            queryModeList.Add(new ButtonBehaviour { ID = 3, Name = "WebOs" });
            queryModeList.Add(new ButtonBehaviour { ID = 4, Name = "SSH (Linux)" });

            SaveDevices = new RelayCommand(SaveDevicesCommand);
            RemoveDevices = new RelayCommand(RemoveDevicesCommand);
            AddDevices = new RelayCommand(AddDevicesCommand);
            WriteRegistry = new RelayCommand(WriteRegistryCommand);
            SaveSettings = new RelayCommand(SaveSettingsCommand);
        }
        private void SaveDevicesCommand(object param) 
        {        
            NewDeviceViewModels = new ObservableCollection<DeviceViewModel>(DevicesInConfigFileOb.Select(p => new DeviceViewModel(_logger, p)).ToList());
            
            foreach (DeviceViewModel device in DeviceViewModels) 
            {
                device.Dispose();
            }
            for(int i = DeviceViewModels.Count-1; i >= 0; i--) 
            {
                DeviceViewModels.RemoveAt(i);
            }
            foreach(DeviceViewModel device in NewDeviceViewModels) DeviceViewModels.Add(device);

            if (DeviceViewModels.Count > 0)
            {
                DeviceViewModel temp = DeviceViewModels.Last();
                if (temp != null) temp.LastItemCorner = new CornerRadius(0, 0, 10, 10);
            }

            NewDeviceViewModels.Clear();
            _settingsService.SettingsWrite(DevicesInConfigFileOb);
        }
        private void RemoveDevicesCommand(object param) 
        {
            if((int)param >=0)
                DevicesInConfigFileOb.RemoveAt((int)param);
        }
        private void AddDevicesCommand(object param)
        {
            DevicesInConfigFileOb.Add(new DeviceModel(new Dictionary<string, string>(),_logger,_showMessage));
        }
        private void WriteRegistryCommand(object param) 
        {
            string command = param as string;

            if(command.Contains("Shutdown")) 
            {
                HideShutDown = WindowsRegistry.WriteRegistryPowerBotton(@"SOFTWARE\Microsoft\PolicyManager\default\Start\HideShutDown", "value", HideShutDown);
            }
            if (command.Contains("Reboot"))
            {
                HideReboot = WindowsRegistry.WriteRegistryPowerBotton(@"SOFTWARE\Microsoft\PolicyManager\default\Start\HideRestart", "value", HideReboot);
            }

            if (command.Contains("StartUp"))
            {
                StartUp = WindowsRegistry.WriteRegistryStartUp(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "NetPowerMan", StartUp);
            }
        }
        private void SaveSettingsCommand(object param) 
        {
            _settingsService.SettingsWriteGlobalSettings("NotifyStatus", NotifyChecked.ToString());
            _settingsService.SettingsWriteGlobalSettings("SessionEndingDetect", SessionEndingDetect.ToString());
            GlobalSettings.SessionEnding = SessionEndingDetect;
            GlobalSettings.NotifyStatus = NotifyChecked; ;

            //if (_settingsInConfigFile.ContainsKey("NotifyStatus"))
            //{
            //    _settingsInConfigFile["NotifyStatus"] = NotifyChecked.ToString();
            //}
            //if (_settingsInConfigFile.ContainsKey("SessionEndingDetect"))
            //{
            //    _settingsInConfigFile["SessionEndingDetect"] = SessionEndingDetect.ToString();
            //}
        }
    }
}
