using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkCreators;
internal class GeneralViewMarkCreator : ViewMarkCreator {
    public GeneralViewMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewMarks() {
        var view = SheetInfo.GeneralView.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);
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
