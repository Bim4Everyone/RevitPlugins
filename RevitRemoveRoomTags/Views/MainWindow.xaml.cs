using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace RevitRemoveRoomTags.Views {
    public partial class MainWindow {

        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitRemoveRoomTags);
        public override string ProjectConfigName => nameof(MainWindow);

        public ObservableCollection<ValidationError> ValidationErrors { get; set; } = new ObservableCollection<ValidationError>();

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }


        private void Window_Error(object sender, ValidationErrorEventArgs e) {

            if(e.Action == ValidationErrorEventAction.Added) {
                ValidationErrors.Add(e.Error);
            } else {
                ValidationErrors.Remove(e.Error);
            }
        }
    }
}
