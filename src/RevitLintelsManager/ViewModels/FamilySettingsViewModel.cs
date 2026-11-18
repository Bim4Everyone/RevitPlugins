using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Models.Settings;
using RevitLintelsManager.Services;

namespace RevitLintelsManager.ViewModels;

internal class FamilySettingsViewModel : BaseViewModel {
    public event Action SelectionChanged;

    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly ILocalizationService _localizationService;
    private readonly Dictionary<string, LintelFamilySettings> _familySettings = [];
    private ObservableCollection<FamilyViewModel> _lintelFamilyViewModels;
    private FamilyViewModel _selectedLintelFamilyViewModel;
    private ObservableCollection<ParamViewModel> _lintelParametersDouble;
    private ObservableCollection<ParamViewModel> _lintelParametersInteger;
    private ObservableCollection<ParamViewModel> _openingParameters;
    private ParamViewModel _selectedLintelWidthParam;
    private ParamViewModel _selectedLintelThicknessParam;
    private ParamViewModel _selectedLintelRightOffsetParam;
    private ParamViewModel _selectedLintelLeftOffsetParam;
    private ParamViewModel _selectedLintelRightCornerParam;
    private ParamViewModel _selectedLintelLeftCornerParam;
    private ParamViewModel _selectedLintelRightWeldingParam;
    private ParamViewModel _selectedLintelLeftWeldingParam;
    private ParamViewModel _selectedOpeningHeightParam;
    private ParamViewModel _selectedOpeningWidthParam;
    private ObservableCollection<FamilyViewModel> _openingFamilyViewModels;
    private ObservableCollection<FamilyViewModel> _selectedOpeningFamilyViewModels;
    private ObservableCollection<FamilyViewModel> _filteredLintelFamilies;
    private ObservableCollection<FamilyViewModel> _filteredOpeningFamilies;
    private string _lintelSearchText;
    private string _openingSearchText;

    public FamilySettingsViewModel(RevitRepository revitRepository, SystemPluginConfig systemPluginConfig,
        ILocalizationService localizationService, LintelManagerSettings settings, bool hasSavedSettings,
        SettingsCommandFactory commandFactory) {
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
        _localizationService = localizationService;
        SelectLintelFamilyCommand = commandFactory.Create<FamilyViewModel>(
            family => SelectedLintelFamilyViewModel = family);
        SearchLintelFamiliesCommand = commandFactory.Create(ApplySearchLintelFamilies);
        SearchOpeningFamiliesCommand = commandFactory.Create(ApplySearchOpeningFamilies);
        LintelFamilyViewModels = new ObservableCollection<FamilyViewModel>(
            _revitRepository.LintelFamilies.OrderBy(family => family.Family.Name).Select(CreateFamilyViewModel));
        OpeningFamilyViewModels = new ObservableCollection<FamilyViewModel>(
            _revitRepository.OpeningFamilies.OrderBy(family => family.Family.Name).Select(CreateFamilyViewModel));
        if(settings.LintelFamilySettings?.LintelFamily is not null) {
            _familySettings[settings.LintelFamilySettings.LintelFamily] = settings.LintelFamilySettings;
        }
        SelectedLintelFamilyViewModel = LintelFamilyViewModels.FirstOrDefault(
            family => family.Name == settings.LintelFamilySettings?.LintelFamily);
        if(SelectedLintelFamilyViewModel is null) {
            UpdateLintelParameters();
        }
        var selectedNames = (settings.OpeningFamilySettings?.FamilyNames ?? []).ToHashSet();
        if(hasSavedSettings) {
            foreach(string name in selectedNames.Except(OpeningFamilyViewModels.Select(family => family.Name))) {
                OpeningFamilyViewModels.Add(new FamilyViewModel { Name = name });
            }
        }
        SelectedOpeningFamilyViewModels = [];
        foreach(var family in OpeningFamilyViewModels) {
            family.IsChecked = selectedNames.Contains(family.Name);
            if(family.IsChecked) {
                SelectedOpeningFamilyViewModels.Add(family);
            }
            family.PropertyChanged += OnOpeningFamilyChanged;
        }
        UpdateOpeningParameters(settings.OpeningFamilySettings?.LintelOpeningHeight,
            settings.OpeningFamilySettings?.LintelOpeningWidth);
        ApplySearchLintelFamilies();
        ApplySearchOpeningFamilies();
    }

