using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;

using RevitServerFolders.ViewModels;

namespace RevitServerFolders.TemplateSelectors {
    internal sealed class TreeImageSelector : TreeListNodeImageSelector {
        public ImageSource Empty { get; set; }
        public ImageSource Model { get; set; }
        public ImageSource Folder { get; set; }
        
        public override ImageSource Select(TreeListRowData rowData) {
            if(rowData.Row is ModelObjectViewModel modelObjectViewModel) {
                if(modelObjectViewModel.IsFolder) {
                    return Folder;
                }

                return Path.GetExtension(modelObjectViewModel.Name)
                    .Equals(".rvt", StringComparison.OrdinalIgnoreCase)
                    ? Model
                    : Empty;
            }

            return Empty;
        }
    }
}
