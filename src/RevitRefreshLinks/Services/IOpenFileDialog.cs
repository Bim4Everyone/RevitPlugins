using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal interface IOpenFileDialog : IOpenDialogBase {
    IFileModel File { get; }

    IFileModel[] Files { get; }

    string Filter { get; set; }
}
