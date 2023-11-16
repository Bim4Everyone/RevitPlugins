using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Visiter {
    internal class LessVisister : IVisiter {
        public FilterRule Create(ElementId paramId, int value) {
            return ParameterFilterRuleFactory.CreateLessRule(paramId, value);
        }

        public FilterRule Create(ElementId paramId, double value) {
            return ParameterFilterRuleFactory.CreateLessRule(paramId, value, 0.001);
        }

        public FilterRule Create(ElementId paramId, string value) {
#if REVIT_2022_OR_LESS
            return ParameterFilterRuleFactory.CreateLessRule(paramId, value, false);
#else
            return ParameterFilterRuleFactory.CreateLessRule(paramId, value);
#endif
        }

        public FilterRule Create(ElementId paramId, ElementId value) {
            throw new NotImplementedException();
        }
    }
}
