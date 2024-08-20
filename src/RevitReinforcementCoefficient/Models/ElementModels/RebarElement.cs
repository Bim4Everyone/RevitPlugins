using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitReinforcementCoefficient.Models.Report;

namespace RevitReinforcementCoefficient.Models.ElementModels {
    internal class RebarElement : ICommonElement {
        private readonly ParamUtils _utils;

        // Соотношение между диаметром арматурного стержня и массой его одного метра
        private readonly Dictionary<double, double> _massPerLengthDict = new Dictionary<double, double>() {
            { 8, 0.395},
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
        // Соотношение между диаметром арматурного стержня и величиной его длины с необходимым перехлестом
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
        };

        private readonly string _dimeterParamName = "мод_ФОП_Диаметр";
        private readonly string _lengthParamName = "обр_ФОП_Длина";
        private readonly string _fullLengthParamName = "Полная длина стержня";

        private readonly string _calcInLinearMetersParamName = "обр_ФОП_Расчет в погонных метрах";
        private readonly string _massPerUnitLengthParamName = "обр_ФОП_Масса на единицу длины";
        private readonly string _numberOfFormParamName = "обр_ФОП_Форма_номер";

        private readonly string _countSharedParamName = "обр_ФОП_Количество";
        private readonly string _countSystemParamName = "Количество";
        private readonly string _countInLevelParamName = "обр_ФОП_Количество типовых на этаже";
        private readonly string _countOfLevelParamName = "обр_ФОП_Количество типовых этажей";

        // Граничный номер формы для арматуры (более данного числа начинаются формы металлических деталей)
        private readonly int _limitNumberOfForm = 2000;
        // Длина арматурного стержня,который приходит на стройку с завода
        private readonly double _lengthOfFactoryRevitElement = 11700;
        // Коэф. нахлеста стержней, для которых не задекларирован коэф. конструкторами от А101
        private readonly double _overlapCoefForUnknown = 1.1;


        public RebarElement(Element element, IReportService reportService) {
            _utils = new ParamUtils(reportService);
            RevitElement = element;
        }


        public Element RevitElement { get; set; }

        /// <summary>
        /// Расчет массы одного арматурного элемента
        /// </summary>
        public double Calculate() {
            int numberOfForm = _utils.GetParamValueAnywhere<int>(RevitElement, _numberOfFormParamName);
            double dimeter = _utils.GetParamValueAnywhere<double>(RevitElement, _dimeterParamName);
            double dimeterInMm = UnitUtilsHelper.ConvertFromInternalValue(dimeter);
            int calcInLinearMeters = _utils.GetParamValueAnywhere<int>(RevitElement, _calcInLinearMetersParamName);
            int countInLevel = _utils.GetParamValueAnywhere<int>(RevitElement, _countInLevelParamName);
            int countOfLevel = _utils.GetParamValueAnywhere<int>(RevitElement, _countOfLevelParamName);

            // В основном масса арматуры будет определяться как масса единицы (* на разные коэффициенты) * количество таких стержней
            // Но когда мы собираем с вида арматуру через FilteredRevitElementCollector, то помимо одиночных стержней у нас собираются и стержни в массиве,
            // среди которых есть массивы с включенной функцией "Переменный набор арматурных стержней", тогда запросить "обр_ФОП_Длина"
            // не представляется возможным, т.к. массив указывает, что у вложенных стержней она разная (вернет значение 0)
            // Из-за этой проблемы, чтобы упростить задачу, получаем среднее значение этой длины из Полной длины стержня
            double length = _utils.GetParamValueAnywhere<double>(RevitElement, _lengthParamName);
            int count;

            // Если элемент класса FamilyInstance, то это IFC арматура
            if(RevitElement is FamilyInstance) {
                count = Convert.ToInt32(_utils.GetParamValueAnywhere<double>(RevitElement, _countSharedParamName));
            } else {
                count = _utils.GetParamValueAnywhere<int>(RevitElement, _countSystemParamName);

                // Если длина одного стержня равна 0, то это стержни переменной длины, и мы находим длину одного деля общую длину на кол-во
                length = length.Equals(0.0) ? _utils.GetParamValueAnywhere<double>(RevitElement, _fullLengthParamName) / count : length;
            }

            double lengthInMm = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(length), MidpointRounding.AwayFromZero);

            // Далее со знаком @ указывается название расчетного поля спецификации армирования в Revit, ниже - пояснения
            // @Базовый_Масса на единицу длины
            // Погонная масса арматуры
            double massPerUnitLength;
            // Сравниваем номер формы арматурногно стержня и граничный номер формы для арматуры (обычно 2000)
            if(numberOfForm < _limitNumberOfForm) {

                // TODO реализовать вывод в отчет с уведомлением, что один из стержней не посчитался
                massPerUnitLength = _massPerLengthDict.ContainsKey(dimeterInMm) ? _massPerLengthDict[dimeterInMm] : 0;
            } else {

                // Проверяем параметр "обр_ФОП_Масса на единицу длины" только сейчас, т.к. крайне маловероятно, что расчет будет вестись через него
                // Если его нет, то расчет невозможен
                if(!_utils.HasParamAnywhere(RevitElement, _massPerUnitLengthParamName)) {

                    // TODO реализовать вывод в отчет с уведомлением, что один из стержней не посчитался
                    return 0;
                }
                massPerUnitLength = _utils.GetParamValueAnywhere<double>(RevitElement, _massPerUnitLengthParamName);
            }

            // @Базовый_Нахлест
            // Коэффициент нахлеста
            double overlapCoef = 1;
            // Сравниваем длину стержня с заводской длиной
            // Если больше, значит будет укладываться несколько стержней по длине с нахлестом и нужно считать коэф. нахлеста
            if(lengthInMm > _lengthOfFactoryRevitElement) {

                overlapCoef = _overlapCoefDict.ContainsKey(dimeterInMm) ? _overlapCoefDict[dimeterInMm] : _overlapCoefForUnknown;
            }

            // @Базовый_Масса ЕД
            // if(обр_ФОП_Расчет в погонных метрах, Базовый_Масса на единицу длины, round((обр_ФОП_Длина / 1000 * Базовый_Масса на единицу длины) / 0.01 мм) * 0.01)
            double baseMassEd;
            if(calcInLinearMeters == 1) {

                baseMassEd = massPerUnitLength;
            } else {

                baseMassEd = Math.Round(lengthInMm / 1000 * massPerUnitLength, 2, MidpointRounding.AwayFromZero);
            }

            // @Базовый_Количество
            // decimal применен, т.к. иначе подсчет давал существенные расхождения со значениями спецификации
            // if(мод_ФОП_IFC семейство, обр_ФОП_Количество, Количество) *
            //          if(обр_ФОП_Расчет в погонных метрах, round((обр_ФОП_Длина / 1000 * Базовый_Нахлест) / 0.01 мм) * 0.01, 1) *
            //          if(Учесть типовые, (обр_ФОП_Количество типовых на этаже * обр_ФОП_Количество типовых этажей), 1)
            decimal baseCount = 1;
            if(calcInLinearMeters == 1) {
                double temp = lengthInMm / 1000 * overlapCoef;
                baseCount = decimal.Round((decimal) temp, 2, MidpointRounding.AwayFromZero);
            }
            baseCount = count * baseCount * countInLevel * countOfLevel;

            // Базовый_Масса Итог
            decimal calc = decimal.Round(baseCount * (decimal) baseMassEd, 2, MidpointRounding.AwayFromZero);

            return decimal.ToDouble(calc);
        }
    }
}
