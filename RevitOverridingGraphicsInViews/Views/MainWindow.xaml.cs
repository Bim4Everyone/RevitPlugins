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

        private MediaPlayer player = new MediaPlayer();
        private Uri SelectColorSoundUri;
        private Uri PaintSoundUri;

        public MainWindow() {
            InitializeComponent();

            GetUri();
        }

        public override string PluginName => nameof(RevitOverridingGraphicsInViews);
        public override string ProjectConfigName => nameof(MainWindow);


        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            if(e.ChangedButton == MouseButton.Left) {
                this.DragMove();
            }
        }

        private void WindowCloseCommand(object sender, System.Windows.Input.ExecutedRoutedEventArgs e) {

            player.Open(PaintSoundUri);
            player.Volume = 0.01;
            player.Play();

            this.Close();
        }

        private void GetUri() {

            string executableFilePath = Assembly.GetExecutingAssembly().Location;
            string executableDirectoryPath = Path.GetDirectoryName(executableFilePath);

            string selectColorSoundFilePath = Path.Combine(executableDirectoryPath, "Views/Sounds/SelectColor.wav");
            string paintSoundFilePath = Path.Combine(executableDirectoryPath, "Views/Sounds/Paint.wav");

            SelectColorSoundUri = new Uri(selectColorSoundFilePath);
            PaintSoundUri = new Uri(paintSoundFilePath);
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e) {

            player.Open(SelectColorSoundUri);
            player.Volume = 0.01;
            player.Play();
        }
    }
}