using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Visiter {
    internal class GreaterVisister : IVisiter {
        public FilterRule Create(ElementId paramId, int value) {
            return ParameterFilterRuleFactory.CreateGreaterRule(paramId, value);
        }

        public FilterRule Create(ElementId paramId, double value) {
            return ParameterFilterRuleFactory.CreateGreaterRule(paramId, value, 0.001);
        }

        public FilterRule Create(ElementId paramId, string value) {
#if REVIT_2022_OR_LESS
            return ParameterFilterRuleFactory.CreateGreaterRule(paramId, value, false);
#else
            return ParameterFilterRuleFactory.CreateGreaterRule(paramId, value);
#endif
        }

        public FilterRule Create(ElementId paramId, ElementId value) {
            throw new NotImplementedException();
        }
    }
}
