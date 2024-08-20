using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.Models.Interfaces {
    interface IVisiter {
        FilterRule Create(ElementId paramId, int value);
        FilterRule Create(ElementId paramId, double value);
        FilterRule Create(ElementId paramId, string value);
        FilterRule Create(ElementId paramId, ElementId value);
    }

    interface IClashesLoader {
        string FilePath { get; }
        bool IsValid();
        IEnumerable<ClashModel> GetClashes();
    }
}
