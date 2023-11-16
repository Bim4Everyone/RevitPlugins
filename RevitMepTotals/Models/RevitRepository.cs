using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitMepTotals.Models {
    internal class RevitRepository {
        public const string DefaultStringParamValue = "отсутствует";

        private const string _sharedName = "ФОП_ВИС_Наименование комбинированное";


        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication ?? throw new System.ArgumentNullException(nameof(uiApplication));
        }

        public UIApplication UIApplication { get; }

        public Application Application => UIApplication.Application;


        public ICollection<Duct> GetDucts(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_DuctCurves)
                .OfClass(typeof(Duct))
                .Cast<Duct>()
                .ToHashSet();
        }

        public ICollection<Pipe> GetPipes(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_PipeCurves)
                .OfClass(typeof(Pipe))
                .Cast<Pipe>()
                .ToHashSet();
        }

        public ICollection<PipeInsulation> GetPipeInsulations(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_PipeInsulations)
                .OfClass(typeof(PipeInsulation))
                .Cast<PipeInsulation>()
                .ToHashSet();
        }

        /// <summary>
        /// Возвращает значение параметра "ФОП_ВИС_Наименование"
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public string GetMepCurveElementSharedName(MEPCurve element) {
            if(element is null) { throw new ArgumentNullException(nameof(element)); }

            return element.GetSharedParamValueOrDefault(_sharedName, DefaultStringParamValue);
        }

        /// <summary>
        /// Возвращает значение параметра "Имя системы"
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetMepCurveElementMepSystemName(MEPCurve element) {
            if(element is null) { throw new ArgumentNullException(nameof(element)); }

            string systemName = element.GetParamValueOrDefault(BuiltInParameter.RBS_SYSTEM_NAME_PARAM, DefaultStringParamValue);
            return systemName;
        }

        /// <summary>
        /// Возвращает значение параметра "Длина" у воздуховодов и труб и их изоляции в мм
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public double GetMepCurveElementLength(MEPCurve element) {
            if(element is null) { throw new ArgumentNullException(nameof(element)); }

            double feetValue = element.GetParamValueOrDefault(BuiltInParameter.CURVE_ELEM_LENGTH, 0.0);
            return ConvertFeetToMillimeters(feetValue);
        }

        /// <summary>
        /// Возвращает значение параметра "Площадь" у воздуховодов
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public double GetMepCurveElementArea(MEPCurve element) {
            if(element is null) { throw new ArgumentNullException(nameof(element)); }

            double feetValue = element.GetParamValueOrDefault(BuiltInParameter.RBS_CURVE_SURFACE_AREA, 0.0);
            return ConvertSquareFeetToSquareMeters(feetValue);
        }

        /// <summary>
        /// Возвращает толщину изоляции в мм
        /// </summary>
        /// <param name="insulation"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public double GetPipeInsulationThickness(InsulationLiningBase insulation) {
            if(insulation is null) { throw new ArgumentNullException(nameof(insulation)); }

            return ConvertFeetToMillimeters(insulation.Thickness);
        }

        /// <summary>
        /// Возвращает название типоразмера воздуховода
        /// </summary>
        /// <param name="duct"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetDuctTypeName(Duct duct) {
            if(duct is null) { throw new ArgumentNullException(nameof(duct)); }

            return duct.DuctType.Name;
        }

        /// <summary>
        /// Возвращает значение параметра "Размер"
        /// </summary>
        /// <param name="duct"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetDuctSize(Duct duct) {
            if(duct is null) { throw new ArgumentNullException(nameof(duct)); }

            return GetMepCurveElementSize(duct);
        }

        /// <summary>
        /// Возвращает значение типоразмера трубы
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetPipeTypeName(Pipe pipe) {
            if(pipe is null) { throw new ArgumentNullException(nameof(pipe)); }

            return pipe.PipeType.Name;
        }

        /// <summary>
        /// Возвращает значение параметра "Размер"
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetPipeSize(Pipe pipe) {
            if(pipe is null) { throw new ArgumentNullException(nameof(pipe)); }

            return GetMepCurveElementSize(pipe);
        }

        /// <summary>
        /// Возвращает название типоразмера изоляции воздуховодов
        /// </summary>
        /// <param name="pipeInsulation"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetPipeInsulationTypeName(PipeInsulation pipeInsulation) {
            if(pipeInsulation is null) { throw new ArgumentNullException(nameof(pipeInsulation)); }

            return GetMepElementTypeName(pipeInsulation);
        }


        /// <summary>
        /// Возвращает значение параметра "Размер воздуховода"
        /// </summary>
        /// <param name="pipeInsulation"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetPipeInsulationSize(PipeInsulation pipeInsulation) {
            if(pipeInsulation is null) { throw new ArgumentNullException(nameof(pipeInsulation)); }

            return pipeInsulation.GetParamValueOrDefault(BuiltInParameter.RBS_PIPE_CALCULATED_SIZE, DefaultStringParamValue);
        }

        /// <summary>
        /// Возвращает, присутствует ли в документе общий параметр <see cref="_sharedName"/> у воздуховодов
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SharedNameForDuctsExists(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return SharedNameForCategoryExists(document, BuiltInCategory.OST_DuctCurves);
        }

        /// <summary>
        /// Возвращает, присутствует ли в документе общий параметр <see cref="_sharedName"/> у труб
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SharedNameForPipesExists(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return SharedNameForCategoryExists(document, BuiltInCategory.OST_PipeCurves);
        }

        /// <summary>
        /// Возвращает, присутствует ли в документе общий параметр <see cref="_sharedName"/> у изоляции трубопроводов
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool SharedNameForPipeInsulationsExists(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return SharedNameForCategoryExists(document, BuiltInCategory.OST_PipeInsulations);
        }


        /// <summary>
        /// Проверяет, присутствует ли в документе у заданной категории общий параметр <see cref="_sharedName"/>
        /// </summary>
        /// <param name="document"></param>
        /// <param name="builtInCategory"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private bool SharedNameForCategoryExists(Document document, BuiltInCategory builtInCategory) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            if(!document.IsExistsSharedParam(_sharedName)) { return false; }

            return document
                .GetSharedParamBinding(_sharedName)
                .Binding
                .GetCategories()
                .Select(category => category.GetBuiltInCategory())
                .Contains(builtInCategory);
        }

        /// <summary>
        /// Возвращает название типоразмера элемента
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string GetMepElementTypeName(Element element) {
            if(element is null) { throw new ArgumentNullException(nameof(element)); }

            return element.HasElementType() ? element.GetElementType().Name : DefaultStringParamValue;
        }

        /// <summary>
        /// Возвращает значение параметра "Размер" из воздуховода или трубы
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string GetMepCurveElementSize(Element element) {
            if(element is null) { throw new ArgumentNullException(nameof(element)); }

            return element.GetParamValueOrDefault(BuiltInParameter.RBS_CALCULATED_SIZE, DefaultStringParamValue);
        }

        /// <summary>
        /// Конвертирует футы в мм
        /// </summary>
        /// <param name="feetValue"></param>
        /// <returns></returns>
        private double ConvertFeetToMillimeters(double feetValue) {
#if REVIT_2020_OR_LESS

            return UnitUtils.ConvertFromInternalUnits(feetValue, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertFromInternalUnits(feetValue, UnitTypeId.Millimeters);
#endif
        }

        /// <summary>
        /// Конвертирует квадратные футы в квадратные метры
        /// </summary>
        /// <param name="squareFeetValue"></param>
        /// <returns></returns>
        private double ConvertSquareFeetToSquareMeters(double squareFeetValue) {
#if REVIT_2020_OR_LESS

            return UnitUtils.ConvertFromInternalUnits(squareFeetValue, DisplayUnitType.DUT_SQUARE_METERS);
#else
            return UnitUtils.ConvertFromInternalUnits(squareFeetValue, UnitTypeId.SquareMeters);
#endif
        }
    }
}
