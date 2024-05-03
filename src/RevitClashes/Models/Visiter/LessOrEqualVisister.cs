using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.Visiter {
    internal class LessOrEqualVisister : IVisiter {
        public FilterRule Create(ElementId paramId, int value) {
            return ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, value);
        }

        public FilterRule Create(ElementId paramId, double value) {
            return ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, value, 0.001);
        }

        public FilterRule Create(ElementId paramId, string value) {
#if REVIT_2022_OR_LESS
            return ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, value, false);
#else
            return ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, value);
#endif 
        }

        public FilterRule Create(ElementId paramId, ElementId value) {
            throw new NotImplementedException();
        }
    }
}
