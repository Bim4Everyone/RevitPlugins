using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCorrectNamingCheck.Models;

internal class LinkedFile {
    public RevitLinkInstance Instance { get; }
    public string Name { get; }
    public bool IsPinned { get; set; }
    public WorksetInfo TypeWorkset { get; set; }
    public WorksetInfo InstanceWorkset { get; set; }

    public LinkedFile(RevitLinkInstance instance, WorksetInfo typeWorkset, WorksetInfo instanceWorkset) {
        Instance = instance;
        Name = instance.Name;
        IsPinned = instance.Pinned;
        TypeWorkset = typeWorkset;
        InstanceWorkset = instanceWorkset;
    }
}
