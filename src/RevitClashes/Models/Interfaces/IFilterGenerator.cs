using Autodesk.Revit.DB;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models.Interfaces {
    interface IFilterGenerator {
        IFilterGenerator SetSetFilter(Document doc, Set set);
        IFilterGenerator SetRuleFilter(Document doc, Rule rule);
    }
}
