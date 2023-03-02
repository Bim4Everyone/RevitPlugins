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

        public bool HasLinkInstance(RevitLinkType linkType) {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(RevitLinkInstance))
                .OfType<RevitLinkInstance>()
                .Any(item => item.GetTypeId() == linkType.Id);
        }

        public IEnumerable<Level> GetLevels(RevitLinkType linkType) {
            var linkInstance = new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(RevitLinkInstance))
                .OfType<RevitLinkInstance>()
                .FirstOrDefault(item => item.GetTypeId() == linkType.Id);

            return linkInstance == null
                ? Enumerable.Empty<Level>()
                : GetLevels(linkInstance.GetLinkDocument());
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
                    double elevation = levelInfo.Level.GetMeterElevation();
                    var elements = levelInfo.Level.Name.Split('_');
                    elements[2] = elevation.ToString(LevelParserImpl.CultureInfo);
                    levelInfo.Level.Name = string.Join("_", elements);
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