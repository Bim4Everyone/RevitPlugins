using System.Windows.Controls;

using RevitGenLookupTables.ViewModels;

using Wpf.Ui;

namespace RevitGenLookupTables.Views.Dialogs;

internal partial class FamilyParamsDialog {
    public FamilyParamsDialog(SelectFamilyParamsViewModel viewModel, IContentDialogService contentDialogService)
        : base(contentDialogService.GetDialogHost()) {
        InitializeComponent();
        DataContext = viewModel;
    }
}
