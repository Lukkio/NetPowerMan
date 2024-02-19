using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetPowerMan.Services.WindowsUnmanaged;

namespace NetPowerMan.Services
{
    internal class SystemShutdownExWrapper
    {
        private ILogger _logger;
        private const int ERROR_SESSION_CREDENTIAL_CONFLICT = 1219;
        public SystemShutdownExWrapper(ILogger logger) 
        { 
            _logger = logger;
        }
        public Task<bool> ExecuteCommand(string DeviceIP, string DeviceMessage, int messageTimeout, string User, string Password, bool force, bool reboot)
        {
            return Task.Run(() =>
            {
                bool result = false;
                try
                {
                    using (NetworkShareAccesser.Access(DeviceIP, "WORKGROUP", User, Password))
                    {
                        result = InitiateSystemShutdownEx(DeviceIP, DeviceMessage, (uint)messageTimeout, force, reboot, 0x00000000);
                        //var two = ExecuteCLICommand("shutdown.exe", string.Format("-s -m \\\\{0} /f /t {2} /c \"{1}\"\"", DeviceIP, DeviceMessage, MessageTimeout));
                        Task.Delay(5);
                    }
                    return result;
                }
                catch (Win32Exception ex)
                {
                    //connection already enstabilished with this user
                    if (ex.NativeErrorCode == ERROR_SESSION_CREDENTIAL_CONFLICT)
                    {
                        result = InitiateSystemShutdownEx(DeviceIP, DeviceMessage, (uint)messageTimeout, force, reboot, 0x00000000);
                        //Task.Delay(5);
                    }
                    else System.Windows.MessageBox.Show(ex.Message + ex.ErrorCode.ToString() + ":" + ex.NativeErrorCode.ToString());
                    return result;
                }
            });
        }
    }
}
