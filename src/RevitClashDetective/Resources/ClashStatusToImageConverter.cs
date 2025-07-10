using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

using DevExpress.Mvvm;

namespace RevitClashDetective.Resources {
    internal class ClashStatusToImageConverter : IValueConverter {
        private static readonly BitmapImage _high = new BitmapImage(
            new Uri("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_High.png"));
        private static readonly BitmapImage _low = new BitmapImage(
            new Uri("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_Low.png"));
        private static readonly BitmapImage _normal = new BitmapImage(
            new Uri("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_Normal.png"));


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if(value is EnumMemberInfo enumInfo) {
                return GetImage(enumInfo.Name);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private BitmapImage GetImage(string statusName) {
            switch(statusName) {
                case "Активно":
                    return _high;
                case "Проанализировано":
                    return _low;
                case "Исправлено":
                    return _normal;
                default:
                    return null;
            }
        }
    }
}
