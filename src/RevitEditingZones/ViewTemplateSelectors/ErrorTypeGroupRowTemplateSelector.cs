using System.Windows;
using System.Windows.Controls;

using DevExpress.Xpf.Grid;

using dosymep.WPF;

using RevitEditingZones.Models;

namespace RevitEditingZones.ViewTemplateSelectors {
    internal class ErrorTypeGroupRowTemplateSelector : DataTemplateSelector {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate NotMatchNamesTemplate { get; set; }
        public DataTemplate NotLinkedZonesTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var errorType = (item as GroupRowData).GetGroupRowValue<ErrorType>();
            if(errorType == ErrorType.NotLinkedZones) {
                return NotLinkedZonesTemplate;
            }

            if(errorType == ErrorType.ZoneNotMatchNames) {
                return NotMatchNamesTemplate;
            }
            
            return DefaultTemplate;
        }
    }
}