using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using DevExpress.Xpf.Grid;

using RevitOpeningPlacement.ViewModels.Navigator;

namespace RevitOpeningPlacement.Views.Selectors {
    internal class GroupedRowTemplateSelector : DataTemplateSelector {
        public DataTemplate OneParentIdTemplate { get; set; }
        public DataTemplate StandardTemplate { get; set; }
        public DataTemplate SeveralParentIdTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            var groupRowData = (GroupRowData) item;
            var presentColumnHeaders = groupRowData.CellData.Select(cell => cell.Column.Header?.ToString()).ToList();

            if(!presentColumnHeaders.Any(header => header?.Equals("ParentId", StringComparison.CurrentCulture) == true) && groupRowData.Level == 0) {
                var opening = (OpeningViewModel) groupRowData.Row;
                return opening.ParentId == 0 ? OneParentIdTemplate : SeveralParentIdTemplate;
            }
            return StandardTemplate;
        }
    }
}