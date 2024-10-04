using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;

namespace RevitValueModifier.Models {
    internal class RevitRepository {
        private readonly ILocalizationService _localizationService;

        public RevitRepository(UIApplication uiApplication, ILocalizationService localizationService) {
            UIApplication = uiApplication;
            _localizationService = localizationService;
        }

        public UIApplication UIApplication { get; }

        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Document Document => ActiveUIDocument.Document;

        public List<RevitElement> GetRevitElements() => ActiveUIDocument
                .Selection
                .GetElementIds()
                .Select(id => Document.GetElement(id))
                .Select(e => new RevitElement(e, _localizationService))
                .ToList();

        internal List<ElementId> GetCategoryIds(List<RevitElement> revitElements) => revitElements
                .Select(rE => rE.Elem.Category.Id)
                .Distinct()
                .ToList();

        internal List<RevitParameter> GetParamsForRead(List<ElementId> categoryIds) => ParameterFilterUtilities
            .GetFilterableParametersInCommon(Document, categoryIds)
            .Select(id => new RevitParameter(id, Document))
            .OrderBy(rP => rP.ParamName)
            .ToList();

        internal List<RevitParameter> GetParamsForWrite(List<RevitParameter> parameters) => parameters
            .Where(p => p.ParamStorageType == StorageType.String)
            .ToList();
    }
}
