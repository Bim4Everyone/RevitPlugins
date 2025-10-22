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

    private string _sourceParamName;
    private string _targetParamName;
    private string _warnings;
    private bool _isChecked;
    private bool _hasWarning;

    public ParamViewModel(ILocalizationService localizationService, IParamAvailabilityService paramDefinitionService) {
        _localizationService = localizationService;
        _paramDefinitionService = paramDefinitionService;
    }

    public ParamMap ParamMap { get; set; }
    public string Description { get; set; }
    public string DetailDescription { get; set; }
    public bool IsPair { get; set; }

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
    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value);
    }
    public bool HasWarning {
        get => _hasWarning;
        set => RaiseAndSetIfChanged(ref _hasWarning, value);
    }

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
