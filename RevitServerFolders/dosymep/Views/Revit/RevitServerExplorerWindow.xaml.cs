﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using dosymep.Views.Revit;

namespace RevitServerFolders.dosymep.Views.Revit {
    /// <summary>
    /// Interaction logic for RevitServerExplorerWindow.xaml
    /// </summary>
    public partial class RevitServerExplorerWindow : Window {
        public RevitServerExplorerWindow() {
            InitializeComponent();
        }

        public RevitServerViewModel ViewModel {
            get { return _revitServerTree.DataContext as RevitServerViewModel; }
            set { _revitServerTree.DataContext = value; }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
