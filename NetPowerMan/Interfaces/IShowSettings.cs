using NetPowerMan.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetPowerMan.Interfaces
{
    internal interface IShowSettings
    {
        void OpenWindow(SettingsViewModel settingsViewModel);
        void CloseWindow();
    }
}
