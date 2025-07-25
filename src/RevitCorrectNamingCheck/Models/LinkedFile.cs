using Autodesk.Revit.DB;

namespace RevitCorrectNamingCheck.Models;

internal class LinkedFile {
    public ElementId Id { get; set; }
    public string Name { get; set; }
    public bool IsPinned { get; set; }
    public NameStatus FileNameStatus { get; set; }
    public WorksetInfo TypeWorkset { get; set; }
    public WorksetInfo InstanceWorkset { get; set; }
    public LinkedFile(ElementId id, string name, bool isPinned, WorksetInfo typeWorkset, WorksetInfo instanceWorkset) {
        Id = id;
        Name = name;
        IsPinned = isPinned;
        TypeWorkset = typeWorkset;
        InstanceWorkset = instanceWorkset;
    }
}
