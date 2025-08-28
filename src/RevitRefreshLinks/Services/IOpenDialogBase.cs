namespace RevitRefreshLinks.Services;
internal interface IOpenDialogBase {
    string Title { get; set; }

    string InitialDirectory { get; set; }

    bool MultiSelect { get; set; }

    bool ShowDialog();
}
