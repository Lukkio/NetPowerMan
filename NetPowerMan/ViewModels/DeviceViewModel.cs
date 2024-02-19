using NetPowerMan.Interfaces;
using NetPowerMan.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NetPowerMan.ViewModels
{
    internal class DeviceViewModel : IViewModel, IDisposable
    {
        #region Variables
        private PropertyInfo[] _modelproperties;
        private PropertyInfo[] _viewModelproperties;
        public DeviceModel _devicemodel;
        private string _name;
        public string Name 
        {
            get => _name;  
            set 
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }
        private string _iP;
        public string IP
        {
            get => _iP;
            set
            {
                _iP = value;
                NotifyPropertyChanged();
            }
        }
        private string _mAC;
        public string MAC
        {
            get => _mAC;
            set
            {
                _mAC = value;
                NotifyPropertyChanged();
            }
        }
        private bool _enabled;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                NotifyPropertyChanged();
            }
        }
        private bool _defaultShutDown;
        public bool DefaultShutDown
        {
            get => _defaultShutDown;
            set
            {
                _defaultShutDown = value;
                NotifyPropertyChanged();
                SyncModel(value);
                //_devicemodel.DefaultShutDown = value;
            }
        }
        private bool _defaultReboot;
        public bool DefaultReboot
        {
            get => _defaultReboot;
            set
            {
                _defaultReboot = value;
                NotifyPropertyChanged();
                SyncModel(value);
            }
        }
        private string deviceStatusColor;
        public string DeviceStatusColor
        {
            get { return deviceStatusColor; }
            set
            {
                deviceStatusColor = value;
                NotifyPropertyChanged();
            }
        }
        //private bool deviceStatusDotShow;
        //public bool DeviceStatusDotShow
        //{
        //    get { return deviceStatusDotShow; }
        //    set
        //    {
        //        deviceStatusDotShow = value;
        //        NotifyPropertyChanged();
        //    }
        //}
        private string deviceStatus;
        public string DeviceStatus
        {
            get { return deviceStatus; }
            set
            {
                deviceStatus = value;
                NotifyPropertyChanged();
            }
        }
        private string deviceStatusColorShadow;
        public string DeviceStatusColorShadow
        {
            get { return deviceStatusColorShadow; }
            set
            {
                deviceStatusColorShadow = value;
                NotifyPropertyChanged();
            }
        }
        private bool acceptCommand;
        public bool AcceptCommand
        {
            get { return acceptCommand; }
            set
            {
                acceptCommand = value;
                NotifyPropertyChanged();
            }
        }
        private CornerRadius lastItemCorner;
        public CornerRadius LastItemCorner
        {
            get { return lastItemCorner; }
            set
            {
                lastItemCorner = value;
                NotifyPropertyChanged();
            }
        }
        private int _queryMode;
        public int QueryMode
        {
            get { return _queryMode; }
            set
            {
                _queryMode = value;
                NotifyPropertyChanged();
            }
        }
        //private bool _notifyDeviceStatusChanged;
        //public bool NotifyDeviceStatusChanged
        //{
        //    get { return _notifyDeviceStatusChanged; }
        //    set
        //    {
        //        _notifyDeviceStatusChanged = value;
        //        NotifyPropertyChanged();
        //    }
        //}
        public RelayCommand ButtonCommand {  get; private set; }
        private ILogger _logger;
        #endregion
        public DeviceViewModel(ILogger logger, DeviceModel deviceModel)
        {
            _logger = logger;
            _devicemodel = deviceModel;
            _modelproperties = _devicemodel.GetType().GetProperties();
            _viewModelproperties = this.GetType().GetProperties();
            Name = _devicemodel.Name;
            IP = _devicemodel.IP;
            MAC = _devicemodel.MAC;
            Enabled = _devicemodel.Enabled;
            DefaultReboot = _devicemodel.DefaultReboot;
            DefaultShutDown = _devicemodel.DefaultShutDown;
            DeviceStatus = _devicemodel.DeviceStatus;
            QueryMode = _devicemodel.QueryMode;
            AcceptCommand = _devicemodel.AcceptCommand;
            DeviceStatusColorShadow = "#FFFF0000";
            DeviceStatusColor = "#50FF0000";
            //DeviceStatusDotShow = true;
            _devicemodel.PropertyChanged += _syncViewModel;
            ButtonCommand = new RelayCommand(ButtonOrCommandExecute);
            _logger.Info("DeviceViewModel init done");
            
        }
        private void _syncViewModel(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var pp = _viewModelproperties.Where(p => p.Name == e.PropertyName).FirstOrDefault();
            var pp2 = _modelproperties.Where(p => p.Name == e.PropertyName).FirstOrDefault();
            var y = pp2.GetValue(_devicemodel, null);

            pp?.SetValue(this, y, null);

            //DeviceStatus = _devicemodel?.DeviceStatus;
            //DeviceStatusColor = _devicemodel?.DeviceStatusColor;
            //DeviceStatusColorShadow = _devicemodel?.DeviceStatusColorShadow;
            //AcceptCommand = _devicemodel.AcceptCommand;

            //if (e.PropertyName.Contains("NotifyDeviceStatusChanged"))
            //{
            //    //NotifyDeviceStatusChanged = _devicemodel.NotifyDeviceStatusChanged;
            //}
        }
        public void Dispose()
        {
        }
        public void StartPing() 
        {
            _devicemodel.StartPing();
        }
        public void StopPing()
        {
            _devicemodel.StopPing();
        }
        public void ButtonOrCommandExecute(object param) 
        {
            string Command = param as string;         
            _devicemodel.ExecuteCommand(Command);         
        }
        private void SyncModel(object value, [CallerMemberName] string propertyName = "" ) 
        {           
            var pp = _modelproperties.Where( p => p.Name == propertyName).First();
            pp?.SetValue(_devicemodel, value, null);
        }
    }
}
