using NetPowerMan.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetPowerMan.Models
{
    internal class GlobalSettingsModel : IModel
    {
        public GlobalSettingsModel() 
        {
            _sessionEnding = false;
            _notifyStatus = false;
        }
        public bool _sessionEnding;
        public bool SessionEnding
        {
            get { return _sessionEnding; }
            set
            {
                _sessionEnding = value;
                NotifyPropertyChanged();
            }
        }
        public bool _notifyStatus;
        public bool NotifyStatus
        {
            get { return _notifyStatus; }
            set
            {
                _notifyStatus = value;
                NotifyPropertyChanged();
            }
        }
    }
}
