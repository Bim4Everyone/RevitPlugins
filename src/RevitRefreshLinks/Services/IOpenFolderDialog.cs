namespace RevitRefreshLinks.Services {
    internal interface IOpenFolderDialog : IOpenDialogBase {
        string FolderName { get; set; }

        string[] FolderNames { get; }
    }
}
