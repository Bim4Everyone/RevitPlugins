using RevitClashDetective.ViewModels.ClashDetective;

using Wpf.Ui;

namespace RevitClashDetective.Views;
internal partial class SelectableNamesDialog {
    public SelectableNamesDialog(SelectableNamesViewModel vm, IContentDialogService dialogService)
        : base(dialogService.GetDialogHost()) {
        InitializeComponent();
        ViewModel = vm;
        DataContext = vm;
    }

    public SelectableNamesViewModel ViewModel { get; }
}
