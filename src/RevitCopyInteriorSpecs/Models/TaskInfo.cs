using Autodesk.Revit.DB;

namespace RevitCopyInteriorSpecs.Models {
    internal class TaskInfo {

        public TaskInfo() { }

        public string GroupType { get; set; }
        public Level Level { get; set; }
        public string LevelShortName { get; set; }
        public Phase Phase { get; set; }
        public string FirstDispatcherGroupingLevel { get; set; }
        public string SecondDispatcherGroupingLevel { get; set; }
        public string ThirdDispatcherGroupingLevel { get; set; }
    }
}
