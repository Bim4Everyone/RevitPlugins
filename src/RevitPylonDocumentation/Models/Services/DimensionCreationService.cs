using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.Services;
internal class DimensionCreationService {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _repository;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;

    public DimensionCreationService(MainViewModel mvm, RevitRepository repository, PylonView pylonView, 
                                    DimensionBaseService dimensionBaseService) {
        _viewModel = mvm;
        _repository = repository;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
    }

    internal void CreateDimension(List<Grid> grids, Element elemForOffset, DirectionType directionType,
                                  double offsetCoefficient, XYZ gridsDirection,
                                  ReferenceArray oldRefArray = null) {
        if((grids?.Count ?? 0) == 0) { return; }
        var view = _viewOfPylon.ViewElement;
        var dimensionLineLeft = _dimensionBaseService.GetDimensionLine(elemForOffset, directionType,
                                                                      offsetCoefficient);
        var refArrayFormworkGridSide = _dimensionBaseService.GetDimensionRefs(grids, gridsDirection, oldRefArray);
        if(refArrayFormworkGridSide.Size != oldRefArray.Size) {
            _repository.Document.Create.NewDimension(view, dimensionLineLeft, refArrayFormworkGridSide,
                                                    _viewModel.SelectedDimensionType);
        }
    }

    internal Dimension CreateDimension(Element dimensioningElement, Element elemForOffset,
                                   DirectionType directionType, double offsetCoefficient,
                                   List<string> importantRefNameParts, ReferenceArray oldRefArray = null,
                                   bool needEqualityFormula = true) {

        if(dimensioningElement is null || (importantRefNameParts?.Count ?? 0) == 0) { return null; }
        var dimensionLine = _dimensionBaseService.GetDimensionLine(elemForOffset, directionType, offsetCoefficient);
        ReferenceArray refArray = _dimensionBaseService.GetDimensionRefs(dimensioningElement as FamilyInstance,
                                                                         importantRefNameParts,
                                                                         oldRefArray: oldRefArray);
        var dimension = _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine, refArray,
                                                                _viewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
        return dimension;
    }

    internal Dimension CreateDimension(ReferenceArray oldRefArray, Element elemForOffset,
                                       DirectionType directionType, double offsetCoefficient,
                                       bool needEqualityFormula = true) {

        var dimensionLine = _dimensionBaseService.GetDimensionLine(elemForOffset, directionType, offsetCoefficient);
        var dimension = _repository.Document.Create.NewDimension(_viewOfPylon.ViewElement, dimensionLine, oldRefArray,
                                                                _viewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
        return dimension;
    }
}
