using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitAxonometryViews.Models;

namespace RevitAxonometryViews.ViewModels {
    internal class CreationViewRules {
        private readonly Document _document;

        public CreationViewRules(bool isCombined, bool useSharedSystemName, Document document) {
            _document = document;
            IsCombined = isCombined;
            IsSingle = !isCombined;
            UseSharedSystemName = useSharedSystemName;
            UseSystemName = !useSharedSystemName;

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
                ? _document.GetSharedParam(AxonometryConfig.SharedVisSystemName).Id
                : new ElementId(BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
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
