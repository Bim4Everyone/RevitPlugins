using RevitServerFolders.ViewModels;

using Wpf.Ui;

namespace RevitServerFolders.Views;
internal partial class NwcExportViewSettingsDialog {
    public NwcExportViewSettingsDialog(NwcExportViewSettingsViewModel vm, IContentDialogService dialogService)
        : base(dialogService.GetDialogHost()) {
        InitializeComponent();
        NwcExportViewSettings = vm;
        DataContext = vm;
    }

    public NwcExportViewSettingsViewModel NwcExportViewSettings { get; }
}
