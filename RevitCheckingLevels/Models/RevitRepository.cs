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
        
        public bool IsKoordFile() {
            return Document.Title.Contains("_KOORD");
        }

        public bool IsKoordFile(RevitLinkType revitLinkType) {
            return revitLinkType.Name.Contains("_KOORD");
        }

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

        public IEnumerable<LevelCreationName> GetLevelCreationNames(IEnumerable<LevelInfo> levelInfos) {
            var levels = GetLevels(Document).ToList();
            return levelInfos
                .Select(item => CreateLevelCreationName(item, levels));
        }

        public void UpdateElevations(IEnumerable<LevelCreationName> levels) {
            using(Transaction transaction = Document.StartTransaction("Обновление отметок уровня")) {
                foreach(LevelCreationName levelCreationName in levels.Where(item => !item.DuplicateName)) {
                    levelCreationName.LevelInfo.Level.Name = levelCreationName.LevelName;
                }

                transaction.Commit();
            }
        }

        private string GetLevelName(LevelInfo levelInfo) {
            double elevation = levelInfo.Level.GetMeterElevation();
            string elevationName = levelInfo.FormatElevation(elevation);

            var elements = levelInfo.Level.Name.Split('_');
            elements[2] = elevationName;
            return string.Join("_", elements);
        }

        private LevelCreationName CreateLevelCreationName(LevelInfo levelInfo, List<Level> levels) {
            var levelName = GetLevelName(levelInfo);
            var duplicateName = IsDuplicateLevelName(levelName, levels);
            return new LevelCreationName() {LevelInfo = levelInfo, LevelName = levelName, DuplicateName = duplicateName};
        }

        private bool IsDuplicateLevelName(string levelName, List<Level> levels) {
            return levels.Any(item => item.Name.Equals(levelName));
        }

        private IEnumerable<Level> GetLevels(Document document) {
            return new FilteredElementCollector(document)
                .OfCategory(BuiltInCategory.OST_Levels)
                .OfType<Level>();
        }
    }

    internal class LevelCreationName {
        public bool DuplicateName { get; set; }
        public string LevelName { get; set; }
        public LevelInfo LevelInfo { get; set; }
    }
}