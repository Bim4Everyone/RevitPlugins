using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

using RevitCorrectNamingCheck.Models;
using System.Diagnostics;
using System.Windows.Media;

namespace RevitCorrectNamingCheck.Views.Selectors
{
    public class WorksetTemplateSelector : DataTemplateSelector {
        public DataTemplate CorrectTemplate { get; set; }
        public DataTemplate PartialCorrectTemplate { get; set; }
        public DataTemplate IncorrectTemplate { get; set; }
        public DataTemplate NoMatchTemplate { get; set; }
        public DataTemplate DisabledTemplate { get; set; }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject {
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while(parent != null && parent is not T) {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as T;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var linkedFile = FindParent<DataGridRow>(container)?.DataContext as LinkedFile;

            if(linkedFile.FileNameStatus == NameStatus.Incorrect) {
                return DisabledTemplate;
            }

            if(item is WorksetInfo worksetInfo) {
                return worksetInfo.WorksetNameStatus switch {
                    NameStatus.Correct => CorrectTemplate,
                    NameStatus.PartialCorrect => PartialCorrectTemplate,
                    NameStatus.Incorrect => IncorrectTemplate,
                    NameStatus.None => NoMatchTemplate,
                    _ => base.SelectTemplate(item, container)
                };
            }

            return base.SelectTemplate(item, container);
        }
    }
}
