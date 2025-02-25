using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

namespace RevitListOfSchedules.Models {
    internal class SheetElement {

        private readonly ViewSheet _viewSheet;

        public SheetElement(ViewSheet viewSheet) {

            _viewSheet = viewSheet;

            SharedParam sheetNumberParam = SharedParamsConfig.Instance.StampSheetNumber;

            SharedParam sheetAlbumParam = SharedParamsConfig.Instance.AlbumBlueprints;

            SharedParam changeAlbumParam = SharedParamsConfig.Instance.Level;

            Name = _viewSheet.Name;

            Number = SetParam(sheetNumberParam);

            AlbumName = SetParam(sheetAlbumParam);

            RevisionNumber = SetParam(changeAlbumParam);

        }

        public string Name { get; }
        public string Number { get; }
        public string AlbumName { get; }
        public string RevisionNumber { get; }


        private string SetParam(SharedParam sharedParam) {
            string value = null;
            if(_viewSheet.IsExistsParam(sharedParam)) {
                if(_viewSheet.IsExistsParamValue(sharedParam)) {
                    value = _viewSheet.GetParamValueOrDefault<string>(sharedParam, "Нет параметра");
                }
            }
            return value;
        }
    }
}
