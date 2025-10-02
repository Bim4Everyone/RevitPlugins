using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.Services;
internal class DimensionCreationService {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _repository;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;

    //  Значения параметра способа отображения размера через формулу равенства
    private readonly int _dimEqualityFormulaIndex = 2;

    public DimensionCreationService(MainViewModel mvm, RevitRepository repository, PylonView pylonView, 
                                    DimensionBaseService dimensionBaseService) {
        _viewModel = mvm;
        _repository = repository;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
    }

    /// <summary>
    /// Создает размер по осям, которые распространяются в определенном заданном направлении.
    /// </summary>
    /// <param name="grids">Список осей</param>
    /// <param name="gridsDirection">Заданное направление осей, только этих осей будет строиться размер</param>
    /// <param name="elemForOffset">Элемент, от которого будет формироваться отступ для размерной линии</param>
    /// <param name="directionType">Направление отступа от элемента для отступа для размерной линии</param>
    /// <param name="offsetCoefficient">Значение отступа от элемента для размерной линии</param>
    /// <param name="oldRefArray">Существующий массив опорных плоскостей, к которому будут добавлены опорные плоскости
    /// по осям</param>
    internal void CreateDimension(List<Grid> grids, XYZ gridsDirection,
                                  DimensionLineOffsetOption dimLineOffsetOption,
                                  ReferenceArray oldRefArray = null) {
        if((grids?.Count ?? 0) == 0) { return; }
        var view = _viewOfPylon.ViewElement;
        var dimensionLine = _dimensionBaseService.GetDimensionLine(dimLineOffsetOption.ElemForOffset,
                                                                   dimLineOffsetOption.OffsetDirectionType, 
                                                                   dimLineOffsetOption.OffsetCoefficient);
        var refArrayFormworkGridSide = _dimensionBaseService.GetDimensionRefs(grids, gridsDirection, oldRefArray);
        if(refArrayFormworkGridSide.Size != oldRefArray.Size) {
            _repository.Document.Create.NewDimension(view, dimensionLine, refArrayFormworkGridSide,
                                                     _viewModel.SelectedDimensionType);
        }
    }

    /// <summary>
    /// Создает размер по опорным плоскостям элемента
    /// </summary>
    /// <param name="dimensioningElement">Элемент, опорные плоскости которого будут запрашиваться/образмериваться</param>
    /// <param name="elemForOffset">Элемент, от которого будет формироваться отступ для размерной линии</param>
    /// <param name="directionType">Направление отступа от элемента для отступа для размерной линии</param>
    /// <param name="offsetCoefficient">Значение отступа от элемента для размерной линии</param>
    /// <param name="importantRefNameParts">Перечень ключевых слов, которые должны быть в имени опорных плоскостей</param>
    /// <param name="oldRefArray">Существующий массив опорных плоскостей, к которому будут добавлены опорные плоскости</param>    
    /// <param name="needEqualityFormula">Нужно ли задавать формульный тип отображения значения размера</param>
    internal Dimension CreateDimension(Element dimensioningElement, DimensionLineOffsetOption dimLineOffsetOption,
                                       List<string> importantRefNameParts, ReferenceArray oldRefArray = null,
                                       bool needEqualityFormula = true) {

        if(dimensioningElement is null || (importantRefNameParts?.Count ?? 0) == 0) { return null; }
        var dimensionLine = _dimensionBaseService.GetDimensionLine(dimLineOffsetOption.ElemForOffset, 
                                                                   dimLineOffsetOption.OffsetDirectionType,
                                                                   dimLineOffsetOption.OffsetCoefficient);
        ReferenceArray refArray = _dimensionBaseService.GetDimensionRefs(dimensioningElement as FamilyInstance,
                                                                         importantRefNameParts,
                                                                         oldRefArray: oldRefArray);
        var dimension = _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine, refArray,
                                                                 _viewModel.SelectedDimensionType);
        
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, _dimEqualityFormulaIndex);
        }
        return dimension;
    }

    /// <summary>
    /// Создает размер по списку переданных опорных плоскостей
    /// </summary>
    /// <param name="oldRefArray">Массив опорных плоскостей, по которым будет построен размер</param>
    /// <param name="elemForOffset">Элемент, от которого будет формироваться отступ для размерной линии</param>
    /// <param name="directionType">Направление отступа от элемента для отступа для размерной линии</param>
    /// <param name="offsetCoefficient">Значение отступа от элемента для размерной линии</param>
    /// <param name="needEqualityFormula">Нужно ли задавать формульный тип отображения значения размера</param>
    internal Dimension CreateDimension(ReferenceArray oldRefArray, DimensionLineOffsetOption dimLineOffsetOption,
                                       bool needEqualityFormula = true) {

        var dimensionLine = _dimensionBaseService.GetDimensionLine(dimLineOffsetOption.ElemForOffset,
                                                                   dimLineOffsetOption.OffsetDirectionType,
                                                                   dimLineOffsetOption.OffsetCoefficient);
        var dimension = _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine, oldRefArray,
                                                                 _viewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, _dimEqualityFormulaIndex);
        }
        return dimension;
    }
}
