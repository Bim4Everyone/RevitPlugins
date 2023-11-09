using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Interfaces {
    interface IRevitRuleCreator {
        FilterRule Create(StorageType storageType, ElementId paramId, object value);
    }
}