    public ICommand SearchLintelFamiliesCommand { get; }
    public ICommand SearchOpeningFamiliesCommand { get; }

    public ObservableCollection<FamilyViewModel> FilteredLintelFamilies {
        get => _filteredLintelFamilies;
        private set => RaiseAndSetIfChanged(ref _filteredLintelFamilies, value);
    }

    public ObservableCollection<FamilyViewModel> FilteredOpeningFamilies {
        get => _filteredOpeningFamilies;
        private set => RaiseAndSetIfChanged(ref _filteredOpeningFamilies, value);
    }
    public ICommand SelectLintelFamilyCommand { get; }

    public string LintelSearchText {
        get => _lintelSearchText;
        set => RaiseAndSetIfChanged(ref _lintelSearchText, value);
    }

    public string OpeningSearchText {
        get => _openingSearchText;
        set => RaiseAndSetIfChanged(ref _openingSearchText, value);
    }

    public ObservableCollection<FamilyViewModel> LintelFamilyViewModels {
        get => _lintelFamilyViewModels;
        set => RaiseAndSetIfChanged(ref _lintelFamilyViewModels, value);
    }
    
    public FamilyViewModel SelectedLintelFamilyViewModel {
        get => _selectedLintelFamilyViewModel;
        set {
            if(_selectedLintelFamilyViewModel == value) {
                return;
            }
            if(_selectedLintelFamilyViewModel is not null) {
                _familySettings[_selectedLintelFamilyViewModel.Name] = GetLintelSettings();
            }
            RaiseAndSetIfChanged(ref _selectedLintelFamilyViewModel, value);
            foreach(var family in LintelFamilyViewModels) {
                family.IsChecked = family == value;
            }
            UpdateLintelParameters();
            SelectionChanged?.Invoke();
        }
    }
    
    public ObservableCollection<ParamViewModel> LintelParametersDouble {
        get => _lintelParametersDouble;
        set => RaiseAndSetIfChanged(ref _lintelParametersDouble, value);
    }
    
    public ObservableCollection<ParamViewModel> LintelParametersInteger {
        get => _lintelParametersInteger;
        set => RaiseAndSetIfChanged(ref _lintelParametersInteger, value);
    }
    
    public ObservableCollection<ParamViewModel> OpeningParameters {
        get => _openingParameters;
        set => RaiseAndSetIfChanged(ref _openingParameters, value);
    }
    
