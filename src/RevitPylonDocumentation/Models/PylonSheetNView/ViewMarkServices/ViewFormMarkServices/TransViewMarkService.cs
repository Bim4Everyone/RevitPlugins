using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
internal class TransViewMarkService {
    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly AnnotationService _annotationService;

    private readonly FamilySymbol _tagSkeletonSymbol;

    internal TransViewMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                  PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        _viewPointsAnalyzer = new ViewPointsAnalyzer(pylonView);
        _annotationService = new AnnotationService(pylonView);

        // Находим типоразмер марки несущей арматуры для обозначения марки изделия
        _tagSkeletonSymbol = mvm.SelectedSkeletonTagType;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }

    internal void TryCreateTransverseViewBarMarks() {
        try {
            var simpleRebars = SheetInfo.RebarInfo.SimpleVerticalRebars;
            if(simpleRebars.Count > 0) {
                CreateLeftBottomMark(simpleRebars);
            }
        } catch(Exception) { }
    }


    private void CreateLeftBottomMark(List<Element> simpleRebars) {
        // Получаем референс-элемент
        var leftBottomVerticalBar = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftBottom, 
                                                                                  false);
        // Получаем точку в которую нужно поставить аннотацию
        var pylonPoint = _viewPointsAnalyzer.GetPylonPointByDirection(SheetInfo, DirectionType.LeftBottom);
        var pointLeftBottom = _viewPointsAnalyzer.GetPointByDirection(pylonPoint, DirectionType.LeftBottom, 0.9, 0.55);

        // Создаем марку арматуры
        _annotationService.CreateRebarTag(pointLeftBottom, _tagSkeletonSymbol, leftBottomVerticalBar);
    }
}


