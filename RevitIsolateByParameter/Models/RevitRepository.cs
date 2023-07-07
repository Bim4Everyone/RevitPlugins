using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitIsolateByParameter.Handlers;
using RevitIsolateByParameter.ViewModels;

namespace RevitIsolateByParameter.Models {
    internal class RevitRepository {
        private readonly RevitEventHandler _revitEventHandler;

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;

            _revitEventHandler = new RevitEventHandler();
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public IList<ElementId> GetFilteredElements(ParamViewModel parameter, ParamValue selectedValue) {
            return new FilteredElementCollector(Document, Document.ActiveView.Id)
                .ToElements()
                .Where(x => x.IsExistsParam(parameter.Param))
                .Where(x => x.GetParamValue<string>(parameter.Param) == selectedValue.Value)
                .Select(x => x.Id)
                .ToList();
        }

        public async Task IsolateElements(ParamViewModel parameter, ParamValue selectedValue) {
            _revitEventHandler.TransactAction = () => {

                using(Transaction t = Document.StartTransaction("Изолировать элементы")) {
                    Document.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);

                    IList<ElementId> filteredElements = GetFilteredElements(parameter, selectedValue);
                    Document.ActiveView.IsolateElementsTemporary(filteredElements);
                    t.Commit();
                }
            };
            await _revitEventHandler.Raise();
        }
    }
}