using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace RevitServerFolders.Converters;
/// <summary>
/// Сокращает длинный текст многоточием посередине по ширине, доступной под него:
/// "Очень длинная строка, которая не помещается на экран" => "Очень длинная...на экран".
/// Если текст помещается целиком, возвращается как есть.
/// </summary>
/// <remarks>
/// Принимает ровно четыре значения, по порядку:
/// <list type="number">
/// <item>
/// <term>текст </term>
/// <description><see cref="string"/> — исходная строка, которую надо сократить.</description>
/// </item>
/// <item>
/// <term>элемент</term>
/// <description>
/// <see cref="TextBlock"/>, в котором текст отображается. Нужен только чтобы посчитать
/// ширину строки его шрифтом, поэтому передается через <c>RelativeSource Self</c>.
/// </description>
/// </item>
/// <item>
/// <term>ширина контейнера</term>
/// <description>
/// <see cref="double"/> — ширина места, отведенного под строку.
/// Брать <c>ActualWidth</c> у самого <see cref="TextBlock"/> нельзя, если он лежит в
/// <c>GridView</c>: там это ширина текста, а не выделенного под него места, и сокращение
/// никогда не сработает. Поэтому передается ширина контейнера, например колонки.
/// </description>
/// </item>
/// <item>
/// <term>отступ</term>
/// <description>
/// <see cref="double"/> — сколько пикселей вычесть из предыдущего значения:
/// ширина соседних элементов контейнера и отступов, то есть все, что занято не текстом.
/// </description>
/// </item>
/// </list>
/// Если значений не четыре или тип любого из них не совпал,
/// возвращается <see cref="DependencyProperty.UnsetValue"/>.
/// </remarks>
internal class MiddleEllipsisConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        if(values is not { Length: 4 }
           || values[0] is not string text
           || values[1] is not TextBlock textBlock
           || values[2] is not double actualWidth
           || values[3] is not double offset) {
            return DependencyProperty.UnsetValue;
        }

        actualWidth -= offset;
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
