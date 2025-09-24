using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitGenLookupTables.ViewModels;

internal class FamilyParamViewModel : BaseViewModel {
    private readonly FamilyParameter _familyParameter;

    private string _columnMetadata;
    private FamilyParamValuesViewModel _familyParamValues;

    public FamilyParamViewModel(FamilyParameter familyParameter) {
        _familyParameter = familyParameter;
    }

    public string Name => _familyParameter.Definition.Name;
    public StorageType StorageType => _familyParameter.StorageType;

    public string ColumnMetadata {
        get => _columnMetadata;
        set => this.RaiseAndSetIfChanged(ref _columnMetadata, value);
    }

    public FamilyParamValuesViewModel FamilyParamValues {
        get => _familyParamValues;
        set => this.RaiseAndSetIfChanged(ref _familyParamValues, value);
    }
}
