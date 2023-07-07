using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitIsolateByParameter.Handlers;
using RevitIsolateByParameter.ViewModels;

namespace RevitIsolateByParameter.Models {
    internal class RevitRepository {

        private const string ParameterNoValueText = "<Параметр не заполнен>";

        private readonly RevitEventHandler _revitEventHandler;

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;

            _revitEventHandler = new RevitEventHandler();
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<ElementId> GetFilteredElements(ParamViewModel parameter, string selectedValue) {
            if(selectedValue == ParameterNoValueText)
                selectedValue = null;

            return new FilteredElementCollector(Document, Document.ActiveView.Id)
                .ToElements()
                .Where(x => x.IsExistsParam(parameter.Param))
                .Where(x => x.GetParamValue<string>(parameter.Param) == selectedValue)
                .Select(x => x.Id)
                .ToList();
        }

        public async Task IsolateElements(ParamViewModel parameter, string selectedValue) {
            _revitEventHandler.TransactAction = () => {

                using(Transaction t = Document.StartTransaction("Изолировать элементы")) {
                    Document.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);

                    List<ElementId> filteredElements = GetFilteredElements(parameter, selectedValue);
                    Document.ActiveView.IsolateElementsTemporary(filteredElements);
                    t.Commit();
                }
            };
            await _revitEventHandler.Raise();
        }
    }
}