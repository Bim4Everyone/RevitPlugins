using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitDocumenter.Models;
using RevitDocumenter.Models.Comparision;
using RevitDocumenter.Models.DimensionLine;

using Grid = Autodesk.Revit.DB.Grid;
using Line = Autodesk.Revit.DB.Line;

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
        DimensionCreator dimensionCreator,
        ValueGuard guard) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _comparisonService = comparisonService;
        _dimensionLineService = dimensionLineService;
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

        //var rebar = _revitRepository.GetRebarElements(
        //    FamilyNamePart,
        //    ReferenceNamesVM.GetVertReferenceNames(),
        //    ReferenceNamesVM.GetHorizReferenceNames()
        //    ).First();
        //var dimensionLineY = _dimensionLineService.GetDimensionLine(rebar, XYZ.BasisY);


        var mapService = new MapService(_revitRepository);
        (SquareInfo[,] map, XYZ viewMinFixed, double mappingStepInMm) = mapService.GetMap();





        foreach(var rebar in _revitRepository.GetRebarElements(
            FamilyNamePart,
            ReferenceNamesVM.GetVertReferenceNames(),
            ReferenceNamesVM.GetHorizReferenceNames())) {

            // Создание вертикального размера (относительно локальных осей зоны армирования)
            CreateDimension(Grids, rebar, mapService);
            // Создание горизонтального размера (относительно локальных осей зоны армирования)
            CreateDimension(Grids, rebar, mapService, false);
        }
        mainTransaction.Commit();
    }

    private Dimension CreateDimension(
        List<Grid> grids, RebarElement rebar, MapService mapService, bool isForVertical = true) {
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


        try {
            var p1 = (dimension.Curve as Line).Origin;
            var leaderEndPosition = dimension.LeaderEndPosition;
            var textPosition = dimension.TextPosition;
            var center = GetDimensionCenterPoint(dimension);
            (var leftStroke, var rightStroke) = GetDimensionEdgePoints(dimension);

            // Горизонтально размеру нужно сделать небольшой отступ от текста
            double coefForHorizOffset = 0.1;
            var textEdgeLeftBottom = leaderEndPosition + (leaderEndPosition - center).Normalize() * coefForHorizOffset;
            var textEdgeRightBottom = textEdgeLeftBottom + (center - textEdgeLeftBottom) * 2;


            int viewScale = _revitRepository.Document.ActiveView.Scale;
            // Текст на самом деле немного отстоит от размерной линии, чтобы это скомпенсировать берем коэффициент
            double someCoefficient = 2;
            double dimTextHeight =
                _selectedDimensionType.GetParamValue<double>(BuiltInParameter.TEXT_SIZE) * viewScale * someCoefficient;

            var textEdgeLeftTop = textEdgeLeftBottom + (textPosition - center).Normalize() * dimTextHeight;
            var textEdgeRightTop = textEdgeRightBottom + (textPosition - center).Normalize() * dimTextHeight;


            XYZ vectorUp = (textPosition - center).Normalize();
            double step = mapService.RevitStep;

            int coef = 0;
            int maxStepsForSearch = 10;
            bool needRecreateDimension = false;
            for(int i = 1; i <= maxStepsForSearch; i++) {
                if(Check(mapService, textEdgeLeftBottom, textEdgeRightBottom, textEdgeLeftTop, textEdgeRightTop)) {
                    if(needRecreateDimension) {
                        _revitRepository.Document.Delete(dimension.Id);

                        dimension = _dimensionCreator.Create(
                            Line.CreateBound(textEdgeLeftBottom, textEdgeRightBottom),
                            dimensionRefs,
                            _selectedDimensionType);
                        Paint(mapService, textEdgeLeftBottom, textEdgeRightBottom, textEdgeLeftTop, textEdgeRightTop);
                    }
                    break;
                }

                coef = i % 2 == 1 ? i : -i;

                textEdgeLeftBottom += vectorUp * step * coef;
                textEdgeRightBottom += vectorUp * step * coef;
                textEdgeLeftTop += vectorUp * step * coef;
                textEdgeRightTop += vectorUp * step * coef;

                needRecreateDimension = true;
            }


            var imagePreparer = new ImagePreparer(_revitRepository);
            imagePreparer.CreateSphere(textEdgeLeftBottom);
            imagePreparer.CreateSphere(textEdgeRightBottom);
            imagePreparer.CreateSphere(textEdgeLeftTop);
            imagePreparer.CreateSphere(textEdgeRightTop);

            imagePreparer.CreateSphere(center);
            imagePreparer.CreateSphere(leftStroke);
            imagePreparer.CreateSphere(rightStroke);


        } catch(Exception) {
        }


        return dimension;
    }


    private bool Check(
        MapService mapService, XYZ textEdgeLeftBottomTemp, XYZ textEdgeRightBottomTemp,
        XYZ textEdgeLeftTopTemp, XYZ textEdgeRightTopTemp) {
        return
            mapService.IsWhiteSquare(textEdgeLeftBottomTemp)
            && mapService.IsWhiteSquare(textEdgeRightBottomTemp)
            && mapService.IsWhiteSquare(textEdgeLeftTopTemp)
            && mapService.IsWhiteSquare(textEdgeRightTopTemp);
    }

    private void Paint(
        MapService mapService, XYZ textEdgeLeftBottomTemp, XYZ textEdgeRightBottomTemp,
        XYZ textEdgeLeftTopTemp, XYZ textEdgeRightTopTemp) {
        mapService.PaintSquare(textEdgeLeftBottomTemp);
        mapService.PaintSquare(textEdgeRightBottomTemp);
        mapService.PaintSquare(textEdgeLeftTopTemp);
        mapService.PaintSquare(textEdgeRightTopTemp);
    }




    private (int, int) GetSquareInfo(SquareInfo[,] map, XYZ point, XYZ startPoint, int mmInSquare) {

        XYZ difVector = point - startPoint;
        double x = difVector.X;
        double y = difVector.Y;

        double revitStep = UnitUtilsHelper.ConvertToInternalValue(mmInSquare);

        int stepsForX = (int) Math.Floor(x / revitStep);
        int stepsForY = (int) Math.Floor(y / revitStep);

        //return map[stepsForY, stepsForX];
        return (stepsForY, stepsForX);
    }



    public XYZ GetDimensionCenterPoint(Dimension dimension) {
        // Получаем бесконечную линию вдоль которой строится размер
        var unboundLine = dimension.Curve as Line;
        // Получаем точку позиции текста - она при первоначальном размещении посередине текста, но чуть выше линии
        var ptForProject = dimension.TextPosition;
        return unboundLine.Project(ptForProject).XYZPoint;
    }

    public (XYZ, XYZ) GetDimensionEdgePoints(Dimension dimension) {
        var center = GetDimensionCenterPoint(dimension);
        var unboundLineVector = (dimension.Curve as Line).Direction;
        double? halfDistance = dimension.Value / 2;

        var leftEdgePoint = center + halfDistance.Value * unboundLineVector.Normalize().Negate();
        var rightEdgePoint = center + halfDistance.Value * unboundLineVector.Normalize();
        return (leftEdgePoint, rightEdgePoint);
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
