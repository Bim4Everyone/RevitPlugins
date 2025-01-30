namespace RevitRefreshLinks.Services {
    internal interface IOpenFileDialog : IOpenDialogBase {
        bool AddExtension { get; set; }

        string FileName { get; set; }

        string[] FileNames { get; }

        string Filter { get; set; }
    }
}
