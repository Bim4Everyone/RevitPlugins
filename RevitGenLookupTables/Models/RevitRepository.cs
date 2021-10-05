using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitGenLookupTables.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);
        }

        public string DocumentName {
            get { return _document.Title; }
        }

        public Family GetMainFamily() {
            if(!_document.IsFamilyDocument) {
                throw new InvalidOperationException("Документ должен быть семейством.");
            }

            return _document.OwnerFamily;
        }

        public IList<FamilyParameter> GetFamilyParams() {
            return _document.FamilyManager.GetParameters();
        }
    }
}
