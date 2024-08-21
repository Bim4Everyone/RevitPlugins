using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitAxonometryViews.ViewModels;

namespace RevitAxonometryViews.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        //Если существует ФОП_ВИС_Имя системы, проверяет во всех ли нужных категориях он. Если нет - возвращает в строке в каких нет,
        //если все окей - возвращает null
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

        //Получаем ФОП_ВИС_Имя системы если оно есть в проекте
        public string GetSystemFopName(Element system) {
            ElementSet elementSet = new ElementSet();
            if(system.Category.IsId(BuiltInCategory.OST_DuctSystem)) {
                elementSet = (system as MechanicalSystem).DuctNetwork;
            }
            if(system.Category.IsId(BuiltInCategory.OST_PipingSystem)) {
                elementSet = (system as PipingSystem).PipingNetwork;
            }
            //Нужно перебирать элементы пока не встретим заплненный параметр, могут быть не до конца обработаны элементы.
            //Выбрасываем первое встреченное заполненное значение
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

        public List<Element> GetCollection(BuiltInCategory category) {
            return new FilteredElementCollector(Document)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Element>()
                .ToList();
        }

        //Создаем коллекцию объектов систем с именами для создания по ним фильтров
        public ObservableCollection<HvacSystem> GetHvacSystems() {
            List<Element> ductSystems = GetCollection(BuiltInCategory.OST_DuctSystem);
            List<Element> pipeSystems = GetCollection(BuiltInCategory.OST_PipingSystem);
            List<Element> allSystems = ductSystems.Concat(pipeSystems).ToList();

            ObservableCollection<HvacSystem> newSystems = new ObservableCollection<HvacSystem>();

            return new ObservableCollection<HvacSystem>(
                allSystems.Select(system => new HvacSystem {
                    SystemElement = system,
                    SystemName = system.Name,
                    FopName = GetSystemFopName(system)
                })
           );
        }

        public void Execute() {
            ObservableCollection<HvacSystem> hvacSystems = GetHvacSystems();

            MainViewModel viewModel = new MainViewModel(this);
            viewModel.ShowWindow();
        }
    }
}
