using System.Windows;
using System.Windows.Controls;

using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeView;

using RevitServerFolders.ViewModels;

namespace RevitServerFolders.TemplateSelectors {
    internal sealed class FoldersTreeTemplateSelector : DataTemplateSelector {
        public DataTemplate FolderNodeTemplate { get; set; }

        public DataTemplate ModelNodeTemplate { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(item is EditGridCellData editGridCellData) {
                if(editGridCellData.Row is ModelObjectViewModel modelObjectViewModel) {
                    return modelObjectViewModel.IsFolder
                        ? FolderNodeTemplate
                        : ModelNodeTemplate;
                }
            }
            
            return null;
        }
    }
}
