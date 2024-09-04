using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Autodesk.Revit.DB;

using RevitAxonometryViews.ViewModels;

namespace RevitAxonometryViews.Views {
    public partial class MainWindow {
        protected CollectionViewSource SystemsCollection;

        public MainWindow() {
            InitializeComponent();            
        }

        // override-ы нужны потому что мы удалили конфиг, в котором переопределяются эти вещи
        public override string PluginName => nameof(RevitAxonometryViews);

        public override string ProjectConfigName => nameof(MainWindow);

        private void Button_Click_Ok(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            ChangeSelected(true);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            ChangeSelected(false);
        }

        private void ChangeSelected(bool state) {
            foreach(HvacSystemViewModel hvacSystem in lvSystems.SelectedItems) {
                hvacSystem.IsSelected = state;
            }
        }
    }
}

