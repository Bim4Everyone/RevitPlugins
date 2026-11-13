using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
internal class TransViewMarkService {
    private readonly CreationSettings _settings;
    private readonly Document _doc;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly ViewPointsAnalyzerService _viewPointsAnalyzer;
    private readonly TagCreationService _annotationService;

    private readonly FamilySymbol _tagSkeletonSymbol;

    internal TransViewMarkService(CreationSettings settings, Document document, PylonSheetInfo pylonSheetInfo,
                                  PylonView pylonView) {
        _settings = settings;
        _doc = document;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _viewPointsAnalyzer = new ViewPointsAnalyzerService(pylonView);
        _annotationService = new TagCreationService(pylonView);

        // Находим типоразмер марки несущей арматуры для обозначения марки изделия
        _tagSkeletonSymbol = _settings.AnnotationSettings.SelectedSkeletonTagType;
    }


    internal void TryCreateTransverseViewBarMarks() {
        try {
            var simpleRebars = _sheetInfo.RebarInfo.SimpleVerticalRebars;
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
        var pylonPoint = _viewPointsAnalyzer.GetPylonPointByDirection(_sheetInfo, DirectionType.LeftBottom);
        var pointLeftBottom = _viewPointsAnalyzer.GetPointByDirection(pylonPoint, DirectionType.LeftBottom, 0.95, 0.55);

        // Создаем марку арматуры

        var tagOption = new TagOption() { BodyPoint = pointLeftBottom, TagSymbol = _tagSkeletonSymbol };
        _annotationService.CreateRebarTag(tagOption, leftBottomVerticalBar);
    }
}


