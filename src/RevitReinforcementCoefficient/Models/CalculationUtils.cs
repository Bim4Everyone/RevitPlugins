using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitReinforcementCoefficient.ViewModels;

namespace RevitReinforcementCoefficient.Models {
    internal class CalculationUtils {

        private readonly ParamUtils _paramUtils;

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

        public CalculationUtils(ParamUtils paramUtils) {

            _paramUtils = paramUtils;
        }


        /// <summary>
        /// Расчет объема одного опалубочного элемента конструкции
        /// </summary>
        private double CalculateFormElementVolume(Element element) {

            double volumeInInternal = element.get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED).AsDouble();
            return UnitUtilsHelper.ConvertVolumeFromInternalValue(volumeInInternal);
        }


        /// <summary>
        /// Расчет массы одного арматурного элемента
        /// </summary>
        private double CalculateRebarMass(Element rebar) {

            int numberOfForm = _paramUtils.GetParamValueAnywhere<int>(rebar, "обр_ФОП_Форма_номер");
            double dimeter = _paramUtils.GetParamValueAnywhere<double>(rebar, "мод_ФОП_Диаметр");
            double dimeterInMm = UnitUtilsHelper.ConvertFromInternalValue(dimeter);
            int calcInLinearMeters = _paramUtils.GetParamValueAnywhere<int>(rebar, "обр_ФОП_Расчет в погонных метрах");
            int countInLevel = _paramUtils.GetParamValueAnywhere<int>(rebar, "обр_ФОП_Количество типовых на этаже");
            int countOfLevel = _paramUtils.GetParamValueAnywhere<int>(rebar, "обр_ФОП_Количество типовых этажей");

            // В основном масса арматуры будет определяться как масса единицы (* на разные коэффициенты) * количество таких стержней
            // Но когда мы собираем с вида арматуру через FilteredrebarCollector, то помимо одиночных стержней у нас собираются и стержни в массиве,
            // среди которых есть массивы с включенной функцией "Переменный набор арматурных стержней", тогда запросить "обр_ФОП_Длина"
            // не представляется возможным, т.к. массив указывает, что у вложенных стержней она разная (вернет значение 0)
            // Из-за этой проблемы, чтобы упростить задачу, получаем среднее значение этой длины из Полной длины стержня

            double length = _paramUtils.GetParamValueAnywhere<double>(rebar, "обр_ФОП_Длина");
            int count;

            // Если элемент класса FamilyInstance, то это IFC арматура
            if(rebar is FamilyInstance) {

                count = Convert.ToInt32(_paramUtils.GetParamValueAnywhere<double>(rebar, "обр_ФОП_Количество"));
            } else {

                count = _paramUtils.GetParamValueAnywhere<int>(rebar, "Количество");

                // Если длина одного стержня равна 0, то это стержни переменной длины, и мы находим длину одного деля общую длину на кол-во
                length = (length == 0) ? _paramUtils.GetParamValueAnywhere<double>(rebar, "Полная длина стержня") / count : length;
            }

            double lengthInMm = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(length), MidpointRounding.AwayFromZero);

            // Далее со знаком @ указывается название расчетного поля спецификации армирования в Revit, ниже - пояснения
            // @Базовый_Масса на единицу длины
            // Погонная масса арматуры
            double massPerUnitLength;
            if(numberOfForm < 200) {

                // TODO реализовать вывод в отчет с уведомлением, что один из стержней не посчитался
                massPerUnitLength = _massPerLengthDict.ContainsKey(dimeterInMm) ? _massPerLengthDict[dimeterInMm] : 0;
            } else {

                // Проверяем параметр "обр_ФОП_Масса на единицу длины" только сейчас, т.к. крайне маловероятно, что расчет будет вестись через него
                // Если его нет, то расчет невозможен
                if(!_paramUtils.HasParamAnywhere(rebar, "обр_ФОП_Масса на единицу длины")) {

                    // TODO реализовать вывод в отчет с уведомлением, что один из стержней не посчитался
                    return 0;
                }
                massPerUnitLength = _paramUtils.GetParamValueAnywhere<double>(rebar, "обр_ФОП_Масса на единицу длины");
            }

            // @Базовый_Нахлест
            // Коэффициент нахлеста
            double overlapCoef = 1;
            if(lengthInMm > 11700) {

                overlapCoef = _overlapCoefDict.ContainsKey(dimeterInMm) ? _overlapCoefDict[dimeterInMm] : 1.1;
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


        /// <summary>
        /// Рассчитывает объем бетона у типа конструкции
        /// </summary>
        public void CalculateConcreteVolume(DesignTypeInfoVM typeInfo) {

            // Рассчет суммарного объема бетона у типа конструкции
            double volume = 0;
            foreach(Element element in typeInfo.Elements) {

                volume += CalculateFormElementVolume(element);
            }
            typeInfo.ConcreteVolume = Math.Round(volume, 2);
        }


        /// <summary>
        /// Рассчитывает массу арматуры у типа конструкции
        /// </summary>
        public void CalculateRebarMass(DesignTypeInfoVM typeInfo) {

            // Рассчет суммарной массы арматуры у типа конструкции
            double sumMass = 0;
            foreach(Element rebar in typeInfo.Rebars) {

                sumMass += CalculateRebarMass(rebar);
            }
            typeInfo.RebarMass = Math.Round(sumMass, 2);
        }


        /// <summary>
        /// Рассчитывает коэффициент армирования у одного типа конструкции по массе арматуры и объему бетона
        /// </summary>
        public void CalculateRebarCoef(DesignTypeInfoVM typeInfo) {

            typeInfo.RebarCoef = Math.Round(typeInfo.RebarMass / typeInfo.ConcreteVolume);
            typeInfo.AlreadyCalculated = true;
        }


        /// <summary>
        /// Рассчитывает коэффициент армирования у нескольких типов конструкции по массе арматуры и объему бетона
        /// </summary>
        public void CalculateRebarCoefBySeveral(IEnumerable<DesignTypeInfoVM> typesInfo) {

            double totalRebarMass = 0;
            double totalConcreteVolume = 0;

            foreach(DesignTypeInfoVM typeInfo in typesInfo) {

                totalRebarMass += typeInfo.RebarMass;
                totalConcreteVolume += typeInfo.ConcreteVolume;
            }

            double averageReinforcementCoefficient = Math.Round(totalRebarMass / totalConcreteVolume);

            foreach(DesignTypeInfoVM typeInfo in typesInfo) {

                typeInfo.RebarCoef = averageReinforcementCoefficient;
                typeInfo.AlreadyCalculated = true;
            }
        }
    }
}
