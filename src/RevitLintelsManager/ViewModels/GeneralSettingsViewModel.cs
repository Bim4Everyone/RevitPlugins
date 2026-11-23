using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Settings;
using RevitLintelsManager.Services;

namespace RevitLintelsManager.ViewModels;

internal class GeneralSettingsViewModel : BaseViewModel {
    public event Action<WallTypeViewModel> StructureWallsChanged;

    private readonly ILocalizationService _localizationService;
    private readonly IParamService _paramService;
    private readonly FamilySettingsViewModel _familySettings;
    private ObservableCollection<ParamViewModel> _fixParameters;
    private ParamViewModel _selectedFixParameter;
    private ItemViewModel _selectedPhase;
    private string _minimalHeightText;
    private ObservableCollection<WallTypeViewModel> _filteredStructureWalls;
    private string _wallSearchText;
    private string _fixParameterName;

    public GeneralSettingsViewModel(RevitRepository repository, IParamService paramService,
        ILocalizationService localizationService, FamilySettingsViewModel familySettings,
        LintelManagerSettings settings, ISettingsCommandFactory commandFactory) {
        _localizationService = localizationService;
        _paramService = paramService;
        _familySettings = familySettings;
        SearchStructureWallsCommand = commandFactory.Create(ApplySearchStructureWalls);
        _fixParameterName = settings.LintelFixParamName;
        Phases = new ObservableCollection<ItemViewModel>(repository.PhaseIds.Select(id =>
            new ItemViewModel { Name = repository.Document.GetElement(id).Name }));
        SelectedPhase = Phases.FirstOrDefault(phase => phase.Name == settings.PhaseName);
        MinimalHeightText = settings.MinimalHeightAboveOpeningMm.ToString(CultureInfo.CurrentCulture);
        var selectedNames = settings.GetReinforcedConcreteTypes().Where(type => type?.IsWallType() == true)
            .Select(type => type.TypeName).ToHashSet();
        var walls = repository.RevitWalls.Select(wall => new WallTypeViewModel {
            Name = wall.WallType.Name,
            WallType = wall.WallType,
            IsChecked = selectedNames.Contains(wall.WallType.Name)
        }).ToList();
        foreach(string name in selectedNames.Except(walls.Select(wall => wall.Name))) {
            walls.Add(new WallTypeViewModel { Name = name, IsChecked = true });
        }
        StructureWalls = new ObservableCollection<WallTypeViewModel>(walls
            .OrderByDescending(wall => wall.IsChecked).ThenBy(wall => wall.Name));
        foreach(var wall in StructureWalls) {
            wall.PropertyChanged += OnWallChanged;
        }
        ApplySearchStructureWalls();
        RefreshFixParameters();
        _familySettings.SelectionChanged += RefreshFixParameters;
    }

    public ObservableCollection<ItemViewModel> Phases { get; }
    public ObservableCollection<WallTypeViewModel> StructureWalls { get; }
    public ICommand SearchStructureWallsCommand { get; }

    public ObservableCollection<WallTypeViewModel> FilteredStructureWalls {
        get => _filteredStructureWalls;
        private set => RaiseAndSetIfChanged(ref _filteredStructureWalls, value);
    }

    public ObservableCollection<ParamViewModel> FixParameters {
        get => _fixParameters;
        private set => RaiseAndSetIfChanged(ref _fixParameters, value);
    }

    public ParamViewModel SelectedFixParameter {
        get => _selectedFixParameter;
        set {
            RaiseAndSetIfChanged(ref _selectedFixParameter, value);
            if(value?.RevitParam is not null) {
                _fixParameterName = value.RevitParam.Name;
            }
        }
    }

    public ItemViewModel SelectedPhase {
        get => _selectedPhase;
        set => RaiseAndSetIfChanged(ref _selectedPhase, value);
    }

    public string MinimalHeightText {
        get => _minimalHeightText;
        set => RaiseAndSetIfChanged(ref _minimalHeightText, value);
    }

    public string WallSearchText {
        get => _wallSearchText;
        set => RaiseAndSetIfChanged(ref _wallSearchText, value);
    }

    public string[] GetStructureWallNames() {
        return StructureWalls.Where(wall => wall.IsChecked).Select(wall => wall.Name).ToArray();
    }

    public double GetMinimalHeight() {
        return double.Parse(MinimalHeightText, NumberStyles.Float, CultureInfo.CurrentCulture);
    }

    public string GetErrorText() {
        if(!ParamService.IsYesNo(SelectedFixParameter?.RevitParam)) {
            return _localizationService.GetLocalizedString("Validation.SelectFixParameter");
        }
        if(SelectedPhase is null) {
            return _localizationService.GetLocalizedString("Validation.SelectPhase");
        }
        if(!double.TryParse(MinimalHeightText, NumberStyles.Float, CultureInfo.CurrentCulture, out double height)
            || double.IsNaN(height) || double.IsInfinity(height) || height < 0) {
            return _localizationService.GetLocalizedString("Validation.MinimalHeight");
        }
        return null;
    }

    private void ApplySearchStructureWalls() {
        FilteredStructureWalls = new ObservableCollection<WallTypeViewModel>(string.IsNullOrEmpty(WallSearchText)
            ? StructureWalls
            : StructureWalls.Where(wall => wall.Name?.IndexOf(WallSearchText,
                StringComparison.OrdinalIgnoreCase) >= 0));
    }

    private void RefreshFixParameters() {
        FixParameters = new ObservableCollection<ParamViewModel>(new[] {
            new ParamViewModel {
                Name = _localizationService.GetLocalizedString("FamilySettingsViewModel.ParamNoSelect")
            }
        }.Concat(_paramService.GetCommonFixParams(_familySettings.GetSelectedFamilies())
            .Select(param => new ParamViewModel { Name = param.Name, RevitParam = param })));
        SelectedFixParameter = FixParameters.FirstOrDefault(param => param.RevitParam?.Name == _fixParameterName)
                               ?? FixParameters.First();
    }

    private void OnWallChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(WallTypeViewModel.IsChecked) && sender is WallTypeViewModel wall) {
            StructureWallsChanged?.Invoke(wall);
        }
    }
}
