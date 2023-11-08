using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SystemParams;

namespace RevitMepTotals.Models {
    internal class RevitRepository {
        private const string _sharedName = "ФОП_ВИС_Наименование комбинированное";
        private const string _default = "отсутствует";


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
        public string GetMepCurveElementSharedName(Document document, MEPCurve element) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(element is null) { throw new ArgumentNullException(nameof(element)); }

            if(element.GetParameters(_sharedName).FirstOrDefault(item => item.IsShared) != null) {
                SharedParam param = SharedParamsConfig.Instance.CreateRevitParam(document, _sharedName);
                return element.GetParamValueOrDefault(param, _default);
            } else {
                return _default;
            }
        }

        /// <summary>
        /// Возвращает значение параметра "Имя системы"
        /// </summary>
        /// <param name="document"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetMepCurveElementMepSystemName(Document document, MEPCurve element) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(element is null) { throw new ArgumentNullException(nameof(element)); }

            SystemParam param = SystemParamsConfig.Instance
                .CreateRevitParam(document, BuiltInParameter.RBS_SYSTEM_NAME_PARAM);
            string systemName = element.GetParamValueOrDefault(param, _default);
            return systemName;
        }

        /// <summary>
        /// Возвращает значение параметра "Длина" у воздуховодов и труб и их изоляции в мм
        /// </summary>
        /// <param name="document"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public double GetMepCurveElementLength(Document document, MEPCurve element) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(element is null) { throw new ArgumentNullException(nameof(element)); }

            SystemParam param = SystemParamsConfig.Instance
                .CreateRevitParam(document, BuiltInParameter.CURVE_ELEM_LENGTH);
            double feetValue = element.GetParamValueOrDefault(param, 0.0);
            return ConvertFeetToMillimeters(feetValue);
        }

        /// <summary>
        /// Возвращает толщину изоляции в мм
        /// </summary>
        /// <param name="document"></param>
        /// <param name="insulation"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public double GetPipeInsulationThickness(Document document, InsulationLiningBase insulation) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
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
        /// <param name="document"></param>
        /// <param name="duct"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetDuctSize(Document document, Duct duct) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(duct is null) { throw new ArgumentNullException(nameof(duct)); }

            return GetMepCurveElementSize(document, duct);
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
        /// <param name="document"></param>
        /// <param name="pipe"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetPipeSize(Document document, Pipe pipe) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(pipe is null) { throw new ArgumentNullException(nameof(pipe)); }

            return GetMepCurveElementSize(document, pipe);
        }

        /// <summary>
        /// Возвращает название типоразмера изоляции воздуховодов
        /// </summary>
        /// <param name="document"></param>
        /// <param name="pipeInsulation"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetPipeInsulationTypeName(Document document, PipeInsulation pipeInsulation) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(pipeInsulation is null) { throw new ArgumentNullException(nameof(pipeInsulation)); }

            return GetMepElementTypeName(document, pipeInsulation);
        }


        /// <summary>
        /// Возвращает значение параметра "Размер воздуховода"
        /// </summary>
        /// <param name="document"></param>
        /// <param name="pipeInsulation"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public string GetPipeInsulationSize(Document document, PipeInsulation pipeInsulation) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(pipeInsulation is null) { throw new ArgumentNullException(nameof(pipeInsulation)); }

            SystemParam param = SystemParamsConfig.Instance
                .CreateRevitParam(document, BuiltInParameter.RBS_PIPE_CALCULATED_SIZE);
            return pipeInsulation.GetParamValueOrDefault(param, _default);
        }

        /// <summary>
        /// Возвращает название типоразмера элемента
        /// </summary>
        /// <param name="document"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string GetMepElementTypeName(Document document, Element element) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(element is null) { throw new ArgumentNullException(nameof(element)); }

            return document.GetElement(element.GetTypeId()).Name;
        }

        /// <summary>
        /// Возвращает значение параметра "Размер" из воздуховода или трубы
        /// </summary>
        /// <param name="document"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private string GetMepCurveElementSize(Document document, Element element) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }
            if(element is null) { throw new ArgumentNullException(nameof(element)); }

            SystemParam param = SystemParamsConfig.Instance
                .CreateRevitParam(document, BuiltInParameter.RBS_CALCULATED_SIZE);
            return element.GetParamValueOrDefault(param, _default);
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
