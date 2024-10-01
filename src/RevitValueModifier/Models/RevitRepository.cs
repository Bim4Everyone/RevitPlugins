using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
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

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<RevitElement> GetRevitElements() => ActiveUIDocument
                .Selection
                .GetElementIds()
                .Select(id => Document.GetElement(id))
                .Select(e => new RevitElement(e, _localizationService))
                .ToList();

        internal List<ElementId> GetCategoryIds(List<RevitElement> revitElements) => revitElements
                .Select(rE => rE.Elem.Category.Id)
                .ToList();

        internal List<RevitParameter> GetParams(List<ElementId> categoryIds) => ParameterFilterUtilities
            .GetFilterableParametersInCommon(Document, categoryIds)
            .Select(id => new RevitParameter(id, Document, _localizationService))
            .OrderBy(rP => rP.ParamName)
            .ToList();
    }
}
