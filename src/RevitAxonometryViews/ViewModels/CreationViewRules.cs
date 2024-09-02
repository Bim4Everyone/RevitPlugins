using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitAxonometryViews.Models;

namespace RevitAxonometryViews.ViewModels {
    internal class CreationViewRules {
        private readonly Document _document;
        private readonly AxonometryConfig _config;

        public CreationViewRules(string selectedCriterion, bool isCombined, RevitRepository revitRepository) {
            _document = revitRepository.Document;
            _config = revitRepository.AxonometryConfig;
            IsCombined = isCombined;
            IsSingle = !isCombined;

            UseSharedSystemName = selectedCriterion == _config.SharedVisSystemName;
            UseSystemName = !UseSharedSystemName;

            Categories = GetCategories();
            FilterParameter = GetFilterParameter();
        }

        public bool IsSingle { get; }
        public bool IsCombined { get; }
        public bool UseSharedSystemName { get; }
        public bool UseSystemName { get; }
        public List<ElementId> Categories { get; }
        public ElementId FilterParameter { get; }

        private ElementId GetFilterParameter() {
            return UseSharedSystemName
                ? _config.SystemSharedNameParam.Id
                : new ElementId(_config.SystemNameBuiltInParam);
        }

        private List<ElementId> GetCategories() {
            var categories = AxonometryConfig.SystemCategories
                .Select(category => new ElementId(category))
                .ToList();

            if(!UseSharedSystemName) {
                categories.Remove(new ElementId(BuiltInCategory.OST_GenericModel));
            }

            return categories;
        }

        public string GetSystemName(HvacSystemViewModel hvacSystem) {
            return UseSharedSystemName ? hvacSystem.SharedName : hvacSystem.SystemName;
        }
    }
}
