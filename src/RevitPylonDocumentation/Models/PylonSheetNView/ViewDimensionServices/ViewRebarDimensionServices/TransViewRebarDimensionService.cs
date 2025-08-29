using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class TransViewRebarDimensionService {
    internal TransViewRebarDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }


    internal void TryCreateTransViewRebarDimensions(View view, bool onTopOfRebar) {
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);
        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }
            // Определяем относительно чего нужно строить размерные линии - каркаса или пилона
            var pylon = SheetInfo.HostElems.First();
            var dimensionLineHostRef = onTopOfRebar ? skeletonParentRebar : pylon;

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            if(onTopOfRebar) {
                if(SheetInfo.RebarInfo.AllRebarAreL) {
                    // Когда все Гэшки
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                    ["низ", "фронт"], view, dimensionBaseService);
                } else if(SheetInfo.RebarInfo.HasLRebar) {
                    // Когда Гэшки с одной стороны
                    if(rebarFinder.DirectionHasLRebar(view, SheetInfo.ProjectSection, DirectionType.Top)) {
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 0.5,
                                        ["низ", "фронт"], view, dimensionBaseService);
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                        ["верх", "фронт"], view, dimensionBaseService);
                    } else {
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 0.5,
                                        ["верх", "фронт"], view, dimensionBaseService);
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                        ["низ", "фронт"], view, dimensionBaseService);
                    }
                } else {
                    // Размер по ФРОНТУ каркас (положение снизу ближнее)
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                    ["верх", "фронт"], view, dimensionBaseService);
                }
            } else {
                // Размер по ФРОНТУ каркас (положение снизу ближнее)
                CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                ["низ", "фронт"], view, dimensionBaseService);
            }
            
            // Размер по ФРОНТУ каркас края (положение снизу дальнее)
            CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 1,
                            ["низ", "фронт", "край"], view, dimensionBaseService);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ТОРЦУ армирование (положение справа ближнее)
            CreateDimension(skeletonParentRebar, pylon, DirectionType.Right, 0.5, ["низ", "торец", "край"], view, 
                            dimensionBaseService);
        } catch(Exception) { }
    }

    private void CreateDimension(Element dimensioningElement, Element elemForOffset,
                                 DirectionType directionType, double offsetCoefficient,
                                 List<string> importantRefNameParts, View view,
                                 DimensionBaseService dimensionBaseService, ReferenceArray oldRefArray = null,
                                 bool needEqualityFormula = true) {

        if(dimensioningElement is null || (importantRefNameParts?.Count ?? 0) == 0) { return; }
        var dimensionLine = dimensionBaseService.GetDimensionLine(elemForOffset, directionType, offsetCoefficient);
        ReferenceArray refArray = dimensionBaseService.GetDimensionRefs(dimensioningElement as FamilyInstance, '#', '/',
                                                                        importantRefNameParts, oldRefArray: oldRefArray);
        var dimension = Repository.Document.Create.NewDimension(view, dimensionLine, refArray,
                                                                ViewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
    }
}
