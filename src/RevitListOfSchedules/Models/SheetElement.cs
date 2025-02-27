using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

namespace RevitListOfSchedules.Models {
    internal class SheetElement {

        public SheetElement(ViewSheet viewSheet) {

            SharedParam sheetNumberParam = SharedParamsConfig.Instance.StampSheetNumber;
            SharedParam changeAlbumParam = SharedParamsConfig.Instance.Level;

            Sheet = viewSheet;
            Name = Sheet.Name;
            Number = SetParam(sheetNumberParam);
            RevisionNumber = SetParam(changeAlbumParam);
        }

        public string Name { get; }
        public string Number { get; }
        public string RevisionNumber { get; }
        public ViewSheet Sheet { get; }


        private string SetParam(SharedParam sharedParam) {
            string value = null;
            if(Sheet.IsExistsParam(sharedParam)) {
                if(Sheet.IsExistsParamValue(sharedParam)) {
                    value = Sheet.GetParamValueOrDefault<string>(sharedParam);
                }
            }
            return value;
        }
    }
}
