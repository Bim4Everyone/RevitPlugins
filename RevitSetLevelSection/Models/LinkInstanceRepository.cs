using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

namespace RevitSetLevelSection.Models {
    internal class LinkInstanceRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        
        public LinkInstanceRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
        }
        
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
                .Where(item => item.DesignOption.Id == designOption.Id)
                .OfType<FamilyInstance>()
                .ToList();
        }
    }
}