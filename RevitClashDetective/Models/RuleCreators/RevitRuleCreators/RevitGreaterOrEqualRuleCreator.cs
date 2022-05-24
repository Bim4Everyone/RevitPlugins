using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RuleCreators.RevitRuleCreators {
    internal class RevitGreaterOrEqualRuleCreator : IRevitRuleCreator {
        public FilterRule Create(StorageType storageType, ElementId paramId, object value) {
            switch(storageType) {
                case StorageType.Integer: {
                    return ParameterFilterRuleFactory.CreateGreaterOrEqualRule(paramId, (int) value);
                }
                case StorageType.Double: {
                    return ParameterFilterRuleFactory.CreateGreaterOrEqualRule(paramId, (double) value, 0.001);
                }
                case StorageType.String: {
                    return ParameterFilterRuleFactory.CreateGreaterOrEqualRule(paramId, (string) value, false);
                }
                case StorageType.ElementId: {
                    return ParameterFilterRuleFactory.CreateGreaterOrEqualRule(paramId, (ElementId) value);
                }
                default: {
                    throw new ArgumentException(nameof(storageType), $"У параметра с id = {paramId} не удалось опредеить тип данных.");
                }
            }
        }
    }
}
