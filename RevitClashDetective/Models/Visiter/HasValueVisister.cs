
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Visiter {
    internal class HasValueVisister : IVisiter {
        public FilterRule Create(ElementId paramId, int value) {
            return ParameterFilterRuleFactory.CreateHasValueParameterRule(paramId);
        }

        public FilterRule Create(ElementId paramId, double value) {
            return ParameterFilterRuleFactory.CreateHasValueParameterRule(paramId);
        }

        public FilterRule Create(ElementId paramId, string value) {
            return ParameterFilterRuleFactory.CreateHasValueParameterRule(paramId);
        }

        public FilterRule Create(ElementId paramId, ElementId value) {
            return ParameterFilterRuleFactory.CreateHasValueParameterRule(paramId);
        }
    }
}
