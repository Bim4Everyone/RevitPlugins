using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
internal class GeneralRebarViewDimCreator : ViewDimensionCreator {
    internal GeneralRebarViewDimCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewDimensions() {
        var doc = Repository.Document;
        View view = SheetInfo.GeneralRebarView.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            var dimensionLineBottom = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom);
            var refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт" });
            Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom, ViewModel.SelectedDimensionType);

            var dimensionLineBottomEdges = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 1.5);
            var refArrayBottomEdges = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт", "край" });
            Dimension dimensionBottomEdges = doc.Create.NewDimension(view, dimensionLineBottomEdges, refArrayBottomEdges, ViewModel.SelectedDimensionType);

            var dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top);
            var refArrayTop = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "верх", "фронт" });
            Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop, ViewModel.SelectedDimensionType);


            List<Element> plates = rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, 2001);
            CreateGeneralRebarViewPlateDimensions(view, skeletonParentRebar, plates, DimensionOffsetType.Left, dimensionBaseService);
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
}
