using System.Text;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitBuildCoordVolumes.Models;
using RevitBuildCoordVolumes.Models.Services;

namespace RevitBuildCoordVolumes.ViewModels;

internal class ParamViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly BuildCoordVolumeServices _buildCoordVolumeServices;
    private string _description;
    private string _detailDescription;
    private string _sourceParamName;
    private string _targetParamName;
    private string _warnings;
    private bool _hasSourceParam;
    private bool _hasTargetParam;
    private bool _isChecked;
    private bool _hasWarning;

    public ParamViewModel(ILocalizationService localizationService, BuildCoordVolumeServices buildCoordVolumeServices) {
        _localizationService = localizationService;
        _buildCoordVolumeServices = buildCoordVolumeServices;
    }

    public ParamMap ParamMap { get; set; }

    public string Description {
        get => _description;
        set => RaiseAndSetIfChanged(ref _description, value);
    }
    public string DetailDescription {
        get => _detailDescription;
        set => RaiseAndSetIfChanged(ref _detailDescription, value);
    }
    public string SourceParamName {
        get => _sourceParamName;
        set => RaiseAndSetIfChanged(ref _sourceParamName, value);
    }
    public string TargetParamName {
        get => _targetParamName;
        set => RaiseAndSetIfChanged(ref _targetParamName, value);
    }
    public string Warnings {
        get => _warnings;
        set => RaiseAndSetIfChanged(ref _warnings, value);
    }
    public bool HasSourceParam {
        get => _hasSourceParam;
        set => RaiseAndSetIfChanged(ref _hasSourceParam, value);
    }
    public bool HasTargetParam {
        get => _hasTargetParam;
        set => RaiseAndSetIfChanged(ref _hasTargetParam, value);
    }
    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }
    public bool HasWarning {
        get => _hasWarning;
        set => RaiseAndSetIfChanged(ref _hasWarning, value);
    }

    /// <summary>
    /// Метод, который обновляет предупреждения в параметрах
    /// </summary>
    public void UpdateWarning(Document document) {
        HasWarning = false;
        Warnings = string.Empty;

        var warningString = new StringBuilder();

        if(IsChecked
            && HasSourceParam
            && (!_buildCoordVolumeServices.ParamAvailabilityService.IsParamExist(document, SourceParamName)
            || !_buildCoordVolumeServices.CategoryAvailabilityService.IsParamAvailableInCategory(
                SourceParamName, Category.GetCategory(document, BuiltInCategory.OST_Areas)))) {

            string desc = _localizationService.GetLocalizedString("MainWindow.SourceParamHeader");
            warningString.Append($"  • {SourceParamName} - {desc}\n");
        }
        if(IsChecked
            && HasTargetParam
            && (!_buildCoordVolumeServices.ParamAvailabilityService.IsParamExist(document, TargetParamName)
            || !_buildCoordVolumeServices.CategoryAvailabilityService.IsParamAvailableInCategory(
                TargetParamName, Category.GetCategory(document, BuiltInCategory.OST_GenericModel)))) {
            string desc = _localizationService.GetLocalizedString("MainWindow.TargetParamHeader");
            warningString.Append($"  • {TargetParamName} - {desc}\n");
        }
        if(warningString.Length != 0) {
            Warnings = warningString
                .Insert(0, _localizationService.GetLocalizedString("ParamViewModel.WarningStartString") + "\n")
                .Remove(warningString.Length - 1, 1)
                .ToString();
            HasWarning = true;
        }
    }
}
