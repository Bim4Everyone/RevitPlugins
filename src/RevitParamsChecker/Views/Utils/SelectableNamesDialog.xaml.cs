using Wpf.Ui;

namespace RevitParamsChecker.Views.Utils;

internal partial class SelectableNamesDialog {
    public SelectableNamesDialog(IContentDialogService dialogService)
        : base(dialogService.GetDialogHost()) {
        InitializeComponent();
    }
}
