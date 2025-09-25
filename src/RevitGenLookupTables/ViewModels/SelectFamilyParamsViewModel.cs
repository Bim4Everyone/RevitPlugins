using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

namespace RevitGenLookupTables.ViewModels;

internal class SelectFamilyParamsViewModel : BaseViewModel {
    private FamilyParamViewModel _chosenFamilyParam;
    private ObservableCollection<FamilyParamViewModel> _chosenFamilyParams;
    private ObservableCollection<FamilyParamViewModel> _selectedFamilyParams = [];

    public FamilyParamViewModel ChosenFamilyParam {
        get => _chosenFamilyParam;
        set => this.RaiseAndSetIfChanged(ref _chosenFamilyParam, value);
    }

    public ObservableCollection<FamilyParamViewModel> ChosenFamilyParams {
        get => _chosenFamilyParams;
        set => this.RaiseAndSetIfChanged(ref _chosenFamilyParams, value);
    }

    public ObservableCollection<FamilyParamViewModel> SelectedFamilyParams {
        get => _selectedFamilyParams;
        set => this.RaiseAndSetIfChanged(ref _selectedFamilyParams, value);
    }
}
