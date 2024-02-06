using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.Revit.Comparators;

namespace RevitCreateViewSheet.Models {
    public class RevitRepository {
        private readonly UIApplication _uiApplication;

        public RevitRepository(UIApplication uiApplication) {
            _uiApplication = uiApplication;
        }

        public Application Application {
            get => _uiApplication.Application;
        }

        public Document Document {
            get => UIDocument.Document;
        }

        public UIDocument UIDocument {
            get => _uiApplication.ActiveUIDocument;
        }


        public ViewSheet CreateViewSheet(FamilySymbol familySymbol) {
            return ViewSheet.Create(Document, familySymbol.Id);
        }

        public string GetDefaultAlbum() {
            return UIDocument.GetSelectedElements()
                .OfType<ViewSheet>()
                .Select(item => (string) item.GetParamValueOrDefault(SharedParamsConfig.Instance.AlbumBlueprints))
                .Distinct()
                .FirstOrDefault();
        }

        public List<string> GetAlbumsBlueprints() {
            return GetViewSheets()
                .Select(item => (string) item.GetParamValueOrDefault(SharedParamsConfig.Instance.AlbumBlueprints))
                .Where(item => item?.EndsWith("BIM") == false)
                .OrderBy(item => item)
                .Distinct()
                .ToList();
        }

        public List<ViewSheet> GetViewSheets() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSheet))
                .OfType<ViewSheet>()
                .ToList();
        }

        public List<FamilySymbol> GetTitleBlocks() {
            var category = Category.GetCategory(Document, BuiltInCategory.OST_TitleBlocks);
            return new FilteredElementCollector(Document)
                .OfClass(typeof(FamilySymbol))
                .OfType<FamilySymbol>()
                .Where(item => item.Category.Id == category.Id)
                .ToList();
        }

        public int GetLastViewSheetIndex(string albumBlueprints) {
            ViewSheet viewSheet = GetViewSheets()
                .Where(item => ((string) item.GetParamValueOrDefault(SharedParamsConfig.Instance.AlbumBlueprints))?.Equals(albumBlueprints) == true)
                .OrderBy(item => item, new ViewSheetComparer())
                .LastOrDefault();

            return GetViewSheetIndex(viewSheet) ?? 1;
        }

        private int? GetViewSheetIndex(ViewSheet viewSheet) {
            string index = viewSheet?.SheetNumber.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            return int.TryParse(index, out int result) ? result : (int?) null;
        }
    }
}
