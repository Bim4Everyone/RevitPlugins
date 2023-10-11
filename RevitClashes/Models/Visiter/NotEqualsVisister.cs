
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Visiter {
    internal class NotEqualsVisister : IVisiter {
        public FilterRule Create(ElementId paramId, int value) {
            return ParameterFilterRuleFactory.CreateNotEqualsRule(paramId, value);
        }

        public FilterRule Create(ElementId paramId, double value) {
            return ParameterFilterRuleFactory.CreateNotEqualsRule(paramId, value, 0.001);
        }

        public FilterRule Create(ElementId paramId, string value) {
#if REVIT_2022_OR_LESS
            return ParameterFilterRuleFactory.CreateNotEqualsRule(paramId, value, false);
#else
            return ParameterFilterRuleFactory.CreateNotEqualsRule(paramId, value);
#endif
        }

        public FilterRule Create(ElementId paramId, ElementId value) {
            return ParameterFilterRuleFactory.CreateNotEqualsRule(paramId, value);
        }
    }
}
