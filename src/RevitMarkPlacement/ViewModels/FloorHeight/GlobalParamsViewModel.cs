using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Extensions;
using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels.FloorHeight;

internal class GlobalParamsViewModel : BaseViewModel, IFloorHeightProvider {
    private readonly IUnitProvider _unitProvider;
    private readonly IGlobalParamSelection _globalParamSelection;
    private readonly ILocalizationService _localizationService;

    private bool _isEnabled;
    private GlobalParamViewModel _globalParam;
    private ObservableCollection<GlobalParamViewModel> _globalParams;

    public GlobalParamsViewModel(
        IUnitProvider unitProvider,
        IGlobalParamSelection globalParamSelection,
        ILocalizationService localizationService) {
        _unitProvider = unitProvider;
        _globalParamSelection = globalParamSelection;
        _localizationService = localizationService;
    }

    public LevelHeightProvider LevelHeightProvider => LevelHeightProvider.GlobalParameter;

    public GlobalParamViewModel GlobalParam {
        get => _globalParam;
        set => RaiseAndSetIfChanged(ref _globalParam, value);
    }

    public ObservableCollection<GlobalParamViewModel> GlobalParams {
        get => _globalParams;
        set => this.RaiseAndSetIfChanged(ref _globalParams, value);
    }

    public bool IsEnabled {
        get => _isEnabled;
        set => RaiseAndSetIfChanged(ref _isEnabled, value);
    }

    public double? GetFloorHeight() {
        return GlobalParam?.Value;
    }

    public string GetErrorText() {
        if(GlobalParam is null) {
            return _localizationService.GetLocalizedString("MainWindow.EmptyGlobalParam");
        }

        if(GlobalParam.IsValidObject
           && GlobalParam.Value < 0.0) {
            return _localizationService.GetLocalizedString("MainWindow.NegativeGlobalParam");
        }

        return null;
    }

    public void LoadConfig(RevitSettings settings) {
        GlobalParams = new ObservableCollection<GlobalParamViewModel>(
            _globalParamSelection.GetElements()
                .Select(item => new GlobalParamViewModel(item, _unitProvider)));

        IsEnabled = GlobalParams.Count > 0;
        GlobalParam = GlobalParams.FirstOrDefault(item => item.Id == settings?.GlobalParameterId)
                      ?? GlobalParams.FirstOrDefault();
    }

    public void SaveConfig(RevitSettings settings) {
        settings.GlobalParameterId = GlobalParam?.Id;
    }
}
