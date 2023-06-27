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

        public ObservableCollection<ParameterElement> GetParameters() {
            ObservableCollection<ParameterElement> parameters = new ObservableCollection<ParameterElement>();

            if(SharedParamsConfig.Instance.BuildingWorksLevel.IsExistsParam(Document))
                parameters.Add(SharedParamsConfig.Instance.BuildingWorksLevel.GetRevitParamElement(Document));
            if(SharedParamsConfig.Instance.BuildingWorksSection.IsExistsParam(Document)) 
                parameters.Add(SharedParamsConfig.Instance.BuildingWorksSection.GetRevitParamElement(Document));
            if(SharedParamsConfig.Instance.BuildingWorksBlock.IsExistsParam(Document)) 
                parameters.Add(SharedParamsConfig.Instance.BuildingWorksBlock.GetRevitParamElement(Document));

            return parameters;
        }

        public List<ElementId> GetFilteredElements(ParameterElement parameter, string selectedValue) {
            if(selectedValue == ParameterNoValueText)
                selectedValue = null;
            View activeView = Document.ActiveView;
            IList<Element> elements = new FilteredElementCollector(Document, activeView.Id).ToElements();
            string paramName = parameter.GetDefinition().Name;

            List<ElementId> filteredElements = elements
                .Where(x => x.IsExistsParam(paramName))
                .Where(x => (string) x.GetParamValue(paramName) == selectedValue)
                .Select(x => x.Id)
                .ToList();

            return filteredElements;
        }

        public Dictionary<string, List<string>> GetParameterValues(ObservableCollection<ParameterElement> parameters) {
            View activeView = Document.ActiveView;
            IList<Element> elements = new FilteredElementCollector(Document, activeView.Id).ToElements();
            
            Dictionary<string, List<string>> parametersValues = new Dictionary<string, List<string>>();

            foreach(var parameter in parameters) { 
                string paramName = parameter.GetDefinition().Name;

                List<string> values = elements
                    .Where(x => x.IsExistsParam(paramName))
                    .Select(x => (string)x.GetParamValue(paramName))
                    .Select(x => x ?? ParameterNoValueText)
                    .Distinct()
                    .OrderBy(i => i)
                    .ToList();

                parametersValues.Add(paramName, values);
            }

            return parametersValues;
        }

        public async Task IsolateElements(ParameterElement parameter, string selectedValue) {
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