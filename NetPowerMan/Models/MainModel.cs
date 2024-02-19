using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using NetPowerMan.Interfaces;
using NetPowerMan.Services;
using NLog;

namespace NetPowerMan.Models
{
    internal class MainModel : IModel
    {
        #region Variables
        private readonly ILogger _logger;
        private readonly IShowMessage _showMessage;
        private SettingsService _settingsService;
        public Dictionary<string,string> SettingsInConfigFile { get; set; } = new Dictionary<string,string>();
        public List<DeviceModel> DevicesInConfigFile {  get; set; } = new List<DeviceModel>();
        private bool _subscribeSessionEnding;
        public bool SubscribeSessionEnding
        {
            get { return _subscribeSessionEnding; }
            set
            {
                _subscribeSessionEnding = value;
                NotifyPropertyChanged();
            }
        }
        public GlobalSettingsModel GlobalSettings;
        public bool Show {  get; set; }
        #endregion
        public MainModel(ILogger logger, IShowMessage showMessage, bool show) 
        {
            _logger = logger;
            _showMessage = showMessage;
            Show = show;
            _settingsService = new SettingsService(_logger, _showMessage);
            GlobalSettings = new GlobalSettingsModel();
            GlobalSettings.PropertyChanged += GlobalSettings_PropertyChanged;
            LoadSettings();    
            _logger.Info("Init MainModel Done.");
        }
        private void GlobalSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GlobalSettingsModel.SessionEnding)) 
            {
                //bisogna aggiornare l'event SessionEnding
                //if(GlobalSettings.SessionEnding) 
                //    SystemEvents.SessionEnding += SystemEvents_SessionEnding;
                //else SystemEvents.SessionEnding -= SystemEvents_SessionEnding;
                if (GlobalSettings.SessionEnding) SubscribeSessionEnding = true;
                else SubscribeSessionEnding = false;
            }
            if (e.PropertyName == nameof(GlobalSettingsModel.NotifyStatus))
            {
                //bisogna aggiornare evento NotifyStatus
            }
        }
        public int LoadSettings() 
        {
            int _result = _settingsService.ReadSettingsAttributes("Setting", out Dictionary<string, string> _readedsettings);

            if (_result == 0) 
            {
                SettingsInConfigFile = _readedsettings;
                if (SettingsInConfigFile.ContainsKey("NotifyStatus"))
                {
                    if(bool.TryParse(SettingsInConfigFile["NotifyStatus"], out bool result)) GlobalSettings.NotifyStatus = result;
                }
                if (SettingsInConfigFile.ContainsKey("SessionEndingDetect"))
                {
                    if (bool.TryParse(SettingsInConfigFile["SessionEndingDetect"], out bool result)) GlobalSettings.SessionEnding = result;
                }

                _logger.Info("LoadSettings Done");
            }
            else 
            {
                _logger.Error("LoadSettings failed: " + _result.ToString());
            }

            _result = _settingsService.ReadAllDevicesAttributes("Device", out List<DeviceModel> _readeddevices);

            if (_result == 0)
            {
                DevicesInConfigFile = _readeddevices;
                _logger.Info("LoadDevices Done");
            }
            else 
            {
                _logger.Error("LoadDevices failed: " + _result.ToString());
            }
            return _result;
        }
        public void StartCommand(string command) 
        {
            _logger.Info($"StartCommand={command}");
            //Esegue la logica di spegnimento o riavvio
        }
        public void ShutDownThis() 
        {
            //WindowsUnmanaged.InitiateSystemShutdownEx("", "", 0, true, false,0);
            WindowsUnmanaged.ExecuteCLICommand("shutdown","/p /f",false,true,false);
        }
    }
}
