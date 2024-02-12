using System.Windows.Media;

using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;

using RevitServerFolders.ViewModels.Rs;

namespace RevitServerFolders.Converters {
    internal sealed class RsNodeImageSelector : TreeListNodeImageSelector {
        public ImageSource Empty { get; set; }
        public ImageSource Server { get; set; }
        public ImageSource Folder { get; set; }
        public ImageSource OpenedFolder { get; set; }

        public override ImageSource Select(TreeListRowData rowData) {
            if(rowData.Row is RsServerDataViewModel) {
                return Server;
            }

            if(rowData.Row is RsFolderDataViewModel folderDataViewModel) {
                return folderDataViewModel.IsLoadedChildren
                    ? OpenedFolder
                    : Folder;
            }

            return Empty;
        }
    }
}
