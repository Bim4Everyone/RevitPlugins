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

namespace RevitMepTotals.Models;
internal class RevitRepository {
    public const string DefaultStringParamValue = "отсутствует";

    private const string _sharedName = "ФОП_ВИС_Наименование комбинированное";


    public RevitRepository(UIApplication uiApplication) {
        UIApplication = uiApplication ?? throw new System.ArgumentNullException(nameof(uiApplication));
    }

    public UIApplication UIApplication { get; }

    public Application Application => UIApplication.Application;


    public ICollection<Duct> GetDucts(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : (ICollection<Duct>) new FilteredElementCollector(document)
            .WhereElementIsNotElementType()
            .OfCategory(BuiltInCategory.OST_DuctCurves)
            .OfClass(typeof(Duct))
            .Cast<Duct>()
            .ToHashSet();
    }

    public ICollection<Pipe> GetPipes(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : (ICollection<Pipe>) new FilteredElementCollector(document)
            .WhereElementIsNotElementType()
            .OfCategory(BuiltInCategory.OST_PipeCurves)
            .OfClass(typeof(Pipe))
            .Cast<Pipe>()
            .ToHashSet();
    }

    public ICollection<PipeInsulation> GetPipeInsulations(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : (ICollection<PipeInsulation>) new FilteredElementCollector(document)
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
        return element is null
            ? throw new ArgumentNullException(nameof(element))
            : element.GetSharedParamValueOrDefault(_sharedName, DefaultStringParamValue);
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
        return insulation is null ? throw new ArgumentNullException(nameof(insulation)) : ConvertFeetToMillimeters(insulation.Thickness);
    }

    /// <summary>
    /// Возвращает название типоразмера воздуховода
    /// </summary>
    /// <param name="duct"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string GetDuctTypeName(Duct duct) {
        return duct is null ? throw new ArgumentNullException(nameof(duct)) : duct.DuctType.Name;
    }

    /// <summary>
    /// Возвращает значение параметра "Размер"
    /// </summary>
    /// <param name="duct"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string GetDuctSize(Duct duct) {
        return duct is null ? throw new ArgumentNullException(nameof(duct)) : GetMepCurveElementSize(duct);
    }

    /// <summary>
    /// Возвращает значение типоразмера трубы
    /// </summary>
    /// <param name="pipe"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string GetPipeTypeName(Pipe pipe) {
        return pipe is null ? throw new ArgumentNullException(nameof(pipe)) : pipe.PipeType.Name;
    }

    /// <summary>
    /// Возвращает значение параметра "Размер"
    /// </summary>
    /// <param name="pipe"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string GetPipeSize(Pipe pipe) {
        return pipe is null ? throw new ArgumentNullException(nameof(pipe)) : GetMepCurveElementSize(pipe);
    }

    /// <summary>
    /// Возвращает название типоразмера изоляции воздуховодов
    /// </summary>
    /// <param name="pipeInsulation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string GetPipeInsulationTypeName(PipeInsulation pipeInsulation) {
        return pipeInsulation is null ? throw new ArgumentNullException(nameof(pipeInsulation)) : GetMepElementTypeName(pipeInsulation);
    }


    /// <summary>
    /// Возвращает значение параметра "Размер воздуховода"
    /// </summary>
    /// <param name="pipeInsulation"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string GetPipeInsulationSize(PipeInsulation pipeInsulation) {
        return pipeInsulation is null
            ? throw new ArgumentNullException(nameof(pipeInsulation))
            : pipeInsulation.GetParamValueOrDefault(BuiltInParameter.RBS_PIPE_CALCULATED_SIZE, DefaultStringParamValue);
    }

    /// <summary>
    /// Возвращает, присутствует ли в документе общий параметр <see cref="_sharedName"/> у воздуховодов
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool SharedNameForDuctsExists(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : SharedNameForCategoryExists(document, BuiltInCategory.OST_DuctCurves);
    }

    /// <summary>
    /// Возвращает, присутствует ли в документе общий параметр <see cref="_sharedName"/> у труб
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool SharedNameForPipesExists(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : SharedNameForCategoryExists(document, BuiltInCategory.OST_PipeCurves);
    }

    /// <summary>
    /// Возвращает, присутствует ли в документе общий параметр <see cref="_sharedName"/> у изоляции трубопроводов
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool SharedNameForPipeInsulationsExists(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : SharedNameForCategoryExists(document, BuiltInCategory.OST_PipeInsulations);
    }


    /// <summary>
    /// Проверяет, присутствует ли в документе у заданной категории общий параметр <see cref="_sharedName"/>
    /// </summary>
    /// <param name="document"></param>
    /// <param name="builtInCategory"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private bool SharedNameForCategoryExists(Document document, BuiltInCategory builtInCategory) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : document.IsExistsSharedParam(_sharedName) && document
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
        return element is null
            ? throw new ArgumentNullException(nameof(element))
            : element.HasElementType() ? element.GetElementType().Name : DefaultStringParamValue;
    }

    /// <summary>
    /// Возвращает значение параметра "Размер" из воздуховода или трубы
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private string GetMepCurveElementSize(Element element) {
        return element is null
            ? throw new ArgumentNullException(nameof(element))
            : element.GetParamValueOrDefault(BuiltInParameter.RBS_CALCULATED_SIZE, DefaultStringParamValue);
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
