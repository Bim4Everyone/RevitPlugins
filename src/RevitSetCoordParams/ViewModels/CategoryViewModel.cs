using System.Collections.Generic;
using System.Text;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models;
using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.ViewModels;

internal class CategoryViewModel : BaseViewModel {

    private string _warnings;
    private bool _isChecked;
    private bool _hasWarning;

    public Category Category { get; set; }
    public string CategoryName { get; set; }

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
        ILocalizationService localizationService,
        List<ParamMap> paramMaps) {

        HasWarning = false;
        Warnings = string.Empty;

        var warningString = new StringBuilder();
        foreach(var paramMap in paramMaps) {
            if(paramMap.TargetParam != null && !paramAvailabilityService.IsParamAvailable(paramMap.TargetParam, Category)) {
                warningString.Append($"  • {paramMap.TargetParam.Name}\n");
            }
        }
        if(warningString.Length != 0) {
            Warnings = warningString
                .Insert(0, localizationService.GetLocalizedString("CategoryViewModel.WarningStartString") + "\n")
                .Remove(warningString.Length - 1, 1)
                .ToString();
            HasWarning = true;
        }
    }
}
