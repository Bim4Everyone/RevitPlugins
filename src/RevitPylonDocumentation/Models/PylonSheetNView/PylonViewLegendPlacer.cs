using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonViewLegendPlacer {
    internal PylonViewLegendPlacer(CreationSettings settings, Document document, PylonSheetInfo pylonSheetInfo) {
        ProjectSettings = settings.ProjectSettings;
        TypesSettings = settings.TypesSettings;
        Doc = document;
        SheetInfo = pylonSheetInfo;
    }

    internal UserProjectSettings ProjectSettings { get; set; }
    internal UserTypesSettings TypesSettings { get; set; }

    internal Document Doc { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }


    /// <summary>
    /// Размещает легенду примечаний на листе над штампом
    /// </summary>
    internal bool PlaceNoteLegend() {
        // Проверяем вдруг легенда не выбрана
        if(TypesSettings.SelectedLegend is null) {
            return false;
        } else {
            // Заполняем данные для задания
            SheetInfo.LegendView.ViewportTypeName = "Без названия";

            double coordinateX = UnitUtilsHelper.ConvertToInternalValue(int.Parse(ProjectSettings.LegendXOffset));
            double coordinateY = UnitUtilsHelper.ConvertToInternalValue(int.Parse(ProjectSettings.LegendYOffset));

            SheetInfo.LegendView.ViewportCenter = new XYZ(coordinateX, coordinateY, 0);
        }

        // Проверяем можем ли разместить на листе легенду
        if(!Viewport.CanAddViewToSheet(Doc, SheetInfo.PylonViewSheet.Id, TypesSettings.SelectedLegend.Id)) { 
            return false; 
        }

        // Размещаем легенду на листе
        var viewPort = Viewport.Create(Doc, 
                                       SheetInfo.PylonViewSheet.Id, 
                                       TypesSettings.SelectedLegend.Id, 
                                       SheetInfo.LegendView.ViewportCenter);
        SheetInfo.LegendView.ViewportElement = viewPort;

        // Задание правильного типа видового экрана
        ICollection<ElementId> typesOfViewPort = viewPort.GetValidTypes();
        foreach(ElementId typeId in typesOfViewPort) {
            if(Doc.GetElement(typeId) is not ElementType type) {
                continue;
            }

            if(type.Name == SheetInfo.LegendView.ViewportTypeName) {
                viewPort.ChangeTypeId(type.Id);
                break;
            }
        }
        return true;
    }
}
