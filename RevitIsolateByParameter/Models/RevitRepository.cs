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

        //public void CreateFilter(ElementId parameter) 
        //{
        //    List<ElementId> categories = new List<ElementId>();
        //    categories.Add(Category.GetCategory(Document, BuiltInCategory.OST_Walls).Id);

        //    string value = "1";
        //    FilterRule filterRule = ParameterFilterRuleFactory.CreateEqualsRule(parameter, value, true);
        //    ElementParameterFilter elemParamFilter = new ElementParameterFilter(filterRule);

        //    using(Transaction t = Document.StartTransaction("Создать фильтры")) 
        //    {
        //        ParameterFilterElement.Create(Document, "Test_filter", categories, elemParamFilter);
        //        t.Commit();
        //    }
        //}

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

        public async Task IsolateElements(List<ElementId> filteredElements) {
            for(int i = 0; i < 2; i++) {
                _revitEventHandler.TransactAction = () => {
                    ActiveUIDocument.Selection.SetElementIds(filteredElements);
                    //var commandId = RevitCommandId.LookupPostableCommandId(PostableCommand.HideElements);
                    var commandId = RevitCommandId.LookupCommandId("ID_TEMPHIDE_HIDE");
                    if(!(commandId is null) && ActiveUIDocument.Application.CanPostCommand(commandId)) {
                        UIApplication.PostCommand(commandId);
                    }
                };
                await _revitEventHandler.Raise();
            }
        }

        public ObservableCollection<ParameterElement> GetParameters() {
            ObservableCollection<ParameterElement> parameters = new ObservableCollection<ParameterElement>();

            parameters.Add(SharedParamsConfig.Instance.BuildingWorksLevel.GetRevitParamElement(Document));
            parameters.Add(SharedParamsConfig.Instance.BuildingWorksSection.GetRevitParamElement(Document));
            parameters.Add(SharedParamsConfig.Instance.BuildingWorksBlock.GetRevitParamElement(Document));
         
            return parameters;
        }

        public ObservableCollection<string> GetParameterValues(ParameterElement parameter) {
            View activeView = Document.ActiveView;
            IList<Element> elements = new FilteredElementCollector(Document, activeView.Id).ToElements();
            string paramName = parameter.GetDefinition().Name;

            List<string> values = elements
                .Where(x => x.IsExistsParam(paramName))
                .Select(x => (string)x.GetParamValue(paramName))
                .Where(x => x != null)
                .Distinct()
                .ToList();

            ObservableCollection<string> parameterValues = new ObservableCollection<string>(values.OrderBy(i => i));

            return parameterValues;
        }
    }
}