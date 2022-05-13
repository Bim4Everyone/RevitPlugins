using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RuleCreators.RevitRuleCreators {
    internal class RevitEqualsRuleCreator : IRevitRuleCreator {
        public FilterRule Create(StorageType storageType, ElementId paramId, object value) {
            switch(storageType) {
                case StorageType.Integer: {
                    return ParameterFilterRuleFactory.CreateEqualsRule(paramId, (int) value);
                }
                case StorageType.Double: {
                    return ParameterFilterRuleFactory.CreateEqualsRule(paramId, (double) value, 0.001);
                }
                case StorageType.String: {
                    return ParameterFilterRuleFactory.CreateEqualsRule(paramId, (string) value, false);
                }
                case StorageType.ElementId: {
                    return ParameterFilterRuleFactory.CreateEqualsRule(paramId, (ElementId) value);
                }
                default: {
                    throw new Exception("");
                }
            }
        }
    }
}
