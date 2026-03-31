using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitDocumenter.Models;
using RevitDocumenter.Models.Dimensions.DimensionReferences;
using RevitDocumenter.Models.Dimensions.DimensionServices;
using RevitDocumenter.Models.Mapping.MapServices;
using RevitDocumenter.Models.Mapping.ViewServices;

using Grid = Autodesk.Revit.DB.Grid;

namespace RevitDocumenter.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class RebarDimensioningViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly ViewMapService _mapService;
    private readonly DimensionBuilder _dimensionService;
    private readonly ViewPreparer _viewPreparer;
    private readonly ImageService _imageService;
    private readonly PaintSquaresByMapService _paintSquaresByMapService;
    private readonly ReferenceAnalizeService _referenceAnalizeService;

    private readonly string _defFamilyNamePart = "IFC_Зона_Доп.Арм";
    private readonly string _defSelectedDimensionTypeName = "я_Основной_Плагин_2.5 мм";
    private readonly List<string> _defVerticalRefNames = ["Габарит_Ширина_1", "Габарит_Ширина_2"];
    private readonly List<string> _defHorizontalRefNames = ["Габарит_Длина_1", "Габарит_Длина_2"];

    private string _errorText;
    private string _familyNamePart;
    private DimensionType _selectedDimensionType;
    private List<DimensionType> _dimensionTypes;
    private bool _placeDimensionsAccurately;
    private bool _createMarkedImage;

    private List<Grid> _grids = [];

    // Шаг создания сетки при генерации карты, если пользователь запросил точное размещение размеров
    private readonly double _mappingStepInMm = 200.0;
    // Цвет якорных линий, которые помогают применяются при создании карты и точном размещении размеров
    private readonly Color _colorForAnchorLines = new(255, 0, 255);
    // Вес якорных линий, которые помогают применяются при создании карты и точном размещении размеров
    private readonly int _weightForAnchorLines = 1;


    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public RebarDimensioningViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        ViewMapService mapService,
        DimensionBuilder dimensionService,
        ViewPreparer viewPreparer,
        ImageService imageService,
        PaintSquaresByMapService paintSquaresByMapService,
        ReferenceAnalizeService referenceAnalizeService) {

        _pluginConfig = pluginConfig.ThrowIfNull();
        _revitRepository = revitRepository.ThrowIfNull();
        _localizationService = localizationService.ThrowIfNull();
        _mapService = mapService.ThrowIfNull();
        _dimensionService = dimensionService.ThrowIfNull();
        _viewPreparer = viewPreparer.ThrowIfNull();
        _imageService = imageService.ThrowIfNull();
        _paintSquaresByMapService = paintSquaresByMapService.ThrowIfNull();
        _referenceAnalizeService = referenceAnalizeService.ThrowIfNull();

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

    public bool PlaceDimensionsAccurately {
        get => _placeDimensionsAccurately;
        set => RaiseAndSetIfChanged(ref _placeDimensionsAccurately, value);
    }

    public bool CreateMarkedImage {
        get => _createMarkedImage;
        set => RaiseAndSetIfChanged(ref _createMarkedImage, value);
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
            _localizationService.GetLocalizedString("RebarDimensioningWindow.Title"));
        mainTransaction.Start();
        MapInfo mapInfo = null;

        if(_placeDimensionsAccurately) {
            var viewPreparerOption = new ViewPreparerOption() {
                MappingStepInFeet = UnitUtilsHelper.ConvertToInternalValue(_mappingStepInMm),
                ColorForAnchorLines = _colorForAnchorLines,
                WeightForAnchorLines = _weightForAnchorLines,
            };

            // Подготавливаем вид, определяя требуемые габариты изображения, размещая якорные линии
            // Якорные линии - линии в модели определенного цвета на виде, по которым будет подрезаться и потом
            // масштабироваться изображение для синхронизации с размерами в Revit. Подрезка и масштабирование требуется
            // в связи с тем, что когда Revit экспортирует изображение, захватываются различные элементы на виде,
            // которые расширяют первоначальную зону подрезки вида, и тогда нельзя связать точку на виде Revit и изображении
            var exportOption = _viewPreparer.Prepare(viewPreparerOption);
            // Производим экспорт вида в качестве изображения, его подрезку и масштабирование
            string imagePath = _imageService.Export(exportOption);
            // Удаляем созданные на виде якорные линии
            _revitRepository.DeleteElementsById(exportOption.AnchorLineIds);

            // Создаем бинарную карту - двумерный массив сгруппированных пикселей с информацией есть ли в группе
            // темные пиксели (т.е. квадрат занят элементами на виде) или они все белые (квадрат свободен и на нем
            // можно размещать размер или другую аннотацию)
            mapInfo = _mapService.CreateMap(imagePath, exportOption);
        }

        // Получаем арматурные элементы на виде с учетом фильтрации по именам семейств, а также их опорные плоскости
        var rebars = _revitRepository.GetRebarElements(
            FamilyNamePart,
            ReferenceNamesVM.GetVertReferenceNames(),
            ReferenceNamesVM.GetHorizReferenceNames(),
            _referenceAnalizeService);
        // Создаем размеры по найденным арматурным элементам (от их опорных плоскостей до осей)
        _dimensionService.Create(rebars, _grids, _selectedDimensionType, mapInfo);

        // Если пользователь запросил изображение для проверки, то создаем его на основе карты (после размещения размеров)
        if(_placeDimensionsAccurately && _createMarkedImage && mapInfo != null) {
            string markedImagePath = _paintSquaresByMapService.MarkWhiteSquaresOnImage(mapInfo, "_marked_after_dims");
            _imageService.OpenImage(markedImagePath);
        }

        // Удаляем первоначальное выгруженное проанализированное изображение (без окраса), если его создавали
        if(_placeDimensionsAccurately && mapInfo != null) {
            _imageService.Delete(mapInfo.ImagePath);
        }

        mainTransaction.Commit();
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
            ErrorText = _localizationService.GetLocalizedString("RebarDimensioningWindow.ViewHasNotGrids");
            return false;
        }
        if(string.IsNullOrEmpty(FamilyNamePart)) {
            ErrorText = _localizationService.GetLocalizedString("RebarDimensioningWindow.WriteFamilyNamePart");
            return false;
        }
        if(SelectedDimensionType is null) {
            ErrorText = _localizationService.GetLocalizedString("RebarDimensioningWindow.DimensionTypeIsNotSelected");
            return false;
        }
        if(ReferenceNamesVM.VerticalRefNames.Count == 0 && ReferenceNamesVM.HorizontalRefNames.Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("RebarDimensioningWindow.WriteReferenceNames");
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

        PlaceDimensionsAccurately = setting?.PlaceDimensionsAccurately ?? true;
        CreateMarkedImage = setting?.CreateMarkedImage ?? false;
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

        setting.PlaceDimensionsAccurately = PlaceDimensionsAccurately;
        setting.CreateMarkedImage = CreateMarkedImage;

        _pluginConfig.SaveProjectConfig();
    }
}
