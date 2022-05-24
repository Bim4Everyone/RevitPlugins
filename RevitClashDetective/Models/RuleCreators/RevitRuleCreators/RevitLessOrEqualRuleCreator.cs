using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RuleCreators.RevitRuleCreators {
    internal class RevitLessOrEqualRuleCreator : IRevitRuleCreator {
        public FilterRule Create(StorageType storageType, ElementId paramId, object value) {
            switch(storageType) {
                case StorageType.Integer: {
                    return ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, (int) value);
                }
                case StorageType.Double: {
                    return ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, (double) value, 0.001);
                }
                case StorageType.String: {
                    return ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, (string) value, false);
                }
                case StorageType.ElementId: {
                    return ParameterFilterRuleFactory.CreateLessOrEqualRule(paramId, (ElementId) value);
                }
                default: {
                    throw new ArgumentException(nameof(storageType), $"У параметра с id = {paramId} не удалось опредеить тип данных.");
                }
            }
        }
    }
}