    public ParamViewModel SelectedLintelWidthParam {
        get => _selectedLintelWidthParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelWidthParam, value);
    }
    
    public ParamViewModel SelectedLintelThicknessParam {
        get => _selectedLintelThicknessParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelThicknessParam, value);
    }
    
    public ParamViewModel SelectedLintelRightOffsetParam {
        get => _selectedLintelRightOffsetParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelRightOffsetParam, value);
    }
    
    public ParamViewModel SelectedLintelLeftOffsetParam {
        get => _selectedLintelLeftOffsetParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelLeftOffsetParam, value);
    }
    
    public ParamViewModel SelectedLintelRightCornerParam {
        get => _selectedLintelRightCornerParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelRightCornerParam, value);
    }
    
    public ParamViewModel SelectedLintelLeftCornerParam {
        get => _selectedLintelLeftCornerParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelLeftCornerParam, value);
    }
    
    public ParamViewModel SelectedLintelRightWeldingParam {
        get => _selectedLintelRightWeldingParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelRightWeldingParam, value);
    }
    
    public ParamViewModel SelectedLintelLeftWeldingParam {
        get => _selectedLintelLeftWeldingParam;
        set => RaiseAndSetIfChanged(ref _selectedLintelLeftWeldingParam, value);
    }
    
    public ObservableCollection<FamilyViewModel> OpeningFamilyViewModels {
        get => _openingFamilyViewModels;
        set => RaiseAndSetIfChanged(ref _openingFamilyViewModels, value);
    }
    
    public ObservableCollection<FamilyViewModel> SelectedOpeningFamilyViewModels {
        get => _selectedOpeningFamilyViewModels;
        set => RaiseAndSetIfChanged(ref _selectedOpeningFamilyViewModels, value);
    }
    
    public ParamViewModel SelectedOpeningHeightParam {
        get => _selectedOpeningHeightParam;
        set => RaiseAndSetIfChanged(ref _selectedOpeningHeightParam, value);
    }
    
    public ParamViewModel SelectedOpeningWidthParam {
        get => _selectedOpeningWidthParam;
        set => RaiseAndSetIfChanged(ref _selectedOpeningWidthParam, value);
    }
    
    public IEnumerable<RevitFamily> GetSelectedFamilies() {
        var lintelFamily = _revitRepository.LintelFamilies.FirstOrDefault(
            family => family.Family == SelectedLintelFamilyViewModel?.Family);
        if(lintelFamily is null) {
            return [];
        }
        var names = SelectedOpeningFamilyViewModels.Select(family => family.Name).ToHashSet();
        return new[] { lintelFamily }.Concat(_revitRepository.OpeningFamilies
            .Where(family => names.Contains(family.Family.Name)));
    }

    public LintelFamilySettings GetLintelSettings() {
        return new LintelFamilySettings {
            LintelFamily = SelectedLintelFamilyViewModel?.Name,
            LintelWidth = SelectedLintelWidthParam?.RevitParam?.Name,
            LintelThickness = SelectedLintelThicknessParam?.RevitParam?.Name,
            LintelRightOffset = SelectedLintelRightOffsetParam?.RevitParam?.Name,
            LintelLeftOffset = SelectedLintelLeftOffsetParam?.RevitParam?.Name,
            LintelRightCorner = SelectedLintelRightCornerParam?.RevitParam?.Name,
            LintelLeftCorner = SelectedLintelLeftCornerParam?.RevitParam?.Name,
            LintelRightWelding = SelectedLintelRightWeldingParam?.RevitParam?.Name,
            LintelLeftWelding = SelectedLintelLeftWeldingParam?.RevitParam?.Name
        };
    }

    public OpeningFamilySettings GetOpeningSettings() {
        return new OpeningFamilySettings {
            FamilyNames = SelectedOpeningFamilyViewModels.Select(family => family.Name).ToArray(),
            LintelOpeningHeight = SelectedOpeningHeightParam?.RevitParam?.Name,
            LintelOpeningWidth = SelectedOpeningWidthParam?.RevitParam?.Name
        };
    }

    public string GetErrorText() {
        if(SelectedLintelFamilyViewModel?.Family is null) {
            return _localizationService.GetLocalizedString("Validation.SelectLintelFamily");
        }
        if(SelectedOpeningFamilyViewModels.Count == 0) {
            return _localizationService.GetLocalizedString("Validation.SelectOpeningFamilies");
        }
        if(SelectedOpeningFamilyViewModels.Any(family => family.Family is null)) {
            return _localizationService.GetLocalizedString("Validation.MissingOpeningFamily");
        }
        var parameters = new[] {
            SelectedLintelWidthParam, SelectedLintelThicknessParam,
            SelectedLintelRightOffsetParam, SelectedLintelLeftOffsetParam,
            SelectedLintelRightCornerParam, SelectedLintelLeftCornerParam,
            SelectedLintelRightWeldingParam, SelectedLintelLeftWeldingParam,
            SelectedOpeningHeightParam, SelectedOpeningWidthParam
        };
        return parameters.Any(parameter => parameter?.RevitParam is null)
            ? _localizationService.GetLocalizedString("Validation.SelectFamilyParameters") : null;
    }

    private static FamilyViewModel CreateFamilyViewModel(RevitFamily family) {
        return new FamilyViewModel {
            Name = family.Family.Name,
            RevitFamily = family
        };
    }

    private void ApplySearchLintelFamilies() {
        FilteredLintelFamilies = new ObservableCollection<FamilyViewModel>(string.IsNullOrEmpty(LintelSearchText)
            ? LintelFamilyViewModels
            : LintelFamilyViewModels.Where(family =>
                family.Name.IndexOf(LintelSearchText, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    private void ApplySearchOpeningFamilies() {
        FilteredOpeningFamilies = new ObservableCollection<FamilyViewModel>(string.IsNullOrEmpty(OpeningSearchText)
            ? OpeningFamilyViewModels
            : OpeningFamilyViewModels.Where(family =>
                family.Name.IndexOf(OpeningSearchText, StringComparison.OrdinalIgnoreCase) >= 0));
    }

    private ObservableCollection<ParamViewModel> CreateParameters(IEnumerable<RevitParam> parameters) {
        return new ObservableCollection<ParamViewModel>(new[] {
            new ParamViewModel {
                Name = _localizationService.GetLocalizedString("FamilySettingsViewModel.ParamNoSelect")
            }
        }.Concat(parameters.Distinct().OrderBy(param => param.Name).Select(param => new ParamViewModel {
            Name = param.Name,
            RevitParam = param
        })));
    }

    private static ParamViewModel SelectParameter(IEnumerable<ParamViewModel> parameters, string name) {
        return parameters.FirstOrDefault(parameter => parameter.RevitParam?.Name == name)
               ?? parameters.First();
    }

    private void UpdateLintelParameters() {
        var parameters = SelectedLintelFamilyViewModel?.OrderedParams?.ToArray() ?? [];
        LintelParametersDouble = CreateParameters(parameters.Where(param => param.StorageType == StorageType.Double));
        LintelParametersInteger = CreateParameters(parameters.Where(param => param.StorageType == StorageType.Integer));
        _familySettings.TryGetValue(SelectedLintelFamilyViewModel?.Name ?? string.Empty, out var settings);
        settings ??= new LintelFamilySettings {
            LintelWidth = _systemPluginConfig.LintelWidthParamName,
            LintelThickness = _systemPluginConfig.LintelThicknessParamName,
            LintelRightOffset = _systemPluginConfig.LintelRightOffsetParamName,
            LintelLeftOffset = _systemPluginConfig.LintelLeftOffsetParamName,
            LintelRightCorner = _systemPluginConfig.LintelRightCornerParamName,
            LintelLeftCorner = _systemPluginConfig.LintelLeftCornerParamName,
            LintelRightWelding = _systemPluginConfig.LintelRightWeldingParamName,
            LintelLeftWelding = _systemPluginConfig.LintelLeftWeldingParamName
        };
        SelectedLintelWidthParam = SelectParameter(LintelParametersDouble, settings.LintelWidth);
        SelectedLintelThicknessParam = SelectParameter(LintelParametersDouble, settings.LintelThickness);
        SelectedLintelRightOffsetParam = SelectParameter(LintelParametersDouble, settings.LintelRightOffset);
        SelectedLintelLeftOffsetParam = SelectParameter(LintelParametersDouble, settings.LintelLeftOffset);
        SelectedLintelRightCornerParam = SelectParameter(LintelParametersInteger, settings.LintelRightCorner);
        SelectedLintelLeftCornerParam = SelectParameter(LintelParametersInteger, settings.LintelLeftCorner);
        SelectedLintelRightWeldingParam = SelectParameter(LintelParametersInteger, settings.LintelRightWelding);
        SelectedLintelLeftWeldingParam = SelectParameter(LintelParametersInteger, settings.LintelLeftWelding);
    }

    private void UpdateOpeningParameters(string heightName, string widthName) {
        var families = SelectedOpeningFamilyViewModels.ToArray();
        var parameters = families.Length == 0 ? [] : families[0].OrderedParams
            .Where(param => param.StorageType == StorageType.Double)
            .Where(param => families.All(family => family.OrderedParams.Any(other => other.Name == param.Name
                && other.StorageType == StorageType.Double)));
        OpeningParameters = CreateParameters(parameters);
        SelectedOpeningHeightParam = SelectParameter(OpeningParameters, heightName);
        SelectedOpeningWidthParam = SelectParameter(OpeningParameters, widthName);
    }

    private void OnOpeningFamilyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName != nameof(FamilyViewModel.IsChecked) || sender is not FamilyViewModel family) {
            return;
        }
        if(family.IsChecked) {
            SelectedOpeningFamilyViewModels.Add(family);
        } else {
            SelectedOpeningFamilyViewModels.Remove(family);
        }
        UpdateOpeningParameters(SelectedOpeningHeightParam?.RevitParam?.Name,
            SelectedOpeningWidthParam?.RevitParam?.Name);
        SelectionChanged?.Invoke();
    }
}
