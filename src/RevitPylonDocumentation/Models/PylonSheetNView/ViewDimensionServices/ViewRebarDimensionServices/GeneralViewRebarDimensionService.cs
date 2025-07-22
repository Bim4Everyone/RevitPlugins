using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class GeneralViewRebarDimensionService {
    internal GeneralViewRebarDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }


    internal void CreateGeneralRebarViewPlateDimensions(View view, FamilyInstance skeletonParentRebar, List<Element> platesArray,
                                                       DimensionOffsetType dimensionOffsetType, DimensionBaseService dimensionBaseService) {
        ReferenceArray refArraySide;
        if(SheetInfo.RebarInfo.AllRebarAreL) {
            // #1_горизонт_Г-стержень
            refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "Г-стержень"]);
        } else {
            // #1_горизонт_выпуск
            refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "выпуск"]);
        }

        // #_1_горизонт_край_низ
        refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "край", "низ"], refArraySide);

        var dimensionLineLeftFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar, dimensionOffsetType, 1.3);
        var dimensionRebarSideFirst = Repository.Document.Create.NewDimension(view, dimensionLineLeftFirst, refArraySide, ViewModel.SelectedDimensionType);


        double lengthTemp = 0.0;
        Element neededPlates = default;
        foreach(var plates in platesArray) {
            var length = plates.GetParamValue<double>("мод_Длина");
            if(length > lengthTemp) {
                lengthTemp = length;
                neededPlates = plates;
            }
        }

        var viewOptions = new Options {
            View = view,
            ComputeReferences = true,
            IncludeNonVisibleObjects = false
        };

        var plateRefs = neededPlates.get_Geometry(viewOptions)?
            .OfType<GeometryInstance>()
            .SelectMany(ge => ge.GetSymbolGeometry())
            .OfType<Solid>()
            .Where(solid => solid?.Volume > 0)
            .SelectMany(solid => solid.Faces.OfType<PlanarFace>())
            .Where(face => Math.Abs(face.FaceNormal.Z + 1) < 0.001)
            .ToList();

        foreach(var plateRef in plateRefs) {
            refArraySide.Append(plateRef.Reference);
        }

        var dimensionLineLeftSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, dimensionOffsetType, 0.7);

        var dimensionRebarSideSecond = Repository.Document.Create.NewDimension(view, dimensionLineLeftSecond,
                                                        refArraySide, ViewModel.SelectedDimensionType);
    }
}
