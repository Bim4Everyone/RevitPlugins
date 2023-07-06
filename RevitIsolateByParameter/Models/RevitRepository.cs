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

        public ObservableCollection<SharedParam> GetParameters() {
            ObservableCollection<SharedParam> parameters = new ObservableCollection<SharedParam>();

            if(SharedParamsConfig.Instance.BuildingWorksLevel.IsExistsParam(Document))
                parameters.Add(SharedParamsConfig.Instance.BuildingWorksLevel);
            if(SharedParamsConfig.Instance.BuildingWorksSection.IsExistsParam(Document)) 
                parameters.Add(SharedParamsConfig.Instance.BuildingWorksSection);
            if(SharedParamsConfig.Instance.BuildingWorksBlock.IsExistsParam(Document)) 
                parameters.Add(SharedParamsConfig.Instance.BuildingWorksBlock);

            return parameters;
        }

        public List<ElementId> GetFilteredElements(SharedParam parameter, string selectedValue) {
            if(selectedValue == ParameterNoValueText)
                selectedValue = null;
            View activeView = Document.ActiveView;
            IList<Element> elements = new FilteredElementCollector(Document, activeView.Id).ToElements();

            List<ElementId> filteredElements = elements
                .Where(x => x.IsExistsParam(parameter))
                .Where(x => x.GetParamValue<string>(parameter) == selectedValue)
                .Select(x => x.Id)
                .ToList();

            return filteredElements;
        }

        public Dictionary<string, List<string>> GetParameterValues(ObservableCollection<SharedParam> parameters) {
            View activeView = Document.ActiveView;
            IList<Element> elements = new FilteredElementCollector(Document, activeView.Id).ToElements();
            
            Dictionary<string, List<string>> parametersValues = new Dictionary<string, List<string>>();

            foreach(var parameter in parameters) { 

                List<string> values = elements
                    .Where(x => x.IsExistsParam(parameter))
                    .Select(x => x.GetParamValue<string>(parameter))
                    .Select(x => x ?? ParameterNoValueText)
                    .Distinct()
                    .OrderBy(i => i)
                    .ToList();

                parametersValues.Add(parameter.Name, values);
            }

            return parametersValues;
        }

        public async Task IsolateElements(SharedParam parameter, string selectedValue) {
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