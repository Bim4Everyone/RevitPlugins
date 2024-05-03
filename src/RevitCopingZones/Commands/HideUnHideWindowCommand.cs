using System;
using System.Windows;
using System.Windows.Input;

namespace RevitCopingZones.Commands {
    internal class HideUnHideWindowCommand : ICommand {
#pragma warning disable CS0067
        public event EventHandler CanExecuteChanged;
#pragma warning disable CS0067

        public bool CanExecute(object parameter) {
            return parameter is Window;
        }

        public void Execute(object parameter) {
            var window = (Window) parameter;
            if(window.IsVisible) {
                window.Hide();
            } else {
                window.ShowDialog();
            }
        }
    }
}