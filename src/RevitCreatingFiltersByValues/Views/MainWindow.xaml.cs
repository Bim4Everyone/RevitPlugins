using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitCreatingFiltersByValues.Models;

namespace RevitCreatingFiltersByValues.Views {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }


        public override string PluginName => nameof(RevitCreatingFiltersByValues);
        public override string ProjectConfigName => nameof(MainWindow);

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void window_Loaded(object sender, RoutedEventArgs e) {
            expander.MaxHeight = window.ActualHeight * 0.88;
        }
    }
}