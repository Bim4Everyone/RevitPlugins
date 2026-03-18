using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitDocumenter.Models;
using RevitDocumenter.Models.Comparision;
using RevitDocumenter.Models.DimensionLine;
using RevitDocumenter.Models.MapServices;

using Grid = Autodesk.Revit.DB.Grid;

namespace RevitDocumenter.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IComparisonService _comparisonService;
    private readonly IDimensionLineService _dimensionLineService;
    private readonly ViewMapService _mapService;
    private readonly DimensionCreator _dimensionCreator;
    private readonly ValueGuard _guard;

    private string _errorText;
    private string _familyNamePart;
    private DimensionType _selectedDimensionType;
    private List<DimensionType> _dimensionTypes;
    private List<Grid> _grids = [];

    private readonly string _defFamilyNamePart = "IFC_Зона_Доп.Арм";
    private readonly string _defSelectedDimensionTypeName = "я_Основной_Плагин_2.5 мм";
    private readonly List<string> _defVerticalRefNames = ["Габарит_Ширина_1", "Габарит_Ширина_2"];
    private readonly List<string> _defHorizontalRefNames = ["Габарит_Длина_1", "Габарит_Длина_2"];

    private readonly bool _createMarkedImage = true;

    private readonly double _mappingStepInMm = 200.0;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IComparisonService comparisonService,
        IDimensionLineService dimensionLineService,
        ViewMapService mapService,
        DimensionCreator dimensionCreator,
        ValueGuard guard) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _comparisonService = comparisonService;
        _dimensionLineService = dimensionLineService;
        _mapService = mapService;
        _dimensionCreator = dimensionCreator;
        _guard = guard;

        ReferenceNamesVM = new ReferenceNamesViewModel();

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public ReferenceNamesViewModel ReferenceNamesVM { get; }


    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public string FamilyNamePart {
        get => _familyNamePart;
        set => RaiseAndSetIfChanged(ref _familyNamePart, value);
    }

    public DimensionType SelectedDimensionType {
        get => _selectedDimensionType;
        set => RaiseAndSetIfChanged(ref _selectedDimensionType, value);
    }

    public List<DimensionType> DimensionTypes {
        get => _dimensionTypes;
        set => RaiseAndSetIfChanged(ref _dimensionTypes, value);
    }
    public List<Grid> Grids {
        get => _grids;
        set => RaiseAndSetIfChanged(ref _grids, value);
    }


    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        DimensionTypes = _revitRepository.DimensionTypes;
        Grids = _revitRepository.GetGrids();

        LoadConfig();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        SaveConfig();
        using var mainTransaction = new Transaction(
            _revitRepository.Document,
            _localizationService.GetLocalizedString("MainWindow.Title"));
        mainTransaction.Start();

        var viewPreparerOption = new ViewPreparerOption() {
            MappingStepInMm = _mappingStepInMm,
            MappingStepInFeet = UnitUtilsHelper.ConvertToInternalValue(_mappingStepInMm),
            ColorForAnchorLines = new(255, 0, 255),
            WeightForAnchorLines = 1,
        };

        var viewPreparer = new ViewPreparer(_revitRepository);
        var exportOption = viewPreparer.Prepare(viewPreparerOption);

        var imageExporter = new ImageExporter(_revitRepository.Document);
        string imagePath = imageExporter.Export(exportOption);

        _mapService.CreateMap(imagePath, exportOption);
        if(_createMarkedImage) {
            var painter = new PaintSquaresByMapService();
            painter.MarkWhiteSquaresOnImage(imagePath, _mapService.Map, exportOption.StepCountX, exportOption.StepCountY);
        }

        foreach(var rebar in _revitRepository.GetRebarElements(
            FamilyNamePart,
            ReferenceNamesVM.GetVertReferenceNames(),
            ReferenceNamesVM.GetHorizReferenceNames())) {

            // Создание вертикального размера (относительно локальных осей зоны армирования)
            CreateDimension(Grids, rebar);
            // Создание горизонтального размера (относительно локальных осей зоны армирования)
            CreateDimension(Grids, rebar, false);
        }
        mainTransaction.Commit();
    }

    private Dimension CreateDimension(List<Grid> grids, RebarElement rebar, bool isForVertical = true) {
        try {
            _guard.ThrowIfNull(grids, rebar);
        } catch(Exception) {
            return null;
        }

        var rebarReferences = isForVertical ? rebar.VerticalRefs : rebar.HorizontalRefs;
        // Нормальная ситуация, когда подходящие оси не были найдены
        if(rebarReferences.Count == 0)
            return null;

        var direction = isForVertical ? rebar.Rebar.FacingOrientation : rebar.Rebar.HandOrientation;

        IComparisonContext comparisonContext =
            new GridComparisonContext(rebarReferences, grids, direction);

        // Получаем опорные плоскости для размера
        var dimensionRefs = _comparisonService.Compare(comparisonContext);
        if(dimensionRefs is null) {
            return null;
        }

        // Получаем линию размещения размера
        var dimensionLineY = _dimensionLineService.GetDimensionLine(rebar, direction);

        // Строим размер
        var dimension = _dimensionCreator.Create(dimensionLineY, dimensionRefs, _selectedDimensionType);

        var dimensionChanger = new DimensionChanger(_revitRepository, _dimensionCreator);
        dimensionChanger.Change(dimension, _mapService, _mappingStepInMm, dimensionRefs);

        return dimension;
    }


    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    /// В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    /// В методе проверяемые свойства окна должны быть отсортированы в таком же порядке как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        if(Grids.Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ViewHasNotGrids");
            return false;
        }
        if(string.IsNullOrEmpty(FamilyNamePart)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.WriteFamilyNamePart");
            return false;
        }
        if(SelectedDimensionType is null) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.DimensionTypeIsNotSelected");
            return false;
        }
        if(ReferenceNamesVM.VerticalRefNames.Count == 0 && ReferenceNamesVM.HorizontalRefNames.Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.WriteReferenceNames");
            return false;
        }
        ErrorText = string.Empty;
        return true;
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        FamilyNamePart = setting?.FamilyNamePart ?? _defFamilyNamePart;
        SelectedDimensionType = DimensionTypes.FirstOrDefault(d =>
            d.Name.Equals(setting?.SelectedDimensionTypeName ?? _defSelectedDimensionTypeName));

        var verticalRefNames = setting?.VerticalRefNames ?? _defVerticalRefNames;
        verticalRefNames.ForEach(item => ReferenceNamesVM.VerticalRefNames.Add(new ReferenceNameViewModel(item)));

        var horizontalRefNames = setting?.HorizontalRefNames ?? _defHorizontalRefNames;
        horizontalRefNames.ForEach(item => ReferenceNamesVM.HorizontalRefNames.Add(new ReferenceNameViewModel(item)));
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.FamilyNamePart = FamilyNamePart;
        setting.SelectedDimensionTypeName = SelectedDimensionType.Name;

        setting.VerticalRefNames = [.. ReferenceNamesVM.VerticalRefNames.Select(r => r.ReferenceName)];
        setting.HorizontalRefNames = [.. ReferenceNamesVM.HorizontalRefNames.Select(r => r.ReferenceName)];

        _pluginConfig.SaveProjectConfig();
    }
}
