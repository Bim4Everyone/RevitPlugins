using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.Revit.DB;
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
            "обр_ФОП_Орг. уровень"
        };

        private readonly List<string> _paramsForFormElements = new List<string>() { "ФОП_ТИП_Армирование" };

        private readonly List<string> _paramsForRebars = new List<string>() {
            "обр_ФОП_Форма_номер",
            "мод_ФОП_Диаметр",
            "обр_ФОП_Длина",
            "обр_ФОП_Расчет в погонных метрах",
            "обр_ФОП_Количество типовых на этаже",
            "обр_ФОП_Количество типовых этажей"
        };


        private readonly List<string> _paramsForSysRebars = new List<string>() {
            "Полная длина стержня",
            "Количество"
        };

        private readonly List<string> _paramsForIfcRebars = new List<string>() {

            "обр_ФОП_Количество"
        };


        private readonly Dictionary<double, double> _massPerLengthDict = new Dictionary<double, double>() {
            { 4, 0.098},
            { 5, 0.144},
            { 6, 0.222},
            { 7, 0.302},
            { 8, 0.395},
            { 9, 0.499},
            { 10, 0.617},
            { 12, 0.888},
            { 14, 1.208},
            { 16, 1.578},
            { 18, 1.998},
            { 20, 2.466},
            { 22, 2.984},
            { 25, 3.853},
            { 28, 4.834},
            { 32, 6.313},
            { 36, 7.99},
            { 40, 9.805},
        };

        private readonly Dictionary<double, double> _overlapCoefDict = new Dictionary<double, double>() {
            { 8, 1.034},
            { 10, 1.043},
            { 12, 1.051},
            { 14, 1.06},
            { 16, 1.068},
            { 18, 1.077},
            { 20, 1.085},
            { 22, 1.094},
            { 25, 1.107},
            { 28, 1.12},
            { 32, 6.313},
            { 36, 7.99},
            { 40, 9.805},
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
                typeName = typeName is null ? "" : typeName;
                string docPackage = element.GetParamValue<string>("обр_ФОП_Раздел проекта");
                docPackage = docPackage is null ? "" : docPackage;
                bool aboveZero = element.GetParamValue<int>("обр_ФОП_Орг. уровень") > 0;
                // Сделали преобразование null в "" из-за того, что фильтрация в GUI иначе нормально не отрабатывает

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

        /// <summary>
        /// Проверяет наличие параметров у опалубки у типа конструкции
        /// </summary>
        public StringBuilder CheckParamsInFormElements(DesignTypeInfoVM designType) {

            StringBuilder errors = new StringBuilder();

            foreach(Element elem in designType.Elements) {

                HasParams(elem, _paramsForFormElements, errors);
            }

            if(errors.Length > 0) {
                designType.HasErrors = true;

                TaskDialog.Show("Ошибки:", errors.ToString());
            }

            designType.FormParamsChecked = true;
            return errors;
        }


        /// <summary>
        /// Проверяет наличие параметров у арматуры у типа конструкции
        /// </summary>
        public StringBuilder CheckParamsInRebars(DesignTypeInfoVM designType) {

            StringBuilder errors = new StringBuilder();

            foreach(Element rebar in designType.Rebars) {

                HasParams(rebar, _paramsForRebars, errors);

                // Если элемент класса Rebar (т.е. системная арматура)
                if(rebar is FamilyInstance) {

                    HasParams(rebar, _paramsForIfcRebars, errors);
                } else {

                    HasParams(rebar, _paramsForSysRebars, errors);
                }
            }

            if(errors.Length > 0) {
                designType.HasErrors = true;

                TaskDialog.Show("Ошибки:", errors.ToString());
            }

            designType.RebarParamsChecked = true;
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

        /// <summary>
        /// Расчет массы одного арматурного элемента
        /// </summary>
        private double CalculateRebarMass(Element rebar) {

            //string rep = string.Empty;

            int numberOfForm = GetParamValueAnywhere<int>(rebar, "обр_ФОП_Форма_номер");
            double dimeter = GetParamValueAnywhere<double>(rebar, "мод_ФОП_Диаметр");
            double dimeterInMm = UnitUtilsHelper.ConvertFromInternalValue(dimeter);
            int calcInLinearMeters = GetParamValueAnywhere<int>(rebar, "обр_ФОП_Расчет в погонных метрах");
            int countInLevel = GetParamValueAnywhere<int>(rebar, "обр_ФОП_Количество типовых на этаже");
            int countOfLevel = GetParamValueAnywhere<int>(rebar, "обр_ФОП_Количество типовых этажей");


            // В основном масса арматуры будет определяться как масса единицы (* на разные коэффициенты) * количество таких стержней
            // Но когда мы собираем с вида арматуру через FilteredrebarCollector, то помимо одиночных стержней у нас собираются и стержни в массиве,
            // среди которых есть массивы с включенной функцией "Переменный набор арматурных стержней", тогда запросить "обра_ФОП_Длина"
            // не представляется возможным, т.к. массив указывает, что у вложенных стержней она разная (вернет значение 0)
            // Из-за этой проблемы, чтобы упростить задачу, получаем среднее значение этой длины из Полной длины стержня

            double length = GetParamValueAnywhere<double>(rebar, "обр_ФОП_Длина");
            int count;
            double fullLength;

            // Если элемент класса FamilyInstance, то это IFC арматура
            if(rebar is FamilyInstance) {

                count = Convert.ToInt32((GetParamValueAnywhere<double>(rebar, "обр_ФОП_Количество")));
            } else {

                count = GetParamValueAnywhere<int>(rebar, "Количество");
                fullLength = GetParamValueAnywhere<double>(rebar, "Полная длина стержня");

                // Если длина одного стержня равна 0, то это стержни переменной длины, и мы находим длину одного деля общую длину на кол-во
                length = (length == 0) ? fullLength / count : length;
            }

            double lengthInMm = UnitUtilsHelper.ConvertFromInternalValue(length);


            // Погонная масса арматуры
            double massPerUnitLength;
            if(numberOfForm < 200) {

                massPerUnitLength = _massPerLengthDict.ContainsKey(dimeterInMm) ? _massPerLengthDict[dimeterInMm] : 0;
            } else {

                massPerUnitLength = GetParamValueAnywhere<double>(rebar, "обр_ФОП_Масса на единицу длины");
            }

            // Коэффициент нахлеста
            double overlapCoef = 1.1;
            if(lengthInMm > 11700) {

                overlapCoef = _overlapCoefDict.ContainsKey(dimeterInMm) ? _overlapCoefDict[dimeterInMm] : 1.1;
            }

            // Расчет массы одного стержня (длина в метрах * массу 1 метра)
            double calc = Math.Round(lengthInMm / 1000 * massPerUnitLength, 3);

            // Если стержень должен считаться погонажно, то добавляем коэффициент нахлеста
            calc = (calcInLinearMeters == 1) ? calc * overlapCoef : calc;

            // Вычисляем массу с учетом кол-ва стержней в массиве/сборке, на этаже, на этажах
            calc = calc * count * countInLevel * countOfLevel;


            //rep += $"Диаметр: {dimeterInMm}" + Environment.NewLine;
            //rep += $"Длина: {lengthInMm}" + Environment.NewLine;
            //rep += $"Масса погонного метра: {massPerUnitLength}" + Environment.NewLine;
            //rep += $"Коэффициент нахлеста: {overlapCoef}" + Environment.NewLine;
            //rep += $"Масса ед: {calc}" + Environment.NewLine;
            //rep += $"Общая масса: {calc}" + Environment.NewLine;
            //TaskDialog.Show("rep", rep);

            return calc;
        }


        public void CalculateRebarCoef(DesignTypeInfoVM typeInfo) {

            // Рассчет суммарного объема бетона у типа конструкции
            double volume = 0;
            foreach(Element element in typeInfo.Elements) {

                double volumeInInternal = element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED).AsDouble();
                volume += UnitUtilsHelper.ConvertVolumeFromInternalValue(volumeInInternal);
            }
            typeInfo.ConcreteVolume = Math.Round(volume, 2);


            // Рассчет суммарной массы арматуры у типа конструкции
            double sumMass = 0;
            foreach(Element rebar in typeInfo.Rebars) {

                sumMass += CalculateRebarMass(rebar);
            }
            typeInfo.RebarMass = Math.Round(sumMass, 2);

            typeInfo.RebarCoef = Math.Round(typeInfo.RebarMass / typeInfo.ConcreteVolume);
        }
    }
}
