using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices;
internal class GeneralViewMarkService {
    internal GeneralViewMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                    PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }


    /// <summary>
    /// Метод по созданию размеров по опалубке пилонов
    /// </summary>
    /// <param name="view">Вид, на котором нужно создать размеры</param>
    /// <param name="clampsParentRebars">Список экземпляров семейств пилонов</param>
    /// <param name="dimensionBaseService">Сервис по анализу основ размеров</param>
    internal void TryCreatePylonElevMark(List<Element> hostElems, DimensionBaseService dimensionBaseService) {
        try {
            var location = dimensionBaseService.GetDimensionLine(hostElems.First() as FamilyInstance, 
                                                                 DimensionOffsetType.Left, 2).Origin;
            foreach(var item in hostElems) {
                if(item is not FamilyInstance hostElem) { return; }

                // Собираем опорные плоскости по опалубке, например:
                // #_1_горизонт_край_низ
                // #_1_горизонт_край_верх
                ReferenceArray refArraySide = dimensionBaseService.GetDimensionRefs(hostElem, '#', '/', 
                                                                                    ["горизонт", "край"]);
                foreach(Reference reference in refArraySide) {
                    SpotDimension spotElevation = Repository.Document.Create.NewSpotElevation(
                        ViewOfPylon.ViewElement,
                        reference,
                        location,
                        location,
                        location,
                        location,
                        false);
                    spotElevation.ChangeTypeId(ViewModel.SelectedSpotDimensionType.Id);
                }
            }
        } catch(Exception) { }
    }
}
