using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Visiter {
    internal class ContainsVisiter : IVisiter {
        public FilterRule Create(ElementId paramId, int value) {
            throw new NotImplementedException();
        }

        public FilterRule Create(ElementId paramId, double value) {
            throw new NotImplementedException();
        }

        public FilterRule Create(ElementId paramId, string value) {
#if REVIT_2022_OR_LESS
            return ParameterFilterRuleFactory.CreateContainsRule(paramId, value, false);
#else
            return ParameterFilterRuleFactory.CreateContainsRule(paramId, value);
#endif
        }

        public FilterRule Create(ElementId paramId, ElementId value) {
            throw new NotImplementedException();
        }
    }
}
