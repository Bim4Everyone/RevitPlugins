using System.Windows;
using System.Windows.Controls;

using DevExpress.Xpf.Grid;

using dosymep.WPF;
using dosymep.WPF.Converters;

using RevitCheckingLevels.Models;

namespace RevitCheckingLevels.ViewTemplateSelectors {
    internal class ErrorTypeGroupRowTemplateSelector : DataTemplateSelector {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate UpdateElevationTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var errorType = (item as GroupRowData).GetGroupRowValue<ErrorType>();
            return errorType == ErrorType.NotElevation
                ? UpdateElevationTemplate
                : DefaultTemplate;
        }
    }
}