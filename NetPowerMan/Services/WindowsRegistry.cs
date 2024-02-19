using Microsoft.Win32;
using NetPowerMan.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetPowerMan.Services
{
    internal class WindowsRegistry
    {
        private readonly ILogger _logger;
        private readonly IShowMessage _showMessage;
        public WindowsRegistry(ILogger logger, IShowMessage showMessage) 
        { 
            _logger = logger;
            _showMessage = showMessage;
        }
        public bool ReadRegistryPowerButton(string keyPath, string KeyName)
        {
            bool result = false;
            try
            {
                var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using (var key = hklm.OpenSubKey(keyPath, false)) // False is only read!
                {
                    //var ciao = key.GetValueNames();
                    var Subkey = key.GetValue(KeyName).ToString();

                    if (Subkey.Equals("1")) result = true;
                    else result = false;
                }
                hklm.Close();
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"ReadRegistryPowerButton: keyPath={keyPath}, KeyName={KeyName} \n" + ex);
                _showMessage.ShowMessageError(ex.Message, "Error");
                return false;
            }
        }
        public bool ReadRegistryStartup(string keyPath, string KeyName)
        {
            bool result = false;
            try
            {
                var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using (var key = hklm.OpenSubKey(keyPath, false)) // False is only read!
                {
                    if (key == null) return false;

                    var Subkey = key.GetValue(KeyName);

                    if (Subkey == null) return false;

                    string StartUpString = "\"" + Directory.GetCurrentDirectory() + "\\PowerManager.exe\" -background";

                    if (Subkey.ToString().Equals(StartUpString)) result = true;
                    else result = false;
                }
                hklm.Close();
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"ReadRegistryPowerButton: keyPath={keyPath}, KeyName={KeyName} \n" + ex);
                _showMessage.ShowMessageError(ex.Message, "Error");
                return false;
            }
        }
        public bool WriteRegistryPowerBotton(string keyPath, string KeyName, bool value)
        {
            bool result = false;
            try
            {
                var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using (var key = hklm.OpenSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree)) // True is important!
                {
                    if (key == null) return false;
                    if (value)
                    {
                        key.SetValue(KeyName, 1, RegistryValueKind.DWord);
                        result = true;
                    }
                    else
                    {
                        key.SetValue(KeyName, 0, RegistryValueKind.DWord);
                        result = false;
                    }
                    key.Close();
                }
                hklm.Close();
            }
            catch (SystemException ex)
            {
                _logger.Error($"WriteRegistryPowerBotton: keyPath={keyPath}, KeyName={KeyName}, value={value} \n" + ex);
                _showMessage.ShowMessageError(ex.Message, "Error");
            }
            return result;
        }
        public bool WriteRegistryStartUp(string keyPath, string KeyName, bool value)
        {
            bool result = false;
            string StartUpString = "\"" + Directory.GetCurrentDirectory() + "\\PowerManager.exe\" -background";
            try
            {
                var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                using (var key = hklm.OpenSubKey(keyPath, RegistryKeyPermissionCheck.ReadWriteSubTree)) // True is important!
                {
                    if (key == null) return false;
                    if (value)
                    {
                        key.SetValue(KeyName, StartUpString, RegistryValueKind.String);
                        result = true;
                    }
                    else
                    {
                        key.DeleteValue(KeyName);
                        result = false;
                    }
                    key.Close();
                }
                hklm.Close();
            }
            catch (SystemException ex)
            {
                _logger.Error($"WriteRegistryStartUp: keyPath={keyPath}, KeyName={KeyName}, value={value} \n" + ex);
                _showMessage.ShowMessageError(ex.Message, "Error");
            }
            return result;
        }
    }
}
