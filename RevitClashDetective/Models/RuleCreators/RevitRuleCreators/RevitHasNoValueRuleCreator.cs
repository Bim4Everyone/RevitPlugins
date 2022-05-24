using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RuleCreators.RevitRuleCreators {
    internal class RevitHasNoValueRuleCreator : IRevitRuleCreator {
        public FilterRule Create(StorageType storageType, ElementId paramId, object value) {
            switch(storageType) {
                case StorageType.Integer: {
                    return ParameterFilterRuleFactory.CreateHasNoValueParameterRule(paramId);
                }
                case StorageType.Double: {
                    return ParameterFilterRuleFactory.CreateHasNoValueParameterRule(paramId);
                }
                case StorageType.String: {
                    return ParameterFilterRuleFactory.CreateHasNoValueParameterRule(paramId);
                }
                case StorageType.ElementId: {
                    return ParameterFilterRuleFactory.CreateHasNoValueParameterRule(paramId);
                }
                default: {
                    throw new ArgumentException(nameof(storageType), $"У параметра с id = {paramId} не удалось опредеить тип данных.");
                }
            }
        }
    }
}
