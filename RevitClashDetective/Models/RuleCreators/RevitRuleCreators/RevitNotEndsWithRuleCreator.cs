using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RuleCreators.RevitRuleCreators {
    internal class RevitNotEndsWithRuleCreator : IRevitRuleCreator {
        public FilterRule Create(StorageType storageType, ElementId paramId, object value) {
            switch(storageType) {
                case StorageType.String: {
                    return ParameterFilterRuleFactory.CreateNotEndsWithRule(paramId, (string) value, false);
                }
                case StorageType.ElementId: {
                    return ParameterFilterRuleFactory.CreateNotEndsWithRule(paramId, (string) value, false);
                }
                default: {
                    throw new ArgumentException(nameof(RevitContainsRuleCreator), $"У параметра с id = {paramId} не строковой тип данных.");
                }
            }
        }
    }
}
