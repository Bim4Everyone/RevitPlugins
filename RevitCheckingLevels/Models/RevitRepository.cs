using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitCheckingLevels.Models.LevelParser;

namespace RevitCheckingLevels.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public IEnumerable<Level> GetLevels() {
            return GetLevels(Document);
        }

        public IEnumerable<Level> GetLevels(RevitLinkType linkType) {
            var linkInstance = new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(RevitLinkInstance))
                .OfType<RevitLinkInstance>()
                .FirstOrDefault(item => item.GetTypeId() == linkType.Id);

            return GetLevels(linkInstance?.GetLinkDocument());
        }

        public IEnumerable<RevitLinkType> GetRevitLinkTypes() {
            return new FilteredElementCollector(Document)
                .WhereElementIsElementType()
                .OfClass(typeof(RevitLinkType))
                .OfType<RevitLinkType>()
                .OrderBy(item => item.Name);
        }

        public void UpdateElevations(IEnumerable<LevelInfo> levels) {
            using(Transaction transaction = Document.StartTransaction("Обновление отметок уровня")) {
                foreach(LevelInfo levelInfo in levels) {
                    levelInfo.Elevation = levelInfo.Level.GetMeterElevation();
                    levelInfo.Level.Name = levelInfo.FormatLevelName();
                }

                transaction.Commit();
            }
        }

        private IEnumerable<Level> GetLevels(Document document) {
            return new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_Levels)
                .OfType<Level>();
        }
    }
}