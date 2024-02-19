using NetPowerMan.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetPowerMan.Services
{
    internal class WmiRemoteWrapper
    {
        private readonly ILogger _logger;
        private readonly IShowMessage _showMessage;
        public WmiRemoteWrapper(ILogger logger, IShowMessage showMessage) {  _logger = logger; _showMessage = showMessage; }
        public Task<bool> WMI_Remote(string DeviceIP, string DeviceMessage, int messageTimeout, string User, string Password, int flags, string DeviceName)
        {
            uint result = 1;

            return Task.Run(() =>
            {
                try
                {
                    // Build an options object for the remote connection
                    // if you plan to connect to the remote
                    // computer with a different user name
                    // and password than the one you are currently using.
                    ConnectionOptions op = new ConnectionOptions();
                    op.Username = User;
                    op.Password = Password;
                    op.EnablePrivileges = true;

                    // Make a connection to a remote computer.  
                    ManagementScope scope = new ManagementScope(@"\\" + DeviceIP + @"\root\cimv2", op);

                    scope.Connect();
                    if (!scope.IsConnected)
                    {
                        System.Windows.MessageBox.Show("Failed to connect to WMI");
                        return true;//true means error
                    }
                    //Query system for Operating System information  
                    ObjectQuery oq = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                    ManagementObjectSearcher query = new ManagementObjectSearcher(scope, oq);
                    //ManagementObjectCollection queryCollection = query.Get();
                    foreach (ManagementObject obj in query.Get())
                    {
                        //https://learn.microsoft.com/it-it/windows/win32/cimwin32prov/win32shutdowntracker-method-in-class-win32-operatingsystem
                        //uint32 Win32ShutdownTracker(
                        //  [in] uint32 Timeout, in seconds
                        //  [in] string Comment, Message to visualize
                        //  [in] uint32 ReasonCode, 
                        //  [in] sint32 Flags, shutdown 1, shutdown force 5, reboot 6, force reboot 8
                        //);

                        //int timeoutint = int.TryParse(messageTimeout, out timeoutint) ? timeoutint : 0;

                        var inParameters = obj.GetMethodParameters("Win32ShutdownTracker");
                        inParameters["Flags"] = flags;
                        inParameters["Timeout"] = messageTimeout;
                        inParameters["Comment"] = DeviceMessage;
                        inParameters["ReasonCode"] = 0;

                        ManagementBaseObject bo = obj.InvokeMethod("Win32ShutdownTracker", inParameters, null);
                        result = Convert.ToUInt32(bo.Properties["ReturnValue"].Value);
                        if (result == 0) return false; //false means success
                        else return true; //true means error                       
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                    msg += "\n IP: " + DeviceIP;
                    msg += "\n Device: " + DeviceName;
                    msg += "\n User: " + User;
                    //System.Windows.MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly);
                    _showMessage.ShowMessageError(msg, "Error");
                    result = 0;

                    //Ready = true; //Reset timer case of failure
                    return true;//true means error
                }
                return true;//true means error
            });
        }
    }
}
