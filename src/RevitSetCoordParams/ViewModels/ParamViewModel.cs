using System.Text;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.ViewModels;

internal class ParamViewModel : BaseViewModel {

    private string _sourceParamName;
    private string _targetParamName;
    private string _warnings;
    private bool _isChecked;
    private bool _hasWarning;

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

    public void UpdateWarning(
        IParamAvailabilityService paramAvailabilityService,
        ILocalizationService localizationService) {

        HasWarning = false;
        Warnings = string.Empty;

        var warningString = new StringBuilder();

        if(IsChecked && IsPair && !paramAvailabilityService.IsParamExist(SourceParamName)) {
            warningString.Append($"  • {SourceParamName}\n");
        }
        if(IsChecked && !paramAvailabilityService.IsParamExist(TargetParamName)) {
            warningString.Append($"  • {TargetParamName}\n");
        }
        if(warningString.Length != 0) {
            Warnings = warningString
                .Insert(0, localizationService.GetLocalizedString("ParamViewModel.WarningStartString") + "\n")
                .Remove(warningString.Length - 1, 1)
                .ToString();
            HasWarning = true;
        }
    }
}
