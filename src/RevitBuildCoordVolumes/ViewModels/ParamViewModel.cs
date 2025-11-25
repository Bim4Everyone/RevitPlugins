using System.Text;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitBuildCoordVolumes.Models;
using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.ViewModels;

internal class ParamViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly IParamAvailabilityService _paramDefinitionService;
    private readonly ICategoryAvailabilityService _categoryAvailabilityService;

    private string _description;
    private string _detailDescription;
    private string _sourceParamName;
    private string _targetParamName;
    private string _warnings;
    private bool _isPair;
    private bool _isChecked;
    private bool _hasWarning;

    public ParamViewModel(
        ILocalizationService localizationService,
        IParamAvailabilityService paramDefinitionService,
        ICategoryAvailabilityService categoryAvailabilityService) {
        _localizationService = localizationService;
        _paramDefinitionService = paramDefinitionService;
        _categoryAvailabilityService = categoryAvailabilityService;
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
    public bool IsPair {
        get => _isPair;
        set => RaiseAndSetIfChanged(ref _isPair, value);
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
            && IsPair
            && (!_paramDefinitionService.IsParamExist(document, SourceParamName)
            || !_categoryAvailabilityService.IsParamAvailableInCategory(
                SourceParamName, Category.GetCategory(document, BuiltInCategory.OST_Areas)))) {

            string desc = _localizationService.GetLocalizedString("MainWindow.SourceParamHeader");
            warningString.Append($"  • {SourceParamName} - {desc}\n");
        }
        if(IsChecked
            && (!_paramDefinitionService.IsParamExist(document, TargetParamName)
            || !_categoryAvailabilityService.IsParamAvailableInCategory(
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
