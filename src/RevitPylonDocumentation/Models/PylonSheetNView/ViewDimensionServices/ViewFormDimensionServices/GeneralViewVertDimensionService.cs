using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
internal class GeneralViewVertDimensionService {
    private readonly DimensionType _selectedDimensionType;
    private readonly Document _doc;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;
    private readonly DimensionSegmentsService _dimSegmentsService;

    internal GeneralViewVertDimensionService(CreationSettings settings, Document document, PylonSheetInfo pylonSheetInfo,
                                             PylonView pylonView, DimensionBaseService dimensionBaseService) {
        _selectedDimensionType = settings.TypesSettings.SelectedDimensionType;
        _doc = document;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
        _dimSegmentsService = new DimensionSegmentsService(_viewOfPylon.ViewElement);
    }


    /// <summary>
    /// Вертикальный размер для вертикального вида по опалубка + армирование
    /// </summary>
    private Dimension TryCreateDimsForFormNRebar(FamilyInstance skeletonParentRebar, ReferenceArray refArrayFormwork, 
                                                 string side) {
        Dimension dimension = null;
        try {
            var dimensionLineBottomFirst = _dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                                  DirectionType.Bottom,
                                                                                  1.3);
            var refArrayFormworkRebarFront =
                _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                       ["низ", side, "край"],
                                                       oldRefArray: refArrayFormwork);

