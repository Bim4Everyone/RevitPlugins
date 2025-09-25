using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RevitGenLookupTables.ViewModels;
using RevitGenLookupTables.Views.Dialogs;

using Wpf.Ui;
using Wpf.Ui.Controls;

namespace RevitGenLookupTables.Services;

internal sealed class SelectFamilyParamsService : ISelectFamilyParamsService {
    private readonly IContentDialogService _contentDialogService;

    public SelectFamilyParamsService(IContentDialogService contentDialogService) {
        _contentDialogService = contentDialogService;
    }

    public IReadOnlyCollection<FamilyParamViewModel> SelectedFamilyParams { get; private set; }

    public async Task<bool?> ShowDialogAsync(
        IEnumerable<FamilyParamViewModel> familyParams,
        CancellationToken cancellationToken = default) {
        var viewModel = new SelectFamilyParamsViewModel() { ChosenFamilyParams = [..familyParams] };
        var contentDialog = new FamilyParamsDialog(viewModel, _contentDialogService);

        var result = await contentDialog.ShowAsync(cancellationToken);
        if(result == ContentDialogResult.Primary) {
            SelectedFamilyParams = viewModel.SelectedFamilyParams.ToArray();
            return true;
        }

        SelectedFamilyParams = null;
        return result == ContentDialogResult.None ? null : false;
    }
}
