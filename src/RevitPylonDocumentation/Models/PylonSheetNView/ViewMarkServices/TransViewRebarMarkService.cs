using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices;
internal class TransViewRebarMarkService {
    private readonly string _commentParamName = "Комментарии";
    
    private readonly int _formNumberForSkeletonPlatesMax = 2999;
    private readonly int _formNumberForSkeletonPlatesMin = 2001;

    private readonly int _formNumberForVerticalRebarMax = 1499;
    private readonly int _formNumberForVerticalRebarMin = 1101;

    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly AnnotationService _annotationService;

    private readonly FamilySymbol _tagSymbolWithoutSerif;
    private readonly FamilySymbol _gostTagSymbol;

    private readonly string _weldingGostText = "ГОСТ 14098-2014-Н1-Рш";

    internal TransViewRebarMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                       PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        _viewPointsAnalyzer = new ViewPointsAnalyzer(pylonView);
        _annotationService = new AnnotationService(pylonView);

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Без засечки на конце
        _tagSymbolWithoutSerif = 
            Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Комментарий - Полка 10");
        // Находим типоразмер типовой аннотации для метки ГОСТа сварки
        _gostTagSymbol = Repository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }

    /// <summary>
    /// Создает марки арматурных стержней
    /// </summary>
    internal void TryCreateBarMarks() {
        try {
            var simpleRebars = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection,
                                                         _formNumberForVerticalRebarMin,
                                                         _formNumberForVerticalRebarMax);
            // Если у нас есть Г-образные стержни или стержни разной длины, то нужно ставить две разные марки
            // Если нет - то допускается поставить одну марку, которая будет характеризовать все стрежни (они одинаковые)
            if(SheetInfo.RebarInfo.FirstLRebarParamValue
                || SheetInfo.RebarInfo.SecondLRebarParamValue
                || SheetInfo.RebarInfo.DifferentRebarParamValue) {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                CreateLeftBottomMark(simpleRebars, true);

                // ЛЕВЫЙ ВЕРХНИЙ УГОЛ
                CreateLeftTopMark(simpleRebars);
            } else {
                // ЛЕВЫЙ НИЖНИЙ УГОЛ
                CreateLeftBottomMark(simpleRebars, false);
            }
            // ПРАВЫЙ НИЖНИЙ УГОЛ
            CreateRightBottomMark(simpleRebars);
        } catch(Exception) { }
    }

    internal void TryCreatePlateMarks() {
        try {
            var simplePlates = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection,
                                                         _formNumberForSkeletonPlatesMin,
                                                         _formNumberForSkeletonPlatesMax);
            if(simplePlates.Count == 0) {
                return;
            }
            CreateTopMark(simplePlates);
            CreateBottomMark(simplePlates);
            CreateLeftMark(simplePlates);
            CreateRightMark(simplePlates);
        } catch(Exception) { }
    }

    private void CreateLeftBottomMark(List<Element> simpleRebars, bool hasLRebar) {
        // Получаем референс-элемент
        var leftBottomElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftBottom, false);

        // Устанавливаем значение комментария у арматуры, к которой привяжем марку
        string commentValue = hasLRebar ? $"{simpleRebars.Count / 2} шт." : $"{simpleRebars.Count} шт.";
        leftBottomElement.SetParamValue(_commentParamName, commentValue);

        // Получаем точку в которую нужно поставить аннотацию
        var pointLeftBottom = _viewPointsAnalyzer.GetPointByDirection(leftBottomElement, DirectionType.LeftBottom, 1, 
                                                                      0.4, false);
        // Создаем марку арматуры
        _annotationService.CreateRebarTag(pointLeftBottom, _tagSymbolWithoutSerif, leftBottomElement);
    }

    private void CreateLeftTopMark(List<Element> simpleRebars) {
        // Получаем референс-элемент
        var leftTopElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftTop, false);

        // Устанавливаем значение комментария у арматуры, к которой привяжем марку
        leftTopElement.SetParamValue(_commentParamName, $"{simpleRebars.Count / 2} шт.");

        // Получаем точку в которую нужно поставить аннотацию
        var pointLeftTop = _viewPointsAnalyzer.GetPointByDirection(leftTopElement, DirectionType.LeftTop, 1, 0.4, false);

        // Создаем марку арматуры
        _annotationService.CreateRebarTag(pointLeftTop, _tagSymbolWithoutSerif, leftTopElement);
    }

    private void CreateRightBottomMark(List<Element> simpleRebars) {
        // Получаем референс-элемент
        var rightBottomElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.RightBottom, 
                                                                           false);
        // Получаем точку в которую нужно поставить аннотацию
        var pointRightBottom = _viewPointsAnalyzer.GetPointByDirection(rightBottomElement, DirectionType.RightBottom, 
                                                                       2, 0.4, false);
        // Создаем типовую аннотацию для обозначения ГОСТа
        _annotationService.CreateUniversalTag(pointRightBottom, _gostTagSymbol, rightBottomElement, 
                                              UnitUtilsHelper.ConvertToInternalValue(40), _weldingGostText);
    }

    private void CreateTopMark(List<Element> simplePlates) {

        // Получаем референс-элемент
        Element topPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Top, true);

        // Получаем точку в которую нужно поставить аннотацию
        var pointTopPlateLeader = _viewPointsAnalyzer.GetPointByDirection(topPlate, DirectionType.Right, 0.2, 0, true);
        var pointTopPlate = _viewPointsAnalyzer.GetPointByDirection(topPlate, DirectionType.LeftBottom, 0.5, 0.4, true);

        // Создаем марку арматуры
        var topPlateTag = _annotationService.CreateRebarTag(pointTopPlate, _tagSymbolWithoutSerif, topPlate);
        topPlateTag.LeaderEndCondition = LeaderEndCondition.Free;
