using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
internal class GeneralRebarViewPerpDimCreator : ViewDimensionCreator {
    internal GeneralRebarViewPerpDimCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }


    public override void TryCreateViewDimensions() {

        var doc = Repository.Document;
        var view = SheetInfo.GeneralRebarViewPerpendicular.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            var dimensionLineBottom = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom);
            var refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "торец" });
            Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom, ViewModel.SelectedDimensionType);

            var dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top);
            var refArrayTop = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "верх", "торец" });
            Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop, ViewModel.SelectedDimensionType);



            var defaultDimensionOffsetType = DimensionOffsetType.Right;
            // Будем ставить по дефолту справа
            // Слева будем ставить только если есть гэшка (но не все) и она справа

            if(SheetInfo.RebarInfo.HasLRebar && LRebarIsRight(view, rebarFinder)) {
                defaultDimensionOffsetType = DimensionOffsetType.Left;
            }

            List<Element> plates = rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, 2001);
            CreateGeneralRebarViewPlateDimensions(view, skeletonParentRebar, plates, defaultDimensionOffsetType, dimensionBaseService);





            if(!SheetInfo.RebarInfo.AllRebarAreL && SheetInfo.RebarInfo.HasLRebar) {
                // #1_горизонт_Г-стержень
                var refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "Г-стержень"]);
                // #_1_горизонт_край_низ
                refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "край", "низ"], refArraySide);

                defaultDimensionOffsetType = defaultDimensionOffsetType == DimensionOffsetType.Left ? DimensionOffsetType.Right : DimensionOffsetType.Left;

                var dimensionLineLeftFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar, defaultDimensionOffsetType, 1.3);
                Dimension dimensionRebarSideFirst = Repository.Document.Create.NewDimension(view, dimensionLineLeftFirst, refArraySide, ViewModel.SelectedDimensionType);
            }


        } catch(Exception) { }
    }


    private void CreateGeneralRebarViewPlateDimensions(View view, FamilyInstance skeletonParentRebar, List<Element> platesArray,
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
        Dimension dimensionRebarSideFirst = Repository.Document.Create.NewDimension(view, dimensionLineLeftFirst, refArraySide, ViewModel.SelectedDimensionType);


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

        Dimension dimensionRebarSideSecond = Repository.Document.Create.NewDimension(view, dimensionLineLeftSecond,
                                                        refArraySide, ViewModel.SelectedDimensionType);
    }


    private bool LRebarIsRight(View view, RebarFinderService rebarFinder) {
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
}
