using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models;
using RevitSetCoordParams.Models.Enums;
using RevitSetCoordParams.Models.Interfaces;
using RevitSetCoordParams.Models.Services;
using RevitSetCoordParams.Models.Settings;


namespace RevitSetCoordParams.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private SetCoordParamsSettings _setCoordParamsSettings;

    private readonly IParamAvailabilityService _paramAvailabilityService;
    private readonly ParamFactory _paramFactory;
    private readonly ProvidersFactory _providersFactory;

    private ObservableCollection<RangeElementsViewModel> _rangeElements;
    private RangeElementsViewModel _selectedRangeElements;
    private ObservableCollection<SourceFileViewModel> _sourceFiles;
    private SourceFileViewModel _selectedSourceFile;
    private ObservableCollection<TypeModelViewModel> _typeModels;
    private TypeModelViewModel _selectedTypeModel;
    private ObservableCollection<PositionViewModel> _positions;
    private PositionViewModel _selectedPosition;
    private ObservableCollection<ParamViewModel> _params;
    private ObservableCollection<ParamViewModel> _selectedParams;
    private ObservableCollection<CategoryViewModel> _categories;
    private ObservableCollection<CategoryViewModel> _selectedCategories;
    private bool _search;
    private bool _hasCategoryWarning;
    private bool _hasParamWarning;
    private string _maxDiameterSearchSphereMm;
    private string _stepDiameterSearchSphereMm;
    private string _errorText;
    private string _saveProperty;

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _paramAvailabilityService = new ParamAvailabilityService(_revitRepository.Document);
        _paramFactory = new ParamFactory(_paramAvailabilityService);
        _providersFactory = new ProvidersFactory();


        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public ObservableCollection<RangeElementsViewModel> RangeElements {
        get => _rangeElements;
        set => RaiseAndSetIfChanged(ref _rangeElements, value);
    }
    public RangeElementsViewModel SelectedRangeElements {
        get => _selectedRangeElements;
        set => RaiseAndSetIfChanged(ref _selectedRangeElements, value);
    }
    public ObservableCollection<SourceFileViewModel> SourceFiles {
        get => _sourceFiles;
        set => RaiseAndSetIfChanged(ref _sourceFiles, value);
    }
    public SourceFileViewModel SelectedSourceFile {
        get => _selectedSourceFile;
        set => RaiseAndSetIfChanged(ref _selectedSourceFile, value);
    }
    public ObservableCollection<TypeModelViewModel> TypeModels {
        get => _typeModels;
        set => RaiseAndSetIfChanged(ref _typeModels, value);
    }
    public TypeModelViewModel SelectedTypeModel {
        get => _selectedTypeModel;
        set => RaiseAndSetIfChanged(ref _selectedTypeModel, value);
    }
    public ObservableCollection<PositionViewModel> Positions {
        get => _positions;
        set => RaiseAndSetIfChanged(ref _positions, value);
    }
    public PositionViewModel SelectedPosition {
        get => _selectedPosition;
        set => RaiseAndSetIfChanged(ref _selectedPosition, value);
    }
    public ObservableCollection<ParamViewModel> Params {
        get => _params;
        set => RaiseAndSetIfChanged(ref _params, value);
    }
    public ObservableCollection<ParamViewModel> SelectedParams {
        get => _selectedParams;
        set => RaiseAndSetIfChanged(ref _selectedParams, value);
    }
    public ObservableCollection<CategoryViewModel> Categories {
        get => _categories;
        set => RaiseAndSetIfChanged(ref _categories, value);
    }
    public ObservableCollection<CategoryViewModel> SelectedCategories {
        get => _selectedCategories;
        set => RaiseAndSetIfChanged(ref _selectedCategories, value);
    }
    public bool Search {
        get => _search;
        set => RaiseAndSetIfChanged(ref _search, value);
    }
    public bool HasParamWarning {
        get => _hasParamWarning;
        set => RaiseAndSetIfChanged(ref _hasParamWarning, value);
    }
    public bool HasCategoryWarning {
        get => _hasCategoryWarning;
        set => RaiseAndSetIfChanged(ref _hasCategoryWarning, value);
    }
    public string MaxDiameterSearchSphereMm {
        get => _maxDiameterSearchSphereMm;
        set => RaiseAndSetIfChanged(ref _maxDiameterSearchSphereMm, value);
    }
    public string StepDiameterSearchSphereMm {
        get => _stepDiameterSearchSphereMm;
        set => RaiseAndSetIfChanged(ref _stepDiameterSearchSphereMm, value);
    }
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }
    public string SaveProperty {
        get => _saveProperty;
        set => RaiseAndSetIfChanged(ref _saveProperty, value);
    }

    // Метод получения коллекции ParamViewModel для Params
    private IEnumerable<ParamViewModel> GetParamViewModels() {
        return _setCoordParamsSettings.ParamMaps
            .Select(paramMap => new ParamViewModel {
                ParamMap = paramMap,
                Description = _localizationService.GetLocalizedString($"MainViewModel.{paramMap.LocalizationKey}Description"),
                DetailDescription = _localizationService.GetLocalizedString($"MainViewModel.{paramMap.LocalizationKey}DetailDescription"),
                SourceParamName = paramMap.SourceParam?.Name ?? string.Empty,
                TargetParamName = paramMap.TargetParam?.Name ?? string.Empty,
                IsChecked = paramMap.IsChecked,
                IsPair = paramMap.IsPair
            });
    }

    // Метод получения коллекции CategoryViewModel для Categories
    private IEnumerable<CategoryViewModel> GetCategoryViewModels() {
        return _setCoordParamsSettings.RevitCategories
            .Select(revitCategory => new {
                RevitCategory = revitCategory,
                Category = Category.GetCategory(_revitRepository.Document, revitCategory.BuiltInCategory)
            })
            .Where(x => x.Category != null)
            .Select(x => new CategoryViewModel(x.RevitCategory, x.Category) {
                IsChecked = x.RevitCategory.IsChecked
            })
            .OrderBy(x => x.CategoryName ?? string.Empty);
    }

    // Метод обновления предупреждений в категориях
    private void UpdateCategoryWarnings() {
        var selectedParams = SelectedParams
            .Select(paramViewModel => paramViewModel.ParamMap)
            .ToList();
        foreach(var category in Categories) {
            category.UpdateWarning(_paramAvailabilityService, _localizationService, selectedParams);
        }
        HasCategoryWarning = Categories.Any(category => category.HasWarning);
    }

    // Метод обновления предупреждений в параметрах
    private void UpdateParamWarnings() {
        foreach(var param in Params) {
            param.UpdateWarning(_paramAvailabilityService, _localizationService);
        }
        HasParamWarning = Params.Any(param => param.HasWarning);
    }

    // Метод обновления параметров в ParamMap
    private bool TryUpdateParam(ParamViewModel viewModel, string paramName, bool isSource) {
        if(viewModel.HasWarning) {
            return false;
        }
        var newParam = _paramFactory.CreateRevitParam(_revitRepository.Document, paramName);
        if(isSource) {
            viewModel.ParamMap.SourceParam = newParam;
        } else {
            viewModel.ParamMap.TargetParam = newParam;
        }
        return true;
    }

    // Метод удаления и добавления элементов коллекции
    private void HandleCheckedChangedParam(ParamViewModel paramViewModel) {
        if(paramViewModel.IsChecked) {
            SelectedParams.Add(paramViewModel);
            paramViewModel.ParamMap.IsChecked = true;
        } else {
            SelectedParams.Remove(paramViewModel);
            paramViewModel.ParamMap.IsChecked = false;
        }
        UpdateParamWarnings();
    }

    // Метод подписанный на событие изменения выделенных параметров
    private void OnParamChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not ParamViewModel vm) {
            return;
        }
        bool needUpdateCategoryWarnings = false;

        switch(e.PropertyName) {
            case nameof(ParamViewModel.SourceParamName):
                UpdateParamWarnings();
                needUpdateCategoryWarnings = TryUpdateParam(vm, vm.SourceParamName, isSource: true);
                break;

            case nameof(ParamViewModel.TargetParamName):
                UpdateParamWarnings();
                needUpdateCategoryWarnings = TryUpdateParam(vm, vm.TargetParamName, isSource: false);
                break;

            case nameof(ParamViewModel.IsChecked):
                HandleCheckedChangedParam(vm);
                needUpdateCategoryWarnings = true;
                break;
        }
        if(needUpdateCategoryWarnings) {
            UpdateCategoryWarnings();
        }
    }

    // Метод подписанный на событие изменения выделенных категорий
    private void OnCategoryChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not CategoryViewModel categoryViewModel) {
            return;
        }
        if(categoryViewModel.IsChecked) {
            SelectedCategories.Add(categoryViewModel);
            categoryViewModel.RevitCategory.IsChecked = true;
        } else {
            SelectedCategories.Remove(categoryViewModel);
            categoryViewModel.RevitCategory.IsChecked = false;
        }
    }

    private IEnumerable<RangeElementsViewModel> GetRangeElementsViewModels() {
        var categories = Categories.Select(c => c.RevitCategory).ToList();

        var providers = new[] {
        new { Type = ProviderType.AllElementsProvider, Key = "RangeElementsViewModel.AllElements" },
        new { Type = ProviderType.SelectedElementsProvider, Key = "RangeElementsViewModel.SelectedElements" },
        new { Type = ProviderType.CurrentViewProvider, Key = "RangeElementsViewModel.CurrentViewElements" }
        };

        var currentType = _setCoordParamsSettings.ElementsProvider.Type;

        return providers
            .Select(p => new RangeElementsViewModel {
                Name = _localizationService.GetLocalizedString(p.Key),
                ElementsProvider = (p.Type == currentType)
                    ? _setCoordParamsSettings.ElementsProvider // уже выбранный, не создаём заново
                    : _providersFactory.GetElementsProvider(_revitRepository, p.Type, categories)
            })
            .OrderByDescending(vm => vm.ElementsProvider.Type == currentType)
            .ToList();
    }

    private IEnumerable<PositionViewModel> GetPositionViewModels() {
        var providers = new[] {
        new { Type = ProviderType.CenterPositionProvider, Key = "PositionsViewModel.Center" },
        new { Type = ProviderType.BottomPositionProvider, Key = "PositionsViewModel.Bottom" }
        };

        var currentType = _setCoordParamsSettings.PositionProvider.Type;

        return providers
            .Select(p => new PositionViewModel {
                Name = _localizationService.GetLocalizedString(p.Key),
                PositionProvider = (p.Type == currentType)
                    ? _setCoordParamsSettings.PositionProvider // уже выбранный, не создаём заново
                    : _providersFactory.GetPositionProvider(_revitRepository, p.Type)
            })
            .OrderByDescending(vm => vm.PositionProvider.Type == currentType)
            .ToList();
    }

    private void LoadView() {
        LoadConfig();
        Params = new ObservableCollection<ParamViewModel>(GetParamViewModels());
        SelectedParams = new ObservableCollection<ParamViewModel>(Params);
        Categories = new ObservableCollection<CategoryViewModel>(GetCategoryViewModels());
        UpdateParamWarnings();
        UpdateCategoryWarnings();
        RangeElements = new ObservableCollection<RangeElementsViewModel>(GetRangeElementsViewModels());
        SelectedRangeElements = RangeElements.First();
        Positions = new ObservableCollection<PositionViewModel>(GetPositionViewModels());
        SelectedPosition = Positions.First();


        // Подписка на события в ParamViewModel
        foreach(var param in Params) {
            param.PropertyChanged += OnParamChanged;
        }

        // Подписка на события в CategoryViewModel
        foreach(var category in Categories) {
            category.PropertyChanged += OnCategoryChanged;
        }

        Search = _setCoordParamsSettings.Search;
        MaxDiameterSearchSphereMm = Convert.ToString(_setCoordParamsSettings.MaxDiameterSearchSphereMm);
        StepDiameterSearchSphereMm = Convert.ToString(_setCoordParamsSettings.StepDiameterSearchSphereMm);

    }
    private void AcceptView() {
        SaveConfig();
    }

    private bool CanAcceptView() {
        if(HasParamWarning) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.WrongParamName");
            return false;
        }
        ErrorText = null;
        return true;
    }

    private void LoadConfig() {
        var projectConfig = _pluginConfig.GetSettings(_revitRepository.Document);
        ConfigSettings configSettings;
        if(projectConfig == null) {
            configSettings = new ConfigSettings();
            configSettings.ApplyDefaultValues();
        } else {
            configSettings = projectConfig.DefaultSettings;
        }
        _setCoordParamsSettings = new SetCoordParamsSettings(_revitRepository, configSettings);
        _setCoordParamsSettings.LoadConfigSettings();
    }


    private void SaveConfig() {

        _setCoordParamsSettings.ParamMaps = Params.Select(paramVM => paramVM.ParamMap).ToList();
        _setCoordParamsSettings.RevitCategories = Categories.Select(catVM => catVM.RevitCategory).ToList();
        _setCoordParamsSettings.ElementsProvider = SelectedRangeElements.ElementsProvider;
        _setCoordParamsSettings.PositionProvider = SelectedPosition.PositionProvider;
        _setCoordParamsSettings.Search = Search;
        _setCoordParamsSettings.MaxDiameterSearchSphereMm = Convert.ToDouble(MaxDiameterSearchSphereMm);
        _setCoordParamsSettings.StepDiameterSearchSphereMm = Convert.ToDouble(StepDiameterSearchSphereMm);
        _setCoordParamsSettings.UpdateConfigSettings();

        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
            ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.DefaultSettings = _setCoordParamsSettings.ConfigSettings;
        _pluginConfig.SaveProjectConfig();
    }
}
