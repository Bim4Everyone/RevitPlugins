using Autodesk.Revit.DB;

namespace RevitCorrectNamingCheck.Models;

public class WorksetInfo {
    public WorksetId Id { get; }
    public string Name { get; }

    public WorksetInfo(WorksetId id, string name) {
        Id = id;
        Name = name;
    }
}
