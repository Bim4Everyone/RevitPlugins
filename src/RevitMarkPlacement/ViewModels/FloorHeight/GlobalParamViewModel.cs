using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Extensions;
using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels.FloorHeight;

internal class GlobalParamViewModel : BaseViewModel {
    private readonly GlobalParameter _globalParameter;
    private readonly IUnitProvider _unitProvider;

    public GlobalParamViewModel(GlobalParameter globalParameter, IUnitProvider unitProvider) {
        _globalParameter = globalParameter;
        _unitProvider = unitProvider;

        DisplayName = GetDisplayName();
    }

    public bool IsValidObject => _globalParameter.IsValidObject;

    public ElementId Id => _globalParameter.Id;
    public string Name => _globalParameter.Name;
    public double Value => _globalParameter.AsDouble();
    public string DisplayName { get; }

    private string GetDisplayName() {
        string value = UnitFormatUtils.Format(
            _unitProvider.GetUnits(),
            SpecTypeId.Length,
            Value,
            false,
            new FormatValueOptions() { AppendUnitSymbol = true });
 
        return $"{Name} ({value})";
    }
}