            dimension = _doc.Create.NewDimension(_viewOfPylon.ViewElement,
                                                                 dimensionLineBottomFirst,
                                                                 refArrayFormworkRebarFront,
                                                                 _selectedDimensionType);
        } catch(Exception) { }
        return dimension;
    }

    /// <summary>
    /// Редактирует крайние сегменты вертикальных размеров
    /// </summary>
    private void EditDimensionSegments(Dimension dimension) {
        if(dimension is null) { return; }
        _dimSegmentsService.EditEdgeDimensionSegments(dimension,
                                                      _dimSegmentsService.VertSmallUpDirectDimTextOffset,
                                                      _dimSegmentsService.VertSmallUpInvertedDimTextOffset);
    }


    /// <summary>
    /// Вертикальный размер для вертикального вида по опалубка + оси
    /// </summary>
    private double TryCreateDimensionByFormNGrids(List<Grid> grids, FamilyInstance skeletonParentRebar,
                                                  ReferenceArray refArrayFormwork) {
        // Определим отступ для размерной линии общего размера по опалубке (если есть верт оси, то будет дальше)
        double formworkDimensionLineOffset = 1.8;
        try {
            if(grids.Count > 0) {
                // Размер по ФРОНТУ опалубка + оси (положение снизу 2)
                var dimensionLineBottomSecond = _dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                                       DirectionType.Bottom,
                                                                                       formworkDimensionLineOffset);
                var refArrayFormworkGridFront = _dimensionBaseService.GetDimensionRefs(grids,
                                                                                       new XYZ(0, 0, 1),
                                                                                       refArrayFormwork);
                _doc.Create.NewDimension(_viewOfPylon.ViewElement,
                                                         dimensionLineBottomSecond,
                                                         refArrayFormworkGridFront,
                                                         _selectedDimensionType);

                // Т.к. поставили размер по осям, то изменяем отступ для размера положение снизу 3
                formworkDimensionLineOffset = 2.3;
            }
        } catch(Exception) { }
        return formworkDimensionLineOffset;
    }


    /// <summary>
    /// Вертикальный размер для вертикального вида по опалубке
    /// </summary>
    private void TryCreatePylonDimensions(FamilyInstance skeletonParentRebar, double formworkDimensionLineOffset, 
                                           ReferenceArray refArrayFormwork) {
        try {
            var dimensionLineBottomThird = _dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                                  DirectionType.Bottom,
                                                                                  formworkDimensionLineOffset);
            _doc.Create.NewDimension(_viewOfPylon.ViewElement, 
                                                     dimensionLineBottomThird, 
                                                     refArrayFormwork,
                                                     _selectedDimensionType);
        } catch(Exception) { }
    }


    /// <summary>
    /// Создание вертикального размера сверху по Г-образному стержню от его конца до ближайшей грани пилона
    /// </summary>
    private void TryCreateLRebarDimension() {
        try {
            if(!_sheetInfo.RebarInfo.HasLRebar) { return; }
            var skeletonParentRebar = _sheetInfo.RebarInfo.SkeletonParentRebar;

            // Проблематично найти ближайшую боковую грань пилона, поэтому просто создадим два размера 
            // от конца Гэшки до одной грани и до другой, и удалим больший
            var lastPylon = _sheetInfo.HostElems.Last();
            var lastPylonRefArrayFirst = _dimensionBaseService.GetDimensionRefs(lastPylon as FamilyInstance,
                                                                                ["1", "торец", "край"]);
            var lastPylonRefArraySecond = _dimensionBaseService.GetDimensionRefs(lastPylon as FamilyInstance,
                                                                                 ["2", "торец", "край"]);

            // Получаем позицию для размерной линии по Г-образному стержню
            var lRebar = _sheetInfo.RebarFinder.GetSimpleRebars(_viewOfPylon.ViewElement, _sheetInfo.ProjectSection, 1101)
                                               .FirstOrDefault();
            var dimensionLine = _dimensionBaseService.GetDimensionLine(lRebar, DirectionType.Top, 0.5);

            //"#1_торец_Г_нутрь"
            //"#1_торец_Г_край"
            if(_sheetInfo.RebarInfo.AllRebarAreL) {
                CreateLRebarToPylonDimension(skeletonParentRebar, dimensionLine,
                                             ["1_торец", "Г", "край"], lastPylonRefArrayFirst, lastPylonRefArraySecond);
                CreateLRebarToPylonDimension(skeletonParentRebar, dimensionLine,
                                             ["2_торец", "Г", "край"], lastPylonRefArrayFirst, lastPylonRefArraySecond);
            } else if(_sheetInfo.RebarInfo.HasLRebar) {
                if(_sheetInfo.RebarFinder.DirectionHasLRebar(_viewOfPylon.ViewElement,
                                                             _sheetInfo.ProjectSection,
                                                             DirectionType.Right)
                    && _sheetInfo.RebarInfo.SecondLRebarParamValue) {
                    CreateLRebarToPylonDimension(skeletonParentRebar, dimensionLine,
                                                 ["2_торец", "Г", "край"],
                                                 lastPylonRefArrayFirst, lastPylonRefArraySecond);
                } else {
                    CreateLRebarToPylonDimension(skeletonParentRebar, dimensionLine,
                                                 ["1_торец", "Г", "край"],
                                                 lastPylonRefArrayFirst, lastPylonRefArraySecond);
                }
            }
        } catch(Exception) { }
    }

    private void CreateLRebarToPylonDimension(FamilyInstance skeletonParentRebar, Line dimensionLine,
                                              List<string> importantRefNameParts,
                                              ReferenceArray pylonRefArrayFirst,
                                              ReferenceArray pylonRefArraySecond) {
        var refArrayFirst = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                   importantRefNameParts,
                                                                   oldRefArray: pylonRefArrayFirst);
        var refArraySecond = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                    importantRefNameParts,
                                                                    oldRefArray: pylonRefArraySecond);

        var dimensionFirst = _doc.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine,
                                                                      refArrayFirst, _selectedDimensionType);
        var dimensionSecond = _doc.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine,
                                                                       refArraySecond, _selectedDimensionType);

        if(dimensionFirst.Value > dimensionSecond.Value) {
            _doc.Delete(dimensionFirst.Id);
        } else {
            _doc.Delete(dimensionSecond.Id);
        }
    }


    internal void CreateDimensions(FamilyInstance skeletonParentRebar, List<Grid> grids, bool isForPerpView) {
        string side = isForPerpView ? "торец" : "фронт";
        // Ссылки на опорные плоскости - крайние пилона
        var refArrayFormwork = _dimensionBaseService.GetDimensionRefs(_sheetInfo.HostElems.First() as FamilyInstance,
                                                                      [side, "край"]);
        // Размер опалубка + армирование(положение снизу 0.5)
        var vertDimsForEdit = TryCreateDimsForFormNRebar(skeletonParentRebar, refArrayFormwork, side);

        // Изменяем размер, передвигая текст у крайних сегментов для корректного отображения
        EditDimensionSegments(vertDimsForEdit);

        // Размер по ФРОНТУ опалубка + оси (положение снизу 1)
        // Положение размера по опалубке зависит от того будет ли установлен этот размер
        double formworkFrontDimLineOffset = TryCreateDimensionByFormNGrids(grids, skeletonParentRebar,
                                                                            refArrayFormwork);
        // Размер по ФРОНТУ опалубка (положение снизу 1/1.5)
        TryCreatePylonDimensions(skeletonParentRebar, formworkFrontDimLineOffset, refArrayFormwork);

        // Если это вертикальный перпендикулярный вид, то необходимо также поставить размеры по выходам Гэшек за пилон
        if(isForPerpView) {
            TryCreateLRebarDimension();
        }
    }
}
