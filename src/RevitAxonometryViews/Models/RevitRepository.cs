using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Windows.Data;
using System.Windows.Forms;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitAxonometryViews.ViewModels;

using Application = Autodesk.Revit.ApplicationServices.Application;
using Binding = Autodesk.Revit.DB.Binding;

namespace RevitAxonometryViews.Models {
    internal class RevitRepository {
        private readonly AxonometryConfig _axonometryConfig;
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
            _axonometryConfig = new AxonometryConfig(this);
        }
        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public AxonometryConfig AxonometryConfig => _axonometryConfig;

        /// <summary>
        /// Получаем ФОП_ВИС_Имя системы если оно есть в проекте
        /// </summary>
        public string GetSharedSystemName(Element system) {
            ElementSet elementSet = null;
            if(system.Category.IsId(BuiltInCategory.OST_DuctSystem)) {
                elementSet = ((MechanicalSystem)system).DuctNetwork;
            }
            if(system.Category.IsId(BuiltInCategory.OST_PipingSystem)) {
                elementSet = ((PipingSystem)system).PipingNetwork;
            }
            // Нужно перебирать элементы пока не встретим заполненный параметр, могут быть не до конца обработаны элементы.
            // Выбрасываем первое встреченное заполненное значение
            if(elementSet != null && !elementSet.IsEmpty) {
                foreach(Element element in elementSet) {
                    string result = element.GetSharedParamValueOrDefault<string>(AxonometryConfig.SharedVisSystemName, null);
                    if(result != null) {
                        return result;
                    }
                }
            }
            return "Нет имени";
        }

        /// <summary>
        /// Транзакция с созданием видов через класс ViewFactory
        /// </summary>
        public void ExecuteViewCreation(List<HvacSystemViewModel> hvacSystems, CreationViewRules creationViewRules) {
            ViewFactory viewFactory = new ViewFactory(Document, ActiveUIDocument, creationViewRules);
            using(Transaction t = Document.StartTransaction("Создать схемы")) {
                viewFactory.CreateViewsBySelectedSystems(hvacSystems);
                t.Commit();
            }
        }
    }
}
