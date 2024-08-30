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
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }
        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;




        /// <summary>
        /// Если существует ФОП_ВИС_Имя системы, проверяет во всех ли нужных категориях он. Если нет - возвращает в строке в каких нет,
        /// если все окей - возвращает null
        /// </summary>
        /// <returns></returns>
        public string CheckVisNameCategories() {
            if(!Document.IsExistsSharedParam(AxonometryConfig.FopVisSystemName)) {
                return $"Параметр {"ФОП_ВИС_Имя системы"} не существует в проекте.";
            } else {
                (Definition Definition, Binding Binding) fopVisNameParam = Document.GetSharedParamBinding(AxonometryConfig.FopVisSystemName);
                Binding parameterBinding = fopVisNameParam.Binding;
                IEnumerable<Category> fopVisNameCategories = parameterBinding.GetCategories();

                HashSet<BuiltInCategory> builtInCategories = new HashSet<BuiltInCategory>(
                    fopVisNameCategories.Select(category => category.GetBuiltInCategory())
                );

                List<string> missingCategories = AxonometryConfig.SystemCategories
                    .Where(builtInCategory => !builtInCategories.Contains(builtInCategory))
                    .Select(builtInCategory => Category.GetCategory(Document, builtInCategory).Name)
                    .ToList();

                if(missingCategories.Any()) {
                    string result = $"Параметр {AxonometryConfig.FopVisSystemName} не назначен для категорий: ";
                    result += string.Join(", ", missingCategories);
                    return result;
                }
                return null;
            }
        }

        /// <summary>
        /// Получаем ФОП_ВИС_Имя системы если оно есть в проекте
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public string GetSystemFopName(Element system) {
            ElementSet elementSet = new ElementSet();
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
                    string result = element.GetSharedParamValueOrDefault<string>(AxonometryConfig.FopVisSystemName, null);
                    if(result != null) {
                        return result;
                    }
                }
            }
            return "Нет имени";
        }

        /// <summary>
        /// Создаем коллекцию объектов систем с именами для создания по ним фильтров
        /// </summary>
        /// <returns></returns>
        public List<HvacSystemViewModel> GetHvacSystems() {
            
            List<Element> ductSystems = Document.GetElementsByCategory(BuiltInCategory.OST_DuctSystem);
            List<Element> pipeSystems = Document.GetElementsByCategory(BuiltInCategory.OST_PipingSystem);
            List<Element> allSystems = ductSystems.Concat(pipeSystems).ToList();

            List<HvacSystemViewModel> newSystems = new List<HvacSystemViewModel>();

            return new List<HvacSystemViewModel>(
                allSystems.Select(
                    system => new HvacSystemViewModel (system.Name, GetSystemFopName(system))));
        }


        /// <summary>
        /// Транзакция с созданием видов через класс ViewFactory
        /// </summary>
        /// <returns></returns>
        public void ExecuteViewCreation(List<HvacSystemViewModel> hvacSystems, CreationViewRules creationViewRules) {
            ViewFactory viewFactory = new ViewFactory(Document, ActiveUIDocument, creationViewRules);
            using(Transaction t = Document.StartTransaction("Создать схемы")) {
                viewFactory.CreateViewsBySelectedSystems(hvacSystems);
                t.Commit();
            }
        }
    }
}
