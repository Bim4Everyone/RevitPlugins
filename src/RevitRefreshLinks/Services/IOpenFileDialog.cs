using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface IOpenFileDialog : IOpenDialogBase {
        bool AddExtension { get; set; }

        IFileModel File { get; set; }

        IFileModel[] Files { get; }

        string Filter { get; set; }
    }
}
