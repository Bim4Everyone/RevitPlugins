using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.ViewModels;

using RevitMarkPlacement.ViewModels;

namespace RevitMarkPlacement.Models {
    internal class RevitRepository : BaseViewModel {
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

        public IEnumerable<string> GetSpotDimentionTypeNames(ISelectionMode selectionMode) {
            return selectionMode.GetSpotDimentions(_document)
                .Select(s => s.SpotDimensionType.Name)
                .Distinct();
        }

        public IEnumerable<GlobalParameterViewModel> GetGlobalParameters() {
            return GlobalParametersManager
                .GetGlobalParametersOrdered(_document)
                .Select(id=>_document.GetElement(id))
                .Cast<GlobalParameter>()
                .Where(p=>p.GetValue() is DoubleParameterValue)
                .Select(p => new GlobalParameterViewModel(p.Name, Math.Round(UnitUtils.ConvertFromInternalUnits((p.GetValue() as DoubleParameterValue).Value, UnitTypeId.Millimeters))));
        }
    }
}
