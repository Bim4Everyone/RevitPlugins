using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitClashDetective.Models.FilterableValueProviders;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models.Interfaces {
    interface IFilterGenerator {
        IFilterGenerator SetSetFilter(Set set);
        IFilterGenerator SetRuleFilter(Rule rule);
    }
}
