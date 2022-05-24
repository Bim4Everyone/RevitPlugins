using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RuleCreators.RevitRuleCreators {
    internal class RevitHasValueRuleCreator : IRevitRuleCreator {
        public FilterRule Create(StorageType storageType, ElementId paramId, object value) {
            switch(storageType) {
                case StorageType.Integer: {
                    return ParameterFilterRuleFactory.CreateHasValueParameterRule(paramId);
                }
                case StorageType.Double: {
                    return ParameterFilterRuleFactory.CreateHasValueParameterRule(paramId);
                }
                case StorageType.String: {
                    return ParameterFilterRuleFactory.CreateHasValueParameterRule(paramId);
                }
                case StorageType.ElementId: {
                    return ParameterFilterRuleFactory.CreateHasValueParameterRule(paramId);
                }
                default: {
                    throw new ArgumentException(nameof(storageType), $"У параметра с id = {paramId} не удалось опредеить тип данных.");
                }
            }
        }
    }
}
