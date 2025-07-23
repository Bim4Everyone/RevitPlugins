using Autodesk.Revit.DB;

namespace RevitCorrectNamingCheck.Models;

public class WorksetInfo {
    public WorksetId Id { get; set; }
    public string Name { get; set; }
    public NameStatus WorksetNameStatus { get; set; }

    public WorksetInfo(WorksetId id, string name) {
        Id = id;
        Name = name;
    }
}
