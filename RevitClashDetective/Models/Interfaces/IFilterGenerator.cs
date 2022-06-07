using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.FilterableValueProviders;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models.Interfaces {
    interface IFilterGenerator {
        IFilterGenerator SetSetFilter(Document doc, Set set);
        IFilterGenerator SetRuleFilter(Document doc, Rule rule);
    }
}
