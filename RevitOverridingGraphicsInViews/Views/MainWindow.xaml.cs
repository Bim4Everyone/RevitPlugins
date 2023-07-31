using System.Diagnostics;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Media;
using Microsoft.Win32;
using System.Windows.Media;
using System.IO;
using System.Reflection;
using Autodesk.Revit.UI;

namespace RevitOverridingGraphicsInViews.Views {
    public partial class MainWindow {

        public MainWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitOverridingGraphicsInViews);
        public override string ProjectConfigName => nameof(MainWindow);


        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            if(e.ChangedButton == MouseButton.Left) {
                this.DragMove();
            }
        }

        private void WindowCloseCommand(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) {

            SoundPlayer player = new SoundPlayer();
            player.Stream = Properties.Resources.PaintSound;
            player.Play();

            this.Close();
        }


        private void ColorButton_Click(object sender, RoutedEventArgs e) {

            SoundPlayer player = new SoundPlayer();
            player.Stream = Properties.Resources.SelectColorSound;
            player.Play();
        }
    }
}