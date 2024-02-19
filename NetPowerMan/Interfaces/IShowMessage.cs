using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetPowerMan.Interfaces
{
    internal interface IShowMessage
    {
        void ShowMessageB(string message);
        void ShowMessageB(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options);
        bool ShowMessageError(string messageBoxText, string caption);
    }
}
