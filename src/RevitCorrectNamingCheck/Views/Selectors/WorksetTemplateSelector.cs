using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using RevitCorrectNamingCheck.Models;

namespace RevitCorrectNamingCheck.Views.Selectors;

public class WorksetTemplateSelector : DataTemplateSelector {
    public DataTemplate CorrectTemplate { get; set; }
    public DataTemplate PartialCorrectTemplate { get; set; }
    public DataTemplate IncorrectTemplate { get; set; }
    public DataTemplate NoMatchTemplate { get; set; }
    public DataTemplate DisabledTemplate { get; set; }

    private static T FindParent<T>(DependencyObject child) where T : DependencyObject {
        var parent = VisualTreeHelper.GetParent(child);
        while(parent is not null and not T) {
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent as T;
    }

    public override DataTemplate SelectTemplate(object item, DependencyObject container) {
        var linkedFile = FindParent<DataGridRow>(container)?.DataContext as LinkedFile;

        return linkedFile.FileNameStatus == NameStatus.Incorrect
            ? DisabledTemplate
            : item is WorksetInfo worksetInfo
            ? worksetInfo.WorksetNameStatus switch {
                NameStatus.Correct => CorrectTemplate,
                NameStatus.PartialCorrect => PartialCorrectTemplate,
                NameStatus.Incorrect => IncorrectTemplate,
                NameStatus.None => NoMatchTemplate,
                _ => base.SelectTemplate(item, container)
            }
            : base.SelectTemplate(item, container);
    }
}
