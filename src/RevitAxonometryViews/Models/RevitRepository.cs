using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitAxonometryViews.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;


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

                List<string> reportList = new List<string>();

                foreach(BuiltInCategory builtInCategory in AxonometryConfig.SystemAndFopCats) {
                    if(!builtInCategories.Contains(builtInCategory)) {
                        Console.WriteLine(builtInCategory);
                    }
                }
                return null;
            }
        }


        public List<Element> GetCollection(BuiltInCategory category) {
            List<Element> col = (List<Element>) new FilteredElementCollector(Document)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .ToElements();
            return col;
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

        public void Execute() {

            string test = CheckVisNameCategories();
            //Console.WriteLine(test);
        }
    }
}
