using NetPowerMan.Interfaces;
using NetPowerMan.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetPowerMan.Views;

namespace NetPowerMan.Services
{
    internal class ShowSettingsService : IShowSettings
    {
        public void CloseWindow()
        {
            throw new NotImplementedException();
        }
        public void OpenWindow(SettingsViewModel settingsViewModel)
        {
            SettingsWindowView settingsWindowView = new SettingsWindowView() { Topmost = true };
            settingsWindowView.DataContext = settingsViewModel;
            settingsWindowView.Show();
        }
    }
}
