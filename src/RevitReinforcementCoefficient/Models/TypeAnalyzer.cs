using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitReinforcementCoefficient.ViewModels;


namespace RevitReinforcementCoefficient.Models {
    internal class TypeAnalyzer {

        //private readonly string _paramNameForIfcDetection = "мод_ФОП_IFC семейство";

        private readonly List<string> _paramsForAll = new List<string>() {
            "обр_ФОП_Марка ведомости расхода",
            "обр_ФОП_Раздел проекта",
            "обр_ФОП_Группа КР",                    // не требуется для распределения по группам конструкций, но потом ее отдельно 
            "обр_ФОП_Орг. уровень"                  // проверять у опалубочных элементов будет не логично, поэтому сразу
        };

        // НУЖНА ЛИ "обр_ФОП_Группа КР"???


        private readonly List<string> _paramsForRebars = new List<string>() {
            "обр_ФОП_Форма_номер",
            "мод_ФОП_Диаметр",
            "обр_ФОП_Количество типовых на этаже",
            "обр_ФОП_Количество типовых этажей"
        };


        private readonly List<string> _paramsForSysRebars = new List<string>() {
            "Полная длина стержня",
            "Количество"
        };

        private readonly List<string> _paramsForIfcRebars = new List<string>() {
            "обр_ФОП_Длина",
            "обр_ФОП_Количество"
        };

        public TypeAnalyzer() { }


        /// <summary>
        /// Проверяет есть ли указанный список параметров в элементе на экземлпяре или типе
        /// </summary>
        public StringBuilder HasParams(Element element, List<string> paramNames, StringBuilder errors = null) {

            if(errors is null) {
                errors = new StringBuilder();
            }

            foreach(string paramName in paramNames) {

                // Сначала проверяем есть ли параметр на экземпляре
                if(!element.IsExistsParam(paramName)) {

                    // Если не нашли, ищем на типоразмере
                    Element elementType = element.Document.GetElement(element.GetTypeId());

                    if(!elementType.IsExistsParam(paramName)) {
                        // Если не нашли записываем

                        errors.AppendLine($"У элемента с {element.Id} не найден параметр {paramName}");
                    }
                }
            }
            return errors;
        }



        /// <summary>
        /// Проверяет наличие нужных параметром и распределяет элементы по типам конструкции
        /// </summary>
        public List<DesignTypeInfoVM> CheckNSortByDesignTypes(IEnumerable<Element> allElements) {

            List<DesignTypeInfoVM> designTypes = new List<DesignTypeInfoVM>();


            foreach(Element element in allElements) {

                // Проверяем наличие параметров, необходимых для распределения по типам конструкций
                if(HasParams(element, _paramsForAll).Length > 0) {

                    // Пока просто пропускаем, в дальйшем нужно сделать сборщик проблемных
                    continue;
                }

                // Пока пусть так, дальше нужно сделать в зависимости от уровня
                // Получение значений параметров, необходимых для распределения по типам конструкций
                string typeName = element.GetParamValue<string>("обр_ФОП_Марка ведомости расхода");
                string docPackage = element.GetParamValue<string>("обр_ФОП_Раздел проекта");
                bool aboveZero = element.GetParamValue<int>("обр_ФОП_Орг. уровень") > 0;

                // Ищем подходящий тип конструкции среди уже существующих в списке
                DesignTypeInfoVM designType = designTypes.FirstOrDefault(
                    e => e.TypeName == typeName && e.DocPackage == docPackage && e.AboveZero == aboveZero);

                // Если null, то создаем новый, если нет, то дописываем элемент в список уже существующего
                if(designType is null) {

                    DesignTypeInfoVM newDesignType = new DesignTypeInfoVM(typeName, docPackage, aboveZero);
                    newDesignType.AddItem(element);

                    designTypes.Add(newDesignType);
                } else {
                    designType.AddItem(element);
                }
            }

            return designTypes;
        }


        public StringBuilder CheckParamsInRebars(DesignTypeInfoVM designType) {

            StringBuilder errors = new StringBuilder();

            foreach(Element rebar in designType.Rebars) {

                HasParams(rebar, _paramsForRebars, errors);

                // Если элемент класса Rebar (т.е. системная арматура)
                if(rebar is Rebar) {

                    HasParams(rebar, _paramsForSysRebars, errors);
                } else {

                    HasParams(rebar, _paramsForIfcRebars, errors);
                }
            }

            if(errors.Length > 0) {
                designType.HasErrors = true;

                TaskDialog.Show("Ошибки:", errors.ToString());
            }

            designType.ParamsChecked = true;
            return errors;
        }




        /// <summary>
        /// Получает значение параметра в элементе на экземлпяре или типе
        /// </summary>
        public T GetParamValueAnywhere<T>(Element element, string paramName) {

            try {

                return element.GetParamValue<T>(paramName);
            } catch(Exception) {

                Element elementType = element.Document.GetElement(element.GetTypeId());
                return elementType.GetParamValue<T>(paramName);
            }
        }

        public void GetRebarData(DesignTypeInfoVM typeInfo) {

            foreach(Element element in typeInfo.Rebars) {

                int numberOfForm = GetParamValueAnywhere<int>(element, "обр_ФОП_Форма_номер");
                double dimeter = GetParamValueAnywhere<double>(element, "мод_ФОП_Диаметр");
                int countInLevel = GetParamValueAnywhere<int>(element, "обр_ФОП_Количество типовых на этаже");
                int countOfLevel = GetParamValueAnywhere<int>(element, "обр_ФОП_Количество типовых этажей");

                double lengthOfRebar;
                int count;

                double length;
                double fopCount;

                // Если элемент класса Rebar (т.е. системная арматура)
                if(element is Rebar) {

                    lengthOfRebar = GetParamValueAnywhere<double>(element, "Полная длина стержня");
                    count = GetParamValueAnywhere<int>(element, "Количество");
                } else {

                    length = GetParamValueAnywhere<double>(element, "обр_ФОП_Длина");
                    fopCount = GetParamValueAnywhere<double>(element, "обр_ФОП_Количество");
                }

                TaskDialog.Show("fdf", dimeter.ToString());
            }
        }
    }
}
