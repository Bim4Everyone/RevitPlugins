using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class GeneralViewPerpRebarDimensionService {
    internal GeneralViewPerpRebarDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) {
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


    internal bool LRebarIsRight(View view, RebarFinderService rebarFinder) {
        // Гэшка
        var lRebar = rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, 1101).FirstOrDefault();
        // Бутылка
        var bottleRebar = rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, 1204).FirstOrDefault();

        if(lRebar is null || bottleRebar is null) {
            return false;
        }

        var lRebarLocation = lRebar.Location as LocationPoint;
        var lRebarPt = lRebarLocation.Point;

        var bottleRebarLocation = bottleRebar.Location as LocationPoint;
        var bottleRebarPt = bottleRebarLocation.Point;

        var transform = view.CropBox.Transform;
        var inverseTransform = transform.Inverse;
        // Получаем координаты точек вставки в координатах вида
        var lRebarPtTransformed = inverseTransform.OfPoint(lRebarPt);
        var bottleRebarPtTransformed = inverseTransform.OfPoint(bottleRebarPt);

        return lRebarPtTransformed.X > bottleRebarPtTransformed.X;
    }





    public void TryCreateGeneralRebarPerpendicularViewAdditionalDimensions() {
        var doc = Repository.Document;
        var view = SheetInfo.GeneralViewPerpendicularRebar.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            var dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top, -1);

            var refArrayTop_1 = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "1_торец" });
            var dimensionTop_1 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop_1, ViewModel.SelectedDimensionType);
            if(dimensionTop_1.Value == 0) {
                doc.Delete(dimensionTop_1.Id);
            }

            var refArrayTop_2 = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "2_торец" });
            var dimensionTop_2 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop_2, ViewModel.SelectedDimensionType);
            if(dimensionTop_2.Value == 0) {
                doc.Delete(dimensionTop_2.Id);
            }

            //// Смещение выноски вправо
            //var rightDirection = GetViewDirections(view).RightDirection;
            //// .Multiply(offsetCoefficient)

            //var dimensionPoint_1 = dimensionBottom_1.LeaderEndPosition;
            //var dimensionPoint_2 = dimensionBottom_2.LeaderEndPosition;

            //dimensionPoint_1 = new XYZ(dimensionPoint_1.X, dimensionPoint_1.Y, 0);
            //dimensionPoint_2 = new XYZ(dimensionPoint_2.X, dimensionPoint_2.Y, 0);

            //var viewMin = view.CropBox.Min;
            //viewMin = new XYZ(viewMin.X, viewMin.Y, 0);

            //if(dimensionPoint_1.DistanceTo(viewMin) < dimensionPoint_2.DistanceTo(viewMin)) {
            //    dimensionBottom_1.LeaderEndPosition = dimensionPoint_1 + rightDirection;
            //    dimensionBottom_2.LeaderEndPosition = dimensionPoint_2 - rightDirection;
            //} else {
            //    dimensionBottom_1.LeaderEndPosition = dimensionPoint_1 - rightDirection;
            //    dimensionBottom_2.LeaderEndPosition = dimensionPoint_2 + rightDirection;
            //}
        } catch(Exception) { }
    }
}
