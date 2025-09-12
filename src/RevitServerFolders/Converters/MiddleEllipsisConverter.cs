using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace RevitServerFolders.Converters;
/// <summary>
/// Конвертирует длинный текст из TextBlock в соответствии с текущей шириной этого TextBlock:
/// "Очень длинная строка, которая не помещается на экран" => "Очень длинная...на экран"
/// </summary>
internal class MiddleEllipsisConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if(values[0] is not string text
            || values[1] is not TextBlock textBlock
            || values[2] is not double actualWidth) {
            return DependencyProperty.UnsetValue;
        }
        if(string.IsNullOrWhiteSpace(text) || actualWidth <= 0) {
            return text;
        }

        var formattedText = GetFormattedText(text, culture, textBlock);
        if(formattedText.Width <= actualWidth) {
            return text;
        }

        int leftCount = text.Length / 2;
        int rightCount = text.Length - leftCount;
        while(leftCount > 0 && rightCount > 0) {
            string candidate = text.Substring(0, leftCount) + "..." + text.Substring(text.Length - rightCount);
            var formattedCandidate = GetFormattedText(candidate, culture, textBlock);

            if(formattedCandidate.Width <= actualWidth) {
                return candidate;
            }
            leftCount--;
            rightCount--;
        }
        return text;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }

    private FormattedText GetFormattedText(string text, CultureInfo culture, TextBlock textBlock) {
        return new FormattedText(
            text,
            culture,
            textBlock.FlowDirection,
            new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
            textBlock.FontSize,
            textBlock.Foreground,
            VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);
    }
}
