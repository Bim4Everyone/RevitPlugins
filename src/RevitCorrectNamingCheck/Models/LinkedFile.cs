using Autodesk.Revit.DB;

namespace RevitCorrectNamingCheck.Models;

internal class LinkedFile {
    public ElementId Id { get; set; }
    public string Name { get; set; }
    public NameStatus FileNameStatus { get; set; }
    public WorksetInfo TypeWorkset { get; set; }
    public WorksetInfo InstanceWorkset { get; set; }

    public LinkedFile(ElementId id, string name, WorksetInfo typeWorkset, WorksetInfo instanceWorkset) {
        Id = id;
        Name = name;
        TypeWorkset = typeWorkset;
        InstanceWorkset = instanceWorkset;
    }
}
