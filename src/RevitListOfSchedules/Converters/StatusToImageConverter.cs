using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RevitListOfSchedules.Converters {

    public class StatusToImageConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                string status = value as string;

                return status switch {
                    "Overlay_Loaded" => new BitmapImage(new Uri("pack://application:,,,/RevitListOfSchedules_2022;component/Resources/OverlayLoaded.png")),
                    "Overlay_Unloaded" => new BitmapImage(new Uri("pack://application:,,,/RevitListOfSchedules_2022;component/Resources/OverlayUnloaded.png")),
                    "Attachment_Loaded" => new BitmapImage(new Uri("pack://application:,,,/RevitListOfSchedules_2022;component/Resources/AttachmentLoaded.png")),
                    "Attachment_Unloaded" or "Attachment_NotFound" => new BitmapImage(new Uri("pack://application:,,,/RevitListOfSchedules_2022;component/Resources/AttachmentUnloaded.png")),
                    "Overlay_NotFound" => new BitmapImage(new Uri("pack://application:,,,/RevitListOfSchedules_2022;component/Resources/OverlayUnloaded.png")),
                    "Overlay_LocallyUnloaded" or "Attachment_LocallyUnloaded" => new BitmapImage(new Uri("pack://application:,,,/RevitListOfSchedules_2022;component/Resources/LocalUnloaded.png")),
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
    }


}

