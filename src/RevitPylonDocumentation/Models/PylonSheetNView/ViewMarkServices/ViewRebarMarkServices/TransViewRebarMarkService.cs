using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
internal class TransViewRebarMarkService {  
    private readonly int _formNumberForVerticalRebarMax = 1499;
    private readonly int _formNumberForVerticalRebarMin = 1101;

    private readonly int _formNumberForClampsMax = 1599;
    private readonly int _formNumberForClampsMin = 1500;

    private readonly int _formNumberForCBarMin = 1202;

    private readonly ViewPointsAnalyzerService _viewPointsAnalyzer;
    private readonly TagCreationService _annotationService;

    private readonly FamilySymbol _tagSymbolWithComment;
    private readonly FamilySymbol _tagSymbolWithSerif;
    private readonly FamilySymbol _tagSymbolWithStep;

    internal TransViewRebarMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                       PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        _viewPointsAnalyzer = new ViewPointsAnalyzerService(pylonView);
        _annotationService = new TagCreationService(pylonView);

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Без засечки на конце
        _tagSymbolWithComment = mvm.SelectedRebarTagTypeWithComment;
        // Без засечки на конце
        _tagSymbolWithStep = mvm.SelectedRebarTagTypeWithStep;
        // С засечкой на конце
        _tagSymbolWithSerif = mvm.SelectedRebarTagTypeWithSerif;
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
            // Марка по вертикальным стержням
            var simpleRebars = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection,
                                                                     _formNumberForVerticalRebarMin,
                                                                     _formNumberForVerticalRebarMax,
                                                                     _formNumberForCBarMin, _formNumberForCBarMin);
            // Если у нас есть Г-образные стержни или стержни разной длины, то нужно ставить две разные марки
            // Если нет - то допускается поставить одну марку, которая будет характеризовать все стрежни (они одинаковые)
            if(SheetInfo.RebarInfo.FirstLRebarParamValue
                || SheetInfo.RebarInfo.SecondLRebarParamValue
                || SheetInfo.RebarInfo.DifferentRebarParamValue) {
                // ПРАВЫЙ НИЖНИЙ УГОЛ
                CreateRightBottomMark(simpleRebars, true);

                // ЛЕВЫЙ ВЕРХНИЙ УГОЛ
                CreateLeftTopMark(simpleRebars);
            } else {
                // ПРАВЫЙ НИЖНИЙ УГОЛ
                CreateRightBottomMark(simpleRebars, false);
            }
            // Марка по хомутам
            var simpleClamps = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection,
                                                                     _formNumberForClampsMin, _formNumberForClampsMax);
            if(simpleClamps != null) {
                CreateLeftBottomMark(simpleClamps, simpleRebars);
                CreateRightTopMark(simpleClamps, simpleRebars);
            }
            // Марка по шпилькам
            var simpleCBars = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection,
                                                                    _formNumberForCBarMin);
            if(simpleCBars != null) {
                CreateLeftMark(simpleCBars, simpleRebars);
            }
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание марки по вертикальному армированию слева сверху
    /// </summary>
    private void CreateLeftTopMark(List<Element> simpleRebars) {
        // Получаем референс-элемент
        var leftTopElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftTop, false);
        var id = leftTopElement.Id;
        // Устанавливаем значение комментария у арматуры, к которой привяжем марку
        leftTopElement.SetParamValue(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, $"{simpleRebars.Count / 2} шт.");

        // Получаем точку в которую нужно поставить аннотацию
        var pylonPoint = _viewPointsAnalyzer.GetPylonPointByDirection(SheetInfo, DirectionType.LeftTop);
        var pointLeftTop = _viewPointsAnalyzer.GetPointByDirection(pylonPoint, DirectionType.LeftTop, 0.7, 0.25);

        // Создаем марку арматуры
        var tagOption = new TagOption() { BodyPoint = pointLeftTop, TagSymbol = _tagSymbolWithComment};
        _annotationService.CreateRebarTag(tagOption, leftTopElement);
    }

    /// <summary>
    /// Создание марки по вертикальному армированию справа снизу
    /// </summary>
    private void CreateRightBottomMark(List<Element> simpleRebars, bool hasLRebar) {
        // Получаем референс-элемент
        var rightBottomElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.RightBottom, false);

        // Устанавливаем значение комментария у арматуры, к которой привяжем марку
        string commentValue = hasLRebar ? $"{simpleRebars.Count / 2} шт." : $"{simpleRebars.Count} шт.";
        rightBottomElement.SetParamValue(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, commentValue);

        // Получаем точку в которую нужно поставить аннотацию
        var pylonPoint = _viewPointsAnalyzer.GetPylonPointByDirection(SheetInfo, DirectionType.RightBottom);
        var pointRightBottom = _viewPointsAnalyzer.GetPointByDirection(pylonPoint, DirectionType.RightBottom, 0.7, 0.5);

        // Создаем марку арматуры
        var tagOption = new TagOption() { BodyPoint = pointRightBottom, TagSymbol = _tagSymbolWithComment };
        _annotationService.CreateRebarTag(tagOption, rightBottomElement);
    }

    /// <summary>
    /// Создание марки по хомутам слева сверху
    /// </summary>
    private void CreateLeftBottomMark(List<Element> simpleClamps, List<Element> simpleRebars) {
        // Получаем референс-элемент
        var leftClamp = _viewPointsAnalyzer.GetElementByDirection(simpleClamps, DirectionType.Left, false);

        // Получаем точку в которую нужно поставить аннотацию
        var pylonPoint = _viewPointsAnalyzer.GetPylonPointByDirection(SheetInfo, DirectionType.LeftBottom);
        var pointLeftBottom = _viewPointsAnalyzer.GetPointByDirection(pylonPoint, DirectionType.LeftBottom, 0.7, 0.25);
        // Создаем марку арматуры
        var tagOption = new TagOption() { BodyPoint = pointLeftBottom, TagSymbol = _tagSymbolWithStep };
        var leftBottomTag = _annotationService.CreateRebarTag(tagOption, leftClamp);

#if REVIT_2022_OR_GREATER
        leftBottomTag.LeaderEndCondition = LeaderEndCondition.Free;
        var leftBottomClampRef = new Reference(leftClamp);

        var tagLeaderEnd = leftBottomTag.GetLeaderEnd(leftBottomClampRef);
        tagLeaderEnd = _viewPointsAnalyzer.GetPointByDirection(tagLeaderEnd, DirectionType.Bottom, 0, 0.3);
        leftBottomTag.SetLeaderEnd(leftBottomClampRef, tagLeaderEnd);
#endif
    }

    /// <summary>
    /// Создание марки по хомутам справа сверху
    /// </summary>
    private void CreateRightTopMark(List<Element> simpleClamps, List<Element> simpleRebars) {
        // Получаем референс-элемент
        var rightClamp = _viewPointsAnalyzer.GetElementByDirection(simpleClamps, DirectionType.Right, true);

        // Получаем точку в которую нужно поставить аннотацию
        var pylonPoint = _viewPointsAnalyzer.GetPylonPointByDirection(SheetInfo, DirectionType.RightTop);
        var pointRightTop = _viewPointsAnalyzer.GetPointByDirection(pylonPoint, DirectionType.RightTop, 0.8, 0.4);
        // Создаем марку арматуры
        var tagOption = new TagOption() { BodyPoint = pointRightTop, TagSymbol = _tagSymbolWithStep };
        var rightTopTag = _annotationService.CreateRebarTag(tagOption, rightClamp);

#if REVIT_2022_OR_GREATER
        rightTopTag.LeaderEndCondition = LeaderEndCondition.Free;
        var rightTopClampRef = new Reference(rightClamp);

        var tagLeaderEnd = rightTopTag.GetLeaderEnd(rightTopClampRef);
        tagLeaderEnd = _viewPointsAnalyzer.GetPointByDirection(tagLeaderEnd, DirectionType.Top, 0, 0.2);
        rightTopTag.SetLeaderEnd(rightTopClampRef, tagLeaderEnd);
#endif
    }

    /// <summary>
    /// Создание марки по шпилькам
    /// </summary>
    private void CreateLeftMark(List<Element> simpleCBars, List<Element> simpleRebars) {
        // Получаем референс-элемент
        var leftCBar = _viewPointsAnalyzer.GetElementByDirection(simpleCBars, DirectionType.Left, true);

        // Получаем точку в которую нужно поставить аннотацию
        var pointLeft = _viewPointsAnalyzer.GetPointByDirection(leftCBar, DirectionType.LeftBottom, 1.5, 0.1, true);

        // Создаем марку арматуры
        var tagOption = new TagOption() { BodyPoint = pointLeft, TagSymbol = _tagSymbolWithSerif };
        var leftTag = _annotationService.CreateRebarTag(tagOption, simpleCBars);
        leftTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
        foreach(var simpleCBar in simpleCBars) {
            var simpleCBarRef = new Reference(simpleCBar);
            var tagLeaderEndPoint = _viewPointsAnalyzer.GetPointByDirection(simpleCBar, DirectionType.LeftBottom,
                                                                            0, 0.1, true);
            leftTag.SetLeaderEnd(simpleCBarRef, tagLeaderEndPoint);
        }
#endif
    }
}
