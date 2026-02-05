using System.Text;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.ViewModels;

internal class ParamViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly IParamAvailabilityService _paramDefinitionService;

    private string _description;
    private string _detailDescription;
    private string _sourceParamName;
    private string _targetParamName;
    private string _warnings;
    private bool _isPair;
    private bool _isChecked;
    private bool _hasWarning;

    public ParamViewModel(ILocalizationService localizationService, IParamAvailabilityService paramDefinitionService) {
        _localizationService = localizationService;
        _paramDefinitionService = paramDefinitionService;
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
    public void UpdateWarning(Document sourceDoc, Document targetDoc) {
        HasWarning = false;
        Warnings = string.Empty;

        var warningString = new StringBuilder();

        if(IsChecked && IsPair && !_paramDefinitionService.IsParamExist(sourceDoc, SourceParamName)) {
            string desc = _localizationService.GetLocalizedString("MainWindow.SourceParamHeader");
            warningString.Append($"  • {SourceParamName} - {desc}\n");
        }
        if(IsChecked && !_paramDefinitionService.IsParamExist(targetDoc, TargetParamName)) {
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
