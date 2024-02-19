using NetPowerMan.Interfaces;
using NetPowerMan.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media;
using System.Windows.Threading;

namespace NetPowerMan.Models
{
    internal class DeviceModel : IModel, IDisposable
    {
        #region Variables
        public string ID {  get; set; }
        public string Name { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public string MAC { get; set; } = string.Empty;
        public int QueryMode { get; set; }
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool Enabled { get; set; } = false;
        public bool DefaultShutDown { get; set; } = false;
        public bool DefaultReboot { get; set; } = false;
        public int MessageTimeout { get; set; } = 0;
        private bool _acceptCommand;
        public bool AcceptCommand
        {
            get { return _acceptCommand; }
            set
            {
                _acceptCommand = value;
                NotifyPropertyChanged();
            }
        }
        private readonly ILogger _logger;
        private readonly IShowMessage _showMessage;
        public CancellationTokenSource DisposePing { get; set; } = new CancellationTokenSource();
        //private bool AcceptCommand = true;
        public bool DeviceSimulation { get; set; } = false;
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
        public bool _notifyDeviceStatusChanged = false;
        public bool NotifyDeviceStatusChanged
        {
            get { return _notifyDeviceStatusChanged; }
            set
            {
                _notifyDeviceStatusChanged = value;
                NotifyPropertyChanged();
            }
        }
        private string _deviceStatusColor;
        public string DeviceStatusColor
        {
            get { return _deviceStatusColor; }
            set
            {
                _deviceStatusColor = value;
                NotifyPropertyChanged();
            }
        }
        private string _deviceStatusColorShadow;
        public string DeviceStatusColorShadow
        {
            get { return _deviceStatusColorShadow; }
            set
            {
                _deviceStatusColorShadow = value;
                NotifyPropertyChanged();
            }
        }
        private DispatcherTimer _timer;
        public bool _deviceIsOnline { get; set; } = false;
        private const bool Reboot = true;
        public int DeviceRemoteResult = 0;
        const int DEVICE_REMOTE_COMMAD_PENDING = 99;
        const int DEVICE_REMOTE_COMMAD_SUCCES = 1;
        const int DEVICE_REMOTE_COMMAND_TIMEOUT = 2;
        const int DEVICE_REMOTE_COMMAD_FAIL = 3;
        const int DEVICE_REMOTE_COMMAND_NOT_POSSIBLE = 4;
        static private int Wmi_ShutDown = 12;
        static private int Wmi_Reboot = 6;
        #endregion
        public DeviceModel(Dictionary<string,string> deviceinfo, ILogger logger, IShowMessage showMessage) 
        {
            _showMessage = showMessage;
            _logger = logger;
            PropertyInfo[] properties = this.GetType().GetProperties();

            foreach (var property in properties) 
            {
                if(deviceinfo.TryGetValue(property.Name, out var value)) 
                {
                    switch (Type.GetTypeCode(property.PropertyType)) 
                    {
                        case TypeCode.String:
                            property.SetValue(this, value, null);
                            break;
                        case TypeCode.Boolean:
                            if (bool.TryParse(value, out bool c))
                                property.SetValue(this, c, null);
                            else logger.Error($"DeviceModel properties \"{property.Name}\" wrong format! Required bool");
                            break;
                        case TypeCode.Int32:
                            if (int.TryParse(value, out int b))
                                property.SetValue(this, b, null);
                            else logger.Error($"DeviceModel properties \"{property.Name}\" wrong format! Required int");
                            break;
                    }
                }
                else _logger.Error($"DeviceModel properties \"{property.Name}\" not found in config file!");
            }

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(30);
            _timer.Tick += _timer_Tick;
            _timer.Stop();

            DisposePing.Cancel();

            DeviceStatus = "Off";
            _deviceIsOnline = false;
            AcceptCommand = true;
        }
        private void _timer_Tick(object sender, EventArgs e)
        {
            AcceptCommand = true;
            DeviceRemoteResult = DEVICE_REMOTE_COMMAND_TIMEOUT;
            //DeviceWaitingAnimation = false;
            //DeviceStatusDot = !DeviceWaitingAnimation;
            _timer.Stop();
        }
        private void PingLoop() 
        {
            Task<bool>.Run(() =>
            {
                int count = 0;
                string TaskAlive = string.Empty;
                // Create a buffer of 32 bytes of data to be transmitted.
                byte[] buffer = Encoding.ASCII.GetBytes("PrimaIsPower?PrimaIsPower?PrimaI");

                while (!DisposePing.IsCancellationRequested)
                {
                    Random random = new Random();
                    var pingSender = new Ping();
                    var pingOptions = new PingOptions
                    {
                        DontFragment = true,
                        Ttl = 64
                    };

                    //if setted less than 1000 cause ping timeout less than 300ms. It's a ping bug??
                    int Pingtimeout = 1000;
                    switch (count)
                    {
                        case 0:
                            TaskAlive = ".";
                        break;
                        case 1:
                            TaskAlive = "..";
                        break;
                        case 2:
                            TaskAlive = "...";
                        break;
                        case 3:
                            TaskAlive = string.Empty;
                        break;
                    }

                    //if (!AcceptCommand) DeviceWaitingAnimation = true;

                    try
                    {
                        if (!DeviceSimulation)
                        {
                            if (NetworkInterface.GetIsNetworkAvailable())
                            {
                                int Retry = 0;
                                string FailedReason = string.Empty;

                                while (Retry < 2)
                                {
                                    var watch = System.Diagnostics.Stopwatch.StartNew();
                                    PingReply reply = pingSender.Send(IP, Pingtimeout, buffer, pingOptions);
                                    watch.Stop();

                                    switch (reply.Status)
                                    {
                                        case IPStatus.Success:
                                            
                                            DeviceStatus = "Online ";
                                            DeviceStatus += reply.RoundtripTime + "ms";

                                            if (!_deviceIsOnline)
                                            {
                                                AcceptCommand = true;
                                                NotifyDeviceStatusChanged = true;
                                                _deviceIsOnline = true;
                                            }

                                            //if (AcceptCommand) DeviceWaitingAnimation = false;

                                            DeviceStatusColor = "#FF00FF00";// Brushes.Green.ToString();
                                            DeviceStatusColorShadow = "#FF00C800";// Color.FromArgb(255, 0, 200, 0);

                                            FailedReason = string.Empty;
                                            Retry = 6;
                                            //_logger.Info(DeviceStatus + _deviceIsOnline + IP);
                                        break;

                                        case IPStatus.TimedOut:
                                            if (_deviceIsOnline)
                                            {
                                                DeviceStatus = "Online ";
                                                DeviceStatus += watch.ElapsedMilliseconds + "ms";
                                                DeviceStatusColor = "#FFFFFF00";// Brushes.Yellow.ToString();
                                                _timer.Stop();
                                                AcceptCommand = true;        
                                                //Task.Delay(1000).Wait();
                                            }
                                            //_logger.Info(DeviceStatus + _deviceIsOnline + " TimedOut " + IP);

                                            FailedReason = "Offline - " +
                                                "\n" + reply.Status.ToString() + " " + watch.ElapsedMilliseconds +
                                                "ms";
                                            Retry++;
                                            Task.Delay(random.Next(10, 150) + Retry * 10).Wait();
                                        break;

                                        default:
                                            FailedReason = "Offline - " +
                                                "\n" + reply.Status.ToString() + " " + watch.ElapsedMilliseconds + "ms";
                                            Retry++;

                                            if (_deviceIsOnline)
                                            {
                                                _timer.Stop();
                                                AcceptCommand = true;
                                            }
                                            //_logger.Info(DeviceStatus + _deviceIsOnline + " Default " + IP);
                                            Task.Delay(random.Next(10, 150) + Retry * 10).Wait();
                                        break;
                                    }
                                }

                                //Failed more than 3 time
                                if (Retry != 6)
                                {                                
                                    DeviceStatus = FailedReason;
                                    if (_deviceIsOnline) NotifyDeviceStatusChanged = true;

                                    _deviceIsOnline = false;
                                    //if (AcceptCommand) DeviceWaitingAnimation = false;

                                    DeviceStatusColor = "#FFFF0000";// Brushes.Red.ToString();
                                    DeviceStatusColorShadow = "#C8FF0000"; // Color.FromArgb(200, 255, 0, 0);
                                }
                            }
                            else
                            {
                                DeviceStatus = "Offline - NetworkInterfaceNotFound";
                                _deviceIsOnline = false;
                                DeviceStatus += TaskAlive;
                                //if (AcceptCommand) DeviceWaitingAnimation = false;

                                DeviceStatusColor = "#FFFF0000";// Brushes.Red.ToString();
                                DeviceStatusColorShadow = "#FFFF0000";// Color.FromArgb(255, 255, 0, 0);
                            }
                        }
                        else
                        {
                            DeviceStatus = "Simul";
                            DeviceStatus += TaskAlive;
                            DeviceStatusColor = "#FFFFA500";// Brushes.Orange.ToString();
                            DeviceStatusColorShadow = "64C87F27"; // Color.FromArgb(100, 200, 127, 39);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException == null) _logger.Error("PingLoop\n" + ex);
                        else
                        {
                            //Could happen if you disconnect the network connection
                            _logger.Info("PingLoop\n" + ex);
                        }
                    }

                    pingSender.Dispose();
                    Task.Delay(500 + random.Next(10, 200)).Wait();
                    count++;
                    if (count == 4) count = 0;
                }
                return true;
            });
        }
        public void StopPing()
        {
            DisposePing.Cancel();
        }
        public void StartPing()
        {
            if (DisposePing.IsCancellationRequested) 
            {
                DisposePing = new CancellationTokenSource();
                PingLoop();             
            }
        }
        public void Dispose()
        {
            DisposePing.Cancel();
        }
        public  void ExecuteCommand(string command) 
        {
            if (AcceptCommand && Enabled)
            {
                bool CommandExecuted = true;
                DeviceRemoteResult = DEVICE_REMOTE_COMMAD_PENDING;

                switch (command)
                {
#pragma warning disable CS4014
                    case "MainReboot":
                        if (DefaultReboot && _deviceIsOnline) ShutDownSelector(Reboot);
                        else CommandExecuted = false;
                        break;
                    case "MainShutDown":
                        if (DefaultShutDown && _deviceIsOnline) ShutDownSelector(!Reboot);
                        else CommandExecuted = false;
                        break;
                    case "ForceShutDown":
                        ShutDownSelector(!Reboot);
                        break;
                    case "ThisReboot":
                        if (_deviceIsOnline) ShutDownSelector(Reboot);
                        else Network.WakeOnLan(MAC); 
                        break;
                    case "ThisShutDown":
                        if (_deviceIsOnline) ShutDownSelector(!Reboot);
                        else Network.WakeOnLan(MAC);
                        break;
                    default:
                        CommandExecuted = false;
                        break;
#pragma warning restore CS4014
                }
                if (CommandExecuted)
                {
                    AcceptCommand = false;
                    _timer.Start();
                }
                else DeviceRemoteResult = DEVICE_REMOTE_COMMAND_NOT_POSSIBLE;
            }
            else DeviceRemoteResult = DEVICE_REMOTE_COMMAND_NOT_POSSIBLE;
        }
        private async Task ShutDownSelector(bool reboot)
        {
            int Wmicommand;
            string PowerShellCommand = string.Empty;

            //DeviceRemoteResult = DEVICE_REMOTE_COMMAD_PENDING;

            if (reboot)
            {
                Wmicommand = Wmi_Reboot;
                PowerShellCommand = "Restart-Computer";
            }
            else
            {
                Wmicommand = Wmi_ShutDown;
                PowerShellCommand = "Stop-Computer";
            }

            switch (QueryMode)
            {
                case 0:
                    WmiRemoteWrapper wmiRemoteWrapper = new WmiRemoteWrapper(_logger, _showMessage);
                    var zero = wmiRemoteWrapper.WMI_Remote(IP, Message, MessageTimeout, User, Password, Wmicommand, Name);
                    await Task.WhenAll(zero);
                    if (!zero.Result) DeviceRemoteResult = DEVICE_REMOTE_COMMAD_SUCCES;
                    else DeviceRemoteResult = DEVICE_REMOTE_COMMAD_FAIL;
                    break;
                case 1:
                    //var two = PowerShell_Remote(PowerShellCommand);
                    //await Task.WhenAll(two);
                    //if (!two.Result) DeviceRemoteResult = DEVICE_REMOTE_COMMAD_SUCCES;
                    //else DeviceRemoteResult = DEVICE_REMOTE_COMMAD_FAIL;
                    DeviceRemoteResult = DEVICE_REMOTE_COMMAD_FAIL;
                    break;
                default:
                    SystemShutdownExWrapper systemShutdownExWrapper = new SystemShutdownExWrapper(_logger);
                    var one = systemShutdownExWrapper.ExecuteCommand(IP,Message,MessageTimeout,User,Password,true, reboot);
                    await Task.WhenAll(one);
                    if (one.Result) DeviceRemoteResult = DEVICE_REMOTE_COMMAD_SUCCES;
                    else DeviceRemoteResult = DEVICE_REMOTE_COMMAD_FAIL;
                    break;
            }
            if(DeviceRemoteResult == DEVICE_REMOTE_COMMAD_FAIL) 
            {
                AcceptCommand = true;
                _timer.Stop();
            }
        }
    }
}
