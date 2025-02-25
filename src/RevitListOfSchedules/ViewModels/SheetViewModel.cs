using Autodesk.Revit.DB;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels {
    internal class SheetViewModel {


        public SheetViewModel(SheetElement sheetElement, ElementId linkTypeId = null) {


            Name = sheetElement.Name;
            Number = sheetElement.Number;
            AlbumName = sheetElement.AlbumName;
            RevisionNumber = sheetElement.RevisionNumber;
            LinkTypeId = linkTypeId;

        }

        public string Name { get; }
        public string Number { get; }
        public string AlbumName { get; }
        public string RevisionNumber { get; }
        public ElementId LinkTypeId { get; }
    }
}
