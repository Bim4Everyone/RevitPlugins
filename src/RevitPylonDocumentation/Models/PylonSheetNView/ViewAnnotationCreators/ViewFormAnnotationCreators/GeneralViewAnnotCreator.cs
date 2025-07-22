using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewAnnotCreator : ViewAnnotationCreator {
    public GeneralViewAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var view = SheetInfo.GeneralView.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            var clampsParentRebars = rebarFinder.GetClampsParentRebars(view, SheetInfo.ProjectSection);
            if(clampsParentRebars is null) {
                return;
            }

            var grids = new FilteredElementCollector(Repository.Document, view.Id)
                .OfCategory(BuiltInCategory.OST_Grids)
                .Cast<Grid>()
                .ToList();

            var dimensionService = new GeneralViewDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon);

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.CreateGeneralViewPylonDimensions(view, skeletonParentRebar, grids, dimensionBaseService);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.CreateGeneralViewClampsDimensions(view, clampsParentRebars, dimensionBaseService);

            dimensionService.CreateGeneralViewTopAdditionalDimensions(view, skeletonParentRebar, dimensionBaseService);

            dimensionService.CreateGeneralViewPylonDimensions(view, SheetInfo.HostElems, dimensionBaseService);
        } catch(Exception) { }


        try {
            CreateGeneralViewPylonElevMark(view, SheetInfo.HostElems, dimensionBaseService);
        } catch(Exception) { }
    }



    /// <summary>
    /// Метод по созданию размеров по опалубке пилонов
    /// </summary>
    /// <param name="view">Вид, на котором нужно создать размеры</param>
    /// <param name="clampsParentRebars">Список экземпляров семейств пилонов</param>
    /// <param name="dimensionBaseService">Сервис по анализу основ размеров</param>
    private void CreateGeneralViewPylonElevMark(View view, List<Element> hostElems,
                                                DimensionBaseService dimensionBaseService) {
        var location = dimensionBaseService.GetDimensionLine(hostElems[0] as FamilyInstance,
                                                           DimensionOffsetType.Left, 2).Origin;
        foreach(var item in hostElems) {
            if(item is not FamilyInstance hostElem) { return; }

            // Собираем опорные плоскости по опалубке, например:
            // #_1_горизонт_край_низ
            // #_1_горизонт_край_верх
            ReferenceArray refArraySide = dimensionBaseService.GetDimensionRefs(hostElem, '#', '/', ["горизонт", "край"]);

            foreach(Reference reference in refArraySide) {
                SpotDimension spotElevation = Repository.Document.Create.NewSpotElevation(
                    view,
                    reference,
                    location,
                    location,
                    location,
                    location,
                    false);
                spotElevation.ChangeTypeId(ViewModel.SelectedSpotDimensionType.Id);
            }
        }
    }
}
