using System.Globalization;

using Autodesk.Revit.DB;

namespace RevitEditingZones.ViewModels {
    public class LevelViewModel {
        public LevelViewModel(Level level) {
            Level = level;
        }

        public Level Level { get; }
        public string LevelName => Level.Name;

        public override string ToString() {
            return LevelName;
        }
    }
}