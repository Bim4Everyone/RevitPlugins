using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface IOpenFileDialog : IOpenDialogBase {
        bool AddExtension { get; set; }

        IFileModel FileName { get; set; }

        IFileModel[] FileNames { get; }

        string Filter { get; set; }
    }
}
