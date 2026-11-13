using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitVolumeModifier.Interfaces;
using RevitVolumeModifier.Models;

namespace RevitVolumeModifier.ViewModels;

internal class ParamViewModel : BaseViewModel {

    private string _revitParamName;
    private string _description;
    private string _detailDescription;
    private bool _hasWarning;
    private string _warning;

    public ParamModel ParamModel { get; set; }

    public string RevitParamName {
        get => _revitParamName;
        set => RaiseAndSetIfChanged(ref _revitParamName, value);
    }
    public string Description {
        get => _description;
        set => RaiseAndSetIfChanged(ref _description, value);
    }
    public string DetailDescription {
        get => _detailDescription;
        set => RaiseAndSetIfChanged(ref _detailDescription, value);
    }
    public bool HasWarning {
        get => _hasWarning;
        set => RaiseAndSetIfChanged(ref _hasWarning, value);
    }
    public string Warning {
        get => _warning;
        set => RaiseAndSetIfChanged(ref _warning, value);
    }

    /// <summary>
    /// Метод, который обновляет предупреждения в параметре
    /// </summary>
    public void UpdateWarning(Document document, ILocalizationService localizationService, IParamAvailabilityService paramAvailabilityService) {
        if(!paramAvailabilityService.IsParamExist(document, RevitParamName)) {
            HasWarning = true;
            Warning = localizationService.GetLocalizedString("ParamViewModel.NoParam");
            return;
        }
        HasWarning = false;
        Warning = string.Empty;
    }
}
