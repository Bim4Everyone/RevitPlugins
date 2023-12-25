using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RevitRemoveRoomTags.Views {
    public partial class MainWindow {
        
        private List<ValidationError> _validationErrors = new List<ValidationError>();

        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitRemoveRoomTags);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }


        private void Window_Error(object sender, ValidationErrorEventArgs e) {

            if(e.Action == ValidationErrorEventAction.Added) {
                _validationErrors.Add(e.Error);
            } else {
                _validationErrors.Remove(e.Error);
            }

            if(_validationErrors.Count == 0) {
                AcceptViewButton.IsEnabled = true;
                ErrorTextBlock.Text = string.Empty;
            } else {
                AcceptViewButton.IsEnabled = false;
                ErrorTextBlock.Text = _validationErrors[0].ErrorContent.ToString();
            }
        }
    }
}
