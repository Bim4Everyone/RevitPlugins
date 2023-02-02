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
            return new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_Levels)
                .OfType<Level>();
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
    }
}