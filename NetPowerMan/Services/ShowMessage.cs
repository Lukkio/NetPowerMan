using NetPowerMan.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetPowerMan.Services
{
    internal class ShowMessage : IShowMessage
    {
        public void ShowMessageB( string message) 
        { MessageBox.Show(message); }
        public void ShowMessageB(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {  MessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult, options); }
        public bool ShowMessageError(string messageBoxText, string caption)
        { return MessageBox.Show(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error)==MessageBoxResult.OK; }
    }
}