#if REVIT_2022_OR_GREATER
        topPlateTag.SetLeaderEnd(new Reference(topPlate), pointTopPlateLeader);
#endif
    }

    private void CreateBottomMark(List<Element> simplePlates) {
        // Получаем референс-элемент
        Element bottomPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Bottom, true);

        // Получаем точку в которую нужно поставить аннотацию
        var pointBottomPlateLeader = _viewPointsAnalyzer.GetPointByDirection(bottomPlate, DirectionType.Left, 
                                                                             0.3, 0, true);
        var pointBottomPlate = _viewPointsAnalyzer.GetPointByDirection(bottomPlate, DirectionType.RightTop, 
                                                                       0.35, 0.3, true);
        // Создаем марку арматуры
        var bottomPlateTag = _annotationService.CreateRebarTag(pointBottomPlate, _tagSymbolWithoutSerif, bottomPlate);
        bottomPlateTag.LeaderEndCondition = LeaderEndCondition.Free;
#if REVIT_2022_OR_GREATER
        bottomPlateTag.SetLeaderEnd(new Reference(bottomPlate), pointBottomPlateLeader);
#endif
    }

    private void CreateLeftMark(List<Element> simplePlates) {
        // Получаем референс-элемент
        Element leftPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Left, true);

        // Получаем точку в которую нужно поставить аннотацию
        var pointLeftPlateLeader = _viewPointsAnalyzer.GetPointByDirection(leftPlate, DirectionType.Bottom, 
                                                                           0.4, 0, true);
        var pointLeftPlate = _viewPointsAnalyzer.GetPointByDirection(leftPlate, DirectionType.LeftBottom, 
                                                                     0.8, 0.3, true);
        // Создаем марку арматуры
        var leftPlateTag = _annotationService.CreateRebarTag(pointLeftPlate, _tagSymbolWithoutSerif, leftPlate);
        leftPlateTag.LeaderEndCondition = LeaderEndCondition.Free;
#if REVIT_2022_OR_GREATER
        leftPlateTag.SetLeaderEnd(new Reference(leftPlate), pointLeftPlateLeader);
#endif
    }

    private void CreateRightMark(List<Element> simplePlates) {
        // Получаем референс-элемент
        Element rightPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Right, true);

        // Получаем точку в которую нужно поставить аннотацию
        var pointRightPlateLeader = _viewPointsAnalyzer.GetPointByDirection(rightPlate, DirectionType.Top, 0.4, 0, true);
        var pointRightPlate = _viewPointsAnalyzer.GetPointByDirection(rightPlate, DirectionType.RightTop, 0.8, 0.6, true);

        // Создаем марку арматуры
        var rightPlateTag = _annotationService.CreateRebarTag(pointRightPlate, _tagSymbolWithoutSerif, rightPlate);
        rightPlateTag.LeaderEndCondition = LeaderEndCondition.Free;
#if REVIT_2022_OR_GREATER
        rightPlateTag.SetLeaderEnd(new Reference(rightPlate), pointRightPlateLeader);
#endif
    }
}
