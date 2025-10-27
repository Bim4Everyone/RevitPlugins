using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
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
    private readonly ICategoryAvailabilityService _categoryAvailabilityService;
    private readonly IParamAvailabilityService _paramAvailabilityService;
    private readonly ProvidersFactory _providersFactory;
    private readonly ParamFactory _paramFactory;
    private SetCoordParamsSettings _setCoordParamsSettings;
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
    private bool _isCheckedAllCats;
    private bool _isNotCheckedAllCats;
    private bool _hasCategoryWarning;
    private bool _hasParamWarning;
    private string _maxDiameterSearchSphereMm;
    private string _stepDiameterSearchSphereMm;
    private string _errorText;

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _categoryAvailabilityService = new CategoryAvailabilityService(_revitRepository.Document);
        _paramAvailabilityService = new ParamAvailabilityService();
        _providersFactory = new ProvidersFactory();
        _paramFactory = new ParamFactory();

        LoadViewCommand = RelayCommand.Create(LoadView);
        CheckAllCatsCommand = RelayCommand.Create(CheckAllCategories);
        UncheckAllCatsCommand = RelayCommand.Create(UncheckAllCategories);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand CheckAllCatsCommand { get; }
    public ICommand UncheckAllCatsCommand { get; }
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
    public string MaxDiameterSearchSphereMm {
        get => _maxDiameterSearchSphereMm;
        set => RaiseAndSetIfChanged(ref _maxDiameterSearchSphereMm, value);
    }
    public string StepDiameterSearchSphereMm {
        get => _stepDiameterSearchSphereMm;
        set => RaiseAndSetIfChanged(ref _stepDiameterSearchSphereMm, value);
    }
    public bool HasParamWarning {
        get => _hasParamWarning;
        set => RaiseAndSetIfChanged(ref _hasParamWarning, value);
    }
    public bool HasCategoryWarning {
        get => _hasCategoryWarning;
        set => RaiseAndSetIfChanged(ref _hasCategoryWarning, value);
    }
    public bool IsCheckedAllCats {
        get => _isCheckedAllCats;
        set => RaiseAndSetIfChanged(ref _isCheckedAllCats, value);
    }
    public bool IsNotCheckedAllCats {
        get => _isNotCheckedAllCats;
        set => RaiseAndSetIfChanged(ref _isNotCheckedAllCats, value);
    }
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    // Метод получения коллекции TypeModelViewModel для TypeModels
    private IEnumerable<TypeModelViewModel> GetTypeModelViewModels() {
        var currentDocument = SelectedSourceFile.FileProvider.Document;
        var sourceElementsValues = _revitRepository.GetSourceElementsValues(currentDocument);
        return !sourceElementsValues.Any()
            ? []
            : sourceElementsValues
                .Select(value => new TypeModelViewModel { Name = value })
                .OrderByDescending(vm => vm.Name.Equals(_setCoordParamsSettings.TypeModel));
    }

    // Метод получения коллекции SourceFilesViewModel для SourceFiles
    private IEnumerable<SourceFileViewModel> GetSourceFilesViewModels() {
        var currentProvider = _setCoordParamsSettings.FileProvider;
        return _revitRepository.GetAllDocuments()
            .Select(document => {
                var provider = _providersFactory.GetFileProvider(_revitRepository, document);
                return new SourceFileViewModel(_localizationService, provider);
            })
            .OrderByDescending(vm => vm.FileProvider.Document.GetUniqId().Equals(currentProvider.Document.GetUniqId()));
    }

    // Метод получения коллекции PositionViewModel для Positions
    private IEnumerable<PositionViewModel> GetPositionViewModels() {
        var currentProvider = _setCoordParamsSettings.PositionProvider;
        var providers = Enum.GetValues(typeof(PositionProviderType)).Cast<PositionProviderType>();
        return providers
            .Select(provider => new PositionViewModel {
                Name = _localizationService.GetLocalizedString($"MainViewModel.{provider}"),
                PositionProvider = (provider == currentProvider.Type)
                    ? currentProvider
                    : _providersFactory.GetPositionProvider(_revitRepository, provider)
            })
            .OrderByDescending(vm => vm.PositionProvider.Type == currentProvider.Type);
    }

    // Метод получения коллекции RangeElementsViewModel для RangeElements
    private IEnumerable<RangeElementsViewModel> GetRangeElementsViewModels() {
        var currentProvider = _setCoordParamsSettings.ElementsProvider;
        var providers = Enum.GetValues(typeof(ElementsProviderType)).Cast<ElementsProviderType>();
        return providers
            .Select(provider => new RangeElementsViewModel {
                Name = _localizationService.GetLocalizedString($"MainViewModel.{provider}"),
                ElementsProvider = (provider == currentProvider.Type)
                    ? currentProvider
                    : _providersFactory.GetElementsProvider(_revitRepository, provider)
            })
            .OrderByDescending(vm => vm.ElementsProvider.Type == currentProvider.Type);
    }

    // Метод обновления предупреждений в категориях
    private void UpdateCategoryWarnings() {
        var selectedParams = SelectedParams
            .Select(paramViewModel => paramViewModel.ParamMap)
            .ToList();
        foreach(var category in Categories) {
            category.UpdateWarning(selectedParams);
        }
        HasCategoryWarning = Categories.Any(category => category.HasWarning);
    }

    // Метод обновления предупреждений в параметрах
    private void UpdateParamWarnings() {
        foreach(var param in Params) {
            param.UpdateWarning(_selectedSourceFile.FileProvider.Document, _revitRepository.Document);
        }
        HasParamWarning = Params.Any(param => param.HasWarning);
    }

    // Метод команды на выделение всех категорий
    private void CheckAllCategories() {
        foreach(var catVM in Categories) {
            catVM.IsChecked = true;
        }
    }

    // Метод команды на снятие выделения всех категорий
    private void UncheckAllCategories() {
        foreach(var catVM in Categories) {
            catVM.IsChecked = false;
        }
    }

    // Метод подписанный на событие MainViewModel
    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(SelectedSourceFile)) {
            TypeModels = new ObservableCollection<TypeModelViewModel>(GetTypeModelViewModels());
            SelectedTypeModel = TypeModels.FirstOrDefault();
            UpdateParamWarnings();
        }
    }

    // Метод подписанный на событие изменения CategoryViewModel
    private void OnCategoryChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not CategoryViewModel categoryViewModel) {
            return;
        }
        if(categoryViewModel.IsChecked) {
            if(!SelectedCategories.Contains(categoryViewModel)) {
                SelectedCategories.Add(categoryViewModel);
            }
        } else {
            SelectedCategories.Remove(categoryViewModel);
        }
    }

    // Метод подписанный на событие изменения ParamViewModel
    private void OnParamChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is not ParamViewModel vm) {
            return;
        }
        switch(e.PropertyName) {
            case nameof(ParamViewModel.SourceParamName):
                UpdateParamWarnings();
                if(!vm.HasWarning) {
                    var def = _paramAvailabilityService.GetDefinitionByName(SelectedSourceFile.FileProvider.Document, vm.SourceParamName);
                    var newParam = _paramFactory.CreateRevitParam(_revitRepository.Document, def);
                    vm.ParamMap.SourceParam = newParam;
                }
                UpdateCategoryWarnings();
                break;

            case nameof(ParamViewModel.TargetParamName):
                UpdateParamWarnings();
                if(!vm.HasWarning) {
                    var def = _paramAvailabilityService.GetDefinitionByName(_revitRepository.Document, vm.SourceParamName);
                    var newParam = _paramFactory.CreateRevitParam(_revitRepository.Document, def);
                    vm.ParamMap.TargetParam = newParam;
                }
                UpdateCategoryWarnings();
                break;

            case nameof(ParamViewModel.IsChecked):
                if(vm.IsChecked) {
                    if(!SelectedParams.Contains(vm)) {
                        SelectedParams.Add(vm);
                    }
                } else {
                    SelectedParams.Remove(vm);
                }
                UpdateParamWarnings();
                UpdateCategoryWarnings();
                break;
        }
    }

    // Метод получения коллекции CategoryViewModel для Categories
    private IEnumerable<CategoryViewModel> GetCategoryViewModels() {
        var allBuiltInCategories = RevitConstants.GetDefaultBuiltInCategories();
        var savedCategories = _setCoordParamsSettings.Categories;

        return allBuiltInCategories
        .Select(builtInCategory => {
            var category = Category.GetCategory(_revitRepository.Document, builtInCategory);
            return category == null
                ? null
                : new CategoryViewModel(_categoryAvailabilityService, _localizationService) {
                    Category = category,
                    CategoryName = category.Name,
                    IsChecked = savedCategories.Contains(builtInCategory)
                };
        })
        .Where(vm => vm != null)
        .OrderBy(vm => vm.CategoryName ?? string.Empty);
    }

    // Метод получения коллекции ParamViewModel для Params
    private IEnumerable<ParamViewModel> GetParamViewModels() {
        var defaultParamMaps = RevitConstants.GetDefaultParamMaps();
        var savedParamMaps = _setCoordParamsSettings.ParamMaps;

        var savedLookup = savedParamMaps.ToDictionary(paramMap => paramMap.Type, paramMap => paramMap);

        return defaultParamMaps.Select(defaultParamMap => {
            bool exists = savedLookup.TryGetValue(defaultParamMap.Type, out var savedParamMap);
            var paramMap = exists ? savedParamMap : defaultParamMap;
            bool isPair = paramMap.SourceParam != null;

            return new ParamViewModel(_localizationService, _paramAvailabilityService) {
                ParamMap = paramMap,
                Description = _localizationService.GetLocalizedString($"MainViewModel.{paramMap.Type}Description"),
                DetailDescription = _localizationService.GetLocalizedString($"MainViewModel.{paramMap.Type}DetailDescription"),
                SourceParamName = paramMap.SourceParam?.Name ?? string.Empty,
                TargetParamName = paramMap.TargetParam?.Name ?? string.Empty,
                IsChecked = exists,
                IsPair = isPair
            };
        });
    }

    // Метод загрузки вида
    private void LoadView() {
        LoadConfig();
        RangeElements = new ObservableCollection<RangeElementsViewModel>(GetRangeElementsViewModels());
        SelectedRangeElements = RangeElements.First();
        SourceFiles = new ObservableCollection<SourceFileViewModel>(GetSourceFilesViewModels());
        SelectedSourceFile = SourceFiles.First();
        TypeModels = new ObservableCollection<TypeModelViewModel>(GetTypeModelViewModels());
        SelectedTypeModel = TypeModels.FirstOrDefault();
        Positions = new ObservableCollection<PositionViewModel>(GetPositionViewModels());
        SelectedPosition = Positions.First();
        Params = new ObservableCollection<ParamViewModel>(GetParamViewModels());
        SelectedParams = [];
        // Подписка на события в ParamViewModel
        foreach(var param in Params) {
            param.PropertyChanged += OnParamChanged;
            if(param.IsChecked) {
                SelectedParams.Add(param);
            }
        }
        Categories = new ObservableCollection<CategoryViewModel>(GetCategoryViewModels());
        SelectedCategories = [];
        // Подписка на события в CategoryViewModel
        foreach(var category in Categories) {
            category.PropertyChanged += OnCategoryChanged;
            if(category.IsChecked) {
                SelectedCategories.Add(category);
            }
        }
        // Подписка на события для обновления TypeViewModel
        PropertyChanged += OnPropertyChanged;
        UpdateParamWarnings();
        UpdateCategoryWarnings();
        IsCheckedAllCats = SelectedCategories.All(catVM => catVM.IsChecked);
        IsNotCheckedAllCats = SelectedCategories.All(catVM => !catVM.IsChecked);
        Search = _setCoordParamsSettings.Search;
        MaxDiameterSearchSphereMm = Convert.ToString(_setCoordParamsSettings.MaxDiameterSearchSphereMm);
        StepDiameterSearchSphereMm = Convert.ToString(_setCoordParamsSettings.StepDiameterSearchSphereMm);
    }

    // Основной метод
    private void AcceptView() {
        SaveConfig();
        var processor = new SetCoordParamsProcessor(_localizationService, _revitRepository, _setCoordParamsSettings);
        processor.Run();
    }

    // Метод проверки возможности выполнения основного вида
    private bool CanAcceptView() {
        if(SelectedRangeElements != null) {
            if(SelectedRangeElements.ElementsProvider.Type == ElementsProviderType.SelectedElementsProvider
                && _revitRepository.GetSelectedElements().ToList().Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoSelection");
                return false;
            }
            if(SelectedRangeElements.ElementsProvider.Type == ElementsProviderType.CurrentViewProvider) {
                var activeView = _revitRepository.GetCurrentView();
                if(activeView.ViewType is not (ViewType.ThreeD or ViewType.Schedule or ViewType.FloorPlan)) {
                    ErrorText = _localizationService.GetLocalizedString("MainViewModel.WrongView");
                    return false;
                }
            }
        }
        if(SelectedParams != null) {
            if(SelectedParams.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoParams");
                return false;
            }
            if(SelectedParams.Count == 1 && SelectedParams.First().ParamMap.SourceParam == null) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoParamMaps");
                return false;
            }
        }
        if(SelectedTypeModel == null) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoTypeModel");
            return false;
        }
        if(SelectedCategories != null) {
            if(SelectedCategories.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainViewModel.NoCategories");
                return false;
            }
        }
        if(!double.TryParse(_maxDiameterSearchSphereMm, out double result)) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.DiameterSearchNoDouble");
            return false;
        }
        if(!double.TryParse(_stepDiameterSearchSphereMm, out double resultStep)) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.DiameterSearchNoDouble");
            return false;
        }
        if(result < 0 || resultStep < 0) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.DiameterSearchNoNegate");
            return false;
        } else if(result == 0 || resultStep == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.DiameterSearchNoZero");
            return false;
        } else if(result > 100000) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.DiameterSearchBig");
            return false;
        } else if(resultStep > 2000) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.DiameterSearchStepBig");
            return false;
        } else if(resultStep > result) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.DiameterSearchStepBigger");
            return false;
        }
        if(HasParamWarning) {
            ErrorText = _localizationService.GetLocalizedString("MainViewModel.WrongParamName");
            return false;
        }
        ErrorText = null;
        return true;
    }

    // Загрузка конфигурации пользователя
    private void LoadConfig() {
        var projectConfig = _pluginConfig.GetSettings(_revitRepository.Document);
        ConfigSettings configSettings;
        if(projectConfig == null) {
            configSettings = new ConfigSettings();
            configSettings.ApplyDefaultValues();
        } else {
            configSettings = projectConfig.ConfigSettings;
        }
        _setCoordParamsSettings = new SetCoordParamsSettings(_revitRepository, configSettings);
        _setCoordParamsSettings.LoadConfigSettings();
    }

    // Сохранение конфигурации пользователя
    private void SaveConfig() {
        _setCoordParamsSettings.ParamMaps = SelectedParams.Select(paramVM => paramVM.ParamMap).ToList();
        _setCoordParamsSettings.Categories = SelectedCategories.Select(catVM => catVM.Category.GetBuiltInCategory()).ToList();
        _setCoordParamsSettings.ElementsProvider = SelectedRangeElements.ElementsProvider;
        _setCoordParamsSettings.PositionProvider = SelectedPosition.PositionProvider;
        _setCoordParamsSettings.FileProvider = SelectedSourceFile.FileProvider;
        _setCoordParamsSettings.TypeModel = SelectedTypeModel.Name;
        _setCoordParamsSettings.Search = Search;
        _setCoordParamsSettings.MaxDiameterSearchSphereMm = Convert.ToDouble(MaxDiameterSearchSphereMm);
        _setCoordParamsSettings.StepDiameterSearchSphereMm = Convert.ToDouble(StepDiameterSearchSphereMm);
        _setCoordParamsSettings.UpdateConfigSettings();

        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
            ?? _pluginConfig.AddSettings(_revitRepository.Document);
        setting.ConfigSettings = _setCoordParamsSettings.ConfigSettings;
        _pluginConfig.SaveProjectConfig();
    }
}
