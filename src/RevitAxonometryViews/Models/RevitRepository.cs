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

        //Если существует ФОП_ВИС_Имя системы, проверяет во всех ли нужных категориях он. Если нет - возвращает в строке в каких нет,
        //если все окей - возвращает null
        public string CheckVisNameCategories() {
            //ЗАМЕЧАНИЕ: ТАК НЕ ДЕЛАЕМ, ДОБАВЛЯЕМ ПАРАМЕТР ЕСЛИ НЕ СУЩЕСТВУЕТ
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

        //Создаем коллекцию объектов систем с именами для создания по ним фильтров
        public ObservableCollection<HvacSystemViewModel> GetHvacSystems() {
            
            List<Element> ductSystems = Document.GetCollection(BuiltInCategory.OST_DuctSystem);
            List<Element> pipeSystems = Document.GetCollection(BuiltInCategory.OST_PipingSystem);
            List<Element> allSystems = ductSystems.Concat(pipeSystems).ToList();

            ObservableCollection<HvacSystemViewModel> newSystems = new ObservableCollection<HvacSystemViewModel>();

            return new ObservableCollection<HvacSystemViewModel>(
                allSystems.Select(system => new HvacSystemViewModel {
                    SystemName = system.Name,
                    FopName = GetSystemFopName(system)
                })
           );
        }

        //Транзакция с созданием видов через класс ViewFactory
        public void ExecuteViewCreation(List<HvacSystemViewModel> hvacSystems, bool? useFopName, bool? useOneView) {
            ViewFactory viewFactory = new ViewFactory(Document, ActiveUIDocument, useFopName, useOneView);
            using(Transaction t = Document.StartTransaction("Создать схемы")) {
                viewFactory.CreateViewsBySelectedSystems(hvacSystems);
                t.Commit();
            }
        }

        //Точка старта. Перед инициализацией проверям корректность "ФОП_ВИС_Имя системы". Если где-то ее нет, пишем и закрываемся.
        public void Initialize() {
            string report = CheckVisNameCategories();

            if(string.IsNullOrEmpty(report)) {
                MainViewModel viewModel = new MainViewModel(this);
                viewModel.ShowWindow();

            } else {
                MessageBox.Show(report);
            }
        }
    }
}
