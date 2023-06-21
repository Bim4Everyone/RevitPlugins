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

        private readonly RevitEventHandler _revitEventHandler;

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;

            _revitEventHandler = new RevitEventHandler();
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<ElementId> GetFilteredElements(ParameterElement parameter, string selectedValue) {
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

        public async Task IsolateElements(ParameterElement parameter, string selectedValue) {
                _revitEventHandler.TransactAction = () => {
                    Transaction tr = new Transaction(Document);
                    tr.Start("Hide elements");
                    Document.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                    List<ElementId> filteredElements = GetFilteredElements(parameter, selectedValue);
                    Document.ActiveView.IsolateElementsTemporary(filteredElements);
                    tr.Commit();
                };
                await _revitEventHandler.Raise();
        }

        public ObservableCollection<ParameterElement> GetParameters() {
            ObservableCollection<ParameterElement> parameters = new ObservableCollection<ParameterElement>();

            parameters.Add(SharedParamsConfig.Instance.BuildingWorksLevel.GetRevitParamElement(Document));
            parameters.Add(SharedParamsConfig.Instance.BuildingWorksSection.GetRevitParamElement(Document));
            parameters.Add(SharedParamsConfig.Instance.BuildingWorksBlock.GetRevitParamElement(Document));
         
            return parameters;
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
                    .Where(x => x != null)
                    .Distinct()
                    .OrderBy(i => i)
                    .ToList();

                parametersValues.Add(paramName, values);
            }

            return parametersValues;
        }
    }
}