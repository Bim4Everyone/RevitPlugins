using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;

namespace RevitSetLevelSection.Models {
    internal class LinkInstanceRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;

        public LinkInstanceRepository(Application application, RevitLinkInstance linkInstance) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = linkInstance.GetLinkDocument();
            Transform = linkInstance.GetTransform();
        }

        public Transform Transform { get; }

        public IEnumerable<DesignOption> GetDesignOptions() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(DesignOption))
                .OfType<DesignOption>()
                .ToList();
        }

        public IEnumerable<FamilyInstance> GetMassElements(DesignOption designOption) {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Mass)
                .Where(item => item.DesignOption?.Id == designOption.Id)
                .OfType<FamilyInstance>()
                .ToList();
        }

        public RevitParam GetPartParam(string paramName) {
            return _document.GetProjectParams()
                .Where(item => item.Name.StartsWith(paramName, StringComparison.CurrentCultureIgnoreCase))
                .Select(item => ProjectParamsConfig.Instance.CreateRevitParam(_document, item))
                .FirstOrDefault();
        }

        public IEnumerable<RevitParam> GetPartParams(IEnumerable<string> paramNames) {
            return paramNames.Select(item => GetPartParam(item)).Where(item => item != null);
        }
    }
}