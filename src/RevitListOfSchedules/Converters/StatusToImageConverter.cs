using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RevitListOfSchedules.Converters {

    public class StatusToImageConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                string status = value as string;

                return status switch {
                    "Overlay_Loaded" => new BitmapImage(new Uri($"{GetImagePath()}OverlayLoaded.png")),
                    "Overlay_Unloaded" => new BitmapImage(new Uri($"{GetImagePath()}OverlayUnloaded.png")),
                    "Attachment_Loaded" => new BitmapImage(new Uri($"{GetImagePath()}AttachmentLoaded.png")),
                    "Attachment_Unloaded"
                    or "Attachment_NotFound" => new BitmapImage(new Uri($"{GetImagePath()}AttachmentUnloaded.png")),
                    "Overlay_NotFound" => new BitmapImage(new Uri($"{GetImagePath()}OverlayUnloaded.png")),
                    "Overlay_LocallyUnloaded"
                    or "Attachment_LocallyUnloaded" => new BitmapImage(new Uri($"{GetImagePath()}LocalUnloaded.png")),
                    _ => null,
                };
            } catch(Exception ex) {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private string GetImagePath() {
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            return $"pack://application:,,,/{assemblyName};component/Resources/";
        }
    }
}

