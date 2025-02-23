using Autodesk.Revit.DB;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels {
    internal class SheetViewModel {


        public SheetViewModel(SheetElement sheetElement, ElementId id = null) {


            Name = sheetElement.Name;
            Number = sheetElement.Number;
            AlbumName = sheetElement.AlbumName;
            ChangeNumber = sheetElement.ChangeNumber;
            Id = id;

        }

        public string Name { get; }
        public string Number { get; }
        public string AlbumName { get; }
        public string ChangeNumber { get; }
        public ElementId Id { get; }
    }
}
