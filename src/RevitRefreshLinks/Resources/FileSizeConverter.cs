using System;
using System.Globalization;
using System.Windows.Data;

namespace RevitRefreshLinks.Resources;
/// <summary>
/// Конвертирует байтовое представление размера файла в удобочитаемый формат
/// </summary>
[ValueConversion(typeof(long), typeof(string))]
internal class FileSizeConverter : IValueConverter {
    private const int _size = 1024;
    private static readonly string[] _units = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if(value is long size) {
            if(size == 0) {
                return string.Empty;
            }

            long bytes = Math.Abs(size);
            int place = System.Convert.ToInt32(Math.Floor(Math.Log(bytes, _size)));
            double num = Math.Round(bytes / Math.Pow(_size, place), 1);
            return (Math.Sign(size) * num).ToString(CultureInfo.CurrentCulture) + " " + _units[place];
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
