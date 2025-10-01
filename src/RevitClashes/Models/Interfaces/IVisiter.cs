using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.RevitClashReport;

namespace RevitClashDetective.Models.Interfaces;
internal interface IVisiter {
    FilterRule Create(ElementId paramId, int value);
    FilterRule Create(ElementId paramId, double value);
    FilterRule Create(ElementId paramId, string value);
    FilterRule Create(ElementId paramId, ElementId value);
}

internal interface IClashesLoader {
    string FilePath { get; }
    bool IsValid();
    IEnumerable<ReportModel> GetReports();
}
