using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return ParameterFilterRuleFactory.CreateContainsRule(paramId, value, false);
        }

        public FilterRule Create(ElementId paramId, ElementId value) {
            throw new NotImplementedException();
        }
    }
}
