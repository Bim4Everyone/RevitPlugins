using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RuleCreators.RevitRuleCreators {
    internal class RevitNotContainsRuleCreator : IRevitRuleCreator {
        public FilterRule Create(StorageType storageType, ElementId paramId, object value) {
            switch(storageType) {
                case StorageType.String: {
                    return ParameterFilterRuleFactory.CreateNotContainsRule(paramId, (string) value, false);
                }
                default: {
                    throw new ArgumentException(nameof(RevitContainsRuleCreator), $"У параметра с id = {paramId} не строковой тип данных.");
                }
            }
        }
    }
}
