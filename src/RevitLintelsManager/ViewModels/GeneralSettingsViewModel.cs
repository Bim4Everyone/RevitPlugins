using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.ViewModels;

internal class GeneralSettingsViewModel : BaseViewModel {
    public event Action<string> StructureWallSelectionChanged;

    private readonly ILocalizationService _localizationService;
    private readonly IParamService _paramService;
    private readonly FamilySettingsViewModel _familySettings;
    private ObservableCollection<ParamViewModel> _fixParameters;
    private ParamViewModel _selectedFixParameter;
    private ItemViewModel _selectedPhase;
    private string _minimalHeightText;
    private string _minimalBearingText;
    private string _fixParameterName;

    /// <summary>Загружает общие настройки и оба перечня типов для исходного проекта.</summary>
    public GeneralSettingsViewModel(RevitRepository repository, IParamService paramService,
        ILocalizationService localizationService, FamilySettingsViewModel familySettings,
        LintelManagerSettings settings, SystemPluginConfig systemConfig) {
        _localizationService = localizationService;
        _paramService = paramService;
        _familySettings = familySettings;
        _fixParameterName = settings.LintelFixParamName;
        Phases = new ObservableCollection<ItemViewModel>(repository.PhaseIds.Select(id =>
            new ItemViewModel { Name = repository.Document.GetElement(id).Name }));
        SelectedPhase = Phases.FirstOrDefault(phase => phase.Name == settings.PhaseName);
        MinimalHeightText = settings.MinimalHeightAboveOpeningMm.ToString(CultureInfo.CurrentCulture);
        double minimalBearing = settings.MinimalBearingMm ?? systemConfig.DefaultMinimalBearingMm;
        _minimalBearingText = minimalBearing.ToString("R", CultureInfo.CurrentCulture);
        var catalog = repository.GetStructureTypes().ToLookup(type => type.Settings.GetKey());
        ReinforcedConcreteTypes = new StructureTypeListViewModel(settings.GetReinforcedConcreteTypes(),
            systemConfig.ReinforcedConcreteCategories, catalog, localizationService);
        SteelTypes = new StructureTypeListViewModel(settings.SteelTypes ?? [], systemConfig.SteelCategories,
            catalog, localizationService);
        ReinforcedConcreteTypes.SelectionChanged += OnConcreteTypeChanged;
        RefreshFixParameters();
        _familySettings.SelectionChanged += RefreshFixParameters;
    }

    public ObservableCollection<ItemViewModel> Phases { get; }
    public StructureTypeListViewModel ReinforcedConcreteTypes { get; }
    public StructureTypeListViewModel SteelTypes { get; }
    public string MinimalBearingError =>
        double.TryParse(MinimalBearingText, NumberStyles.Float, CultureInfo.CurrentCulture, out double bearing)
        && LintelManagerConfigValidator.IsValidMinimalBearing(bearing)
            ? null
            : _localizationService.GetLocalizedString("Validation.MinimalBearing");

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

    public string MinimalBearingText {
        get => _minimalBearingText;
        set {
            if(_minimalBearingText == value) {
                return;
            }
            RaiseAndSetIfChanged(ref _minimalBearingText, value);
            OnPropertyChanged(nameof(MinimalBearingError));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>Возвращает имена разрешённых ЖБ стен для редактора назначений.</summary>
    public IEnumerable<string> GetStructureWallNames() {
        return ReinforcedConcreteTypes.GetSelectedTypes().Where(type =>
                type.Resolution == StructureTypeResolution.Resolved && type.Settings.IsWallType())
            .Select(type => type.Settings.TypeName);
    }

    /// <summary>Возвращает проверенное значение высоты кладки в миллиметрах.</summary>
    public double GetMinimalHeight() {
        return double.Parse(MinimalHeightText, NumberStyles.Float, CultureInfo.CurrentCulture);
    }

    /// <summary>Подготавливает копию общих настроек с обоими перечнями для сохранения.</summary>
    public LintelManagerSettings BuildSettings(LintelManagerSettings source) {
        double bearing = double.Parse(MinimalBearingText, NumberStyles.Float, CultureInfo.CurrentCulture);
        return source.WithGeneralSettings(bearing,
            ReinforcedConcreteTypes.GetSelectedTypes().Select(type => type.Settings),
            SteelTypes.GetSelectedTypes().Select(type => type.Settings));
    }

    /// <summary>Возвращает первую ошибку общих настроек для окна и команды сохранения.</summary>
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
        return MinimalBearingError;
    }

    // Подбирает параметры фиксации, общие для выбранных семейств.
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

    // Синхронизирует назначения правил при явной отметке разрешённой стены.
    private void OnConcreteTypeChanged(StructureTypeViewModel type) {
        if(type.IsAvailable && type.Selection.Settings.IsWallType()) {
            StructureWallSelectionChanged?.Invoke(type.IsChecked ? type.Selection.Settings.TypeName : null);
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
