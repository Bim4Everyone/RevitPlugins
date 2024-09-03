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


        private List<ElementId> GetSharedVisNameCategories() {
            (Definition Definition, Binding Binding) sharedVisNameParam = _document.GetSharedParamBinding(_config.SharedVisSystemName);
            Binding parameterBinding = sharedVisNameParam.Binding;
            IEnumerable<Category> sharedVisNameCategories = parameterBinding.GetCategories();

            return new List<ElementId>(
                sharedVisNameCategories.Select(category => new ElementId(category.GetBuiltInCategory())));
        }



        private List<ElementId> GetCategories() {
            return UseSharedSystemName ? 
                GetSharedVisNameCategories() : AxonometryConfig.SystemCategories
                    .Select(category => new ElementId(category))
                    .ToList();
        }

        public string GetSystemName(HvacSystemViewModel hvacSystem) {
            return UseSharedSystemName ? hvacSystem.SharedName : hvacSystem.SystemName;
        }
    }
}
