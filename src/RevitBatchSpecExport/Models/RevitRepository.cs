using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

namespace RevitBatchSpecExport.Models;
internal class RevitRepository {
    public readonly SharedParam CombinedNameParam = SharedParamsConfig.Instance.VISCombinedName;


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
    public string GetMepCurveElementSharedName(MEPCurve element, string defaultValue) {
        return element is null
            ? throw new ArgumentNullException(nameof(element))
            : element.GetParamValueOrDefault(CombinedNameParam.Name, defaultValue);
    }

    /// <summary>
    /// Возвращает значение параметра "Имя системы"
    /// </summary>
    public string GetMepCurveElementMepSystemName(MEPCurve element, string defaultValue) {
        if(element is null) { throw new ArgumentNullException(nameof(element)); }

        string systemName = element.GetParamValueOrDefault(BuiltInParameter.RBS_SYSTEM_NAME_PARAM, defaultValue);
        return systemName;
    }

    /// <summary>
    /// Возвращает значение параметра "Длина" у воздуховодов и труб и их изоляции в мм
    /// </summary>
    public double GetMepCurveElementLength(MEPCurve element) {
        if(element is null) { throw new ArgumentNullException(nameof(element)); }

        double feetValue = element.GetParamValueOrDefault(BuiltInParameter.CURVE_ELEM_LENGTH, 0.0);
        return ConvertFeetToMillimeters(feetValue);
    }

    /// <summary>
    /// Возвращает значение параметра "Площадь" у воздуховодов
    /// </summary>
    public double GetMepCurveElementArea(MEPCurve element) {
        if(element is null) { throw new ArgumentNullException(nameof(element)); }

        double feetValue = element.GetParamValueOrDefault(BuiltInParameter.RBS_CURVE_SURFACE_AREA, 0.0);
        return ConvertSquareFeetToSquareMeters(feetValue);
    }

    /// <summary>
    /// Возвращает толщину изоляции в мм
    /// </summary>
    public double GetPipeInsulationThickness(InsulationLiningBase insulation) {
        return insulation is null ? throw new ArgumentNullException(nameof(insulation)) : ConvertFeetToMillimeters(insulation.Thickness);
    }

    /// <summary>
    /// Возвращает название типоразмера воздуховода
    /// </summary>
    public string GetDuctTypeName(Duct duct) {
        return duct is null ? throw new ArgumentNullException(nameof(duct)) : duct.DuctType.Name;
    }

    /// <summary>
    /// Возвращает значение параметра "Размер"
    /// </summary>
    public string GetDuctSize(Duct duct, string defaultValue) {
        return duct is null
            ? throw new ArgumentNullException(nameof(duct))
            : GetMepCurveElementSize(duct, defaultValue);
    }

    /// <summary>
    /// Возвращает значение типоразмера трубы
    /// </summary>
    public string GetPipeTypeName(Pipe pipe) {
        return pipe is null ? throw new ArgumentNullException(nameof(pipe)) : pipe.PipeType.Name;
    }

    /// <summary>
    /// Возвращает значение параметра "Размер"
    /// </summary>
    public string GetPipeSize(Pipe pipe, string defaultValue) {
        return pipe is null
            ? throw new ArgumentNullException(nameof(pipe))
            : GetMepCurveElementSize(pipe, defaultValue);
    }

    /// <summary>
    /// Возвращает название типоразмера изоляции воздуховодов
    /// </summary>
    public string GetPipeInsulationTypeName(PipeInsulation pipeInsulation, string defaultValue) {
        return pipeInsulation is null ? throw new ArgumentNullException(nameof(pipeInsulation)) : GetMepElementTypeName(pipeInsulation, defaultValue);
    }


    /// <summary>
    /// Возвращает значение параметра "Размер воздуховода"
    /// </summary>
    public string GetPipeInsulationSize(PipeInsulation pipeInsulation, string defaultValue) {
        return pipeInsulation is null
            ? throw new ArgumentNullException(nameof(pipeInsulation))
            : pipeInsulation.GetParamValueOrDefault(BuiltInParameter.RBS_PIPE_CALCULATED_SIZE, defaultValue);
    }

    /// <summary>
    /// Возвращает, присутствует ли в документе общий параметр <see cref="CombinedNameParam"/> у воздуховодов
    /// </summary>
    public bool SharedNameForDuctsExists(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : SharedNameForCategoryExists(document, BuiltInCategory.OST_DuctCurves);
    }

    /// <summary>
    /// Возвращает, присутствует ли в документе общий параметр <see cref="CombinedNameParam"/> у труб
    /// </summary>
    public bool SharedNameForPipesExists(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : SharedNameForCategoryExists(document, BuiltInCategory.OST_PipeCurves);
    }

    /// <summary>
    /// Возвращает, присутствует ли в документе общий параметр <see cref="CombinedNameParam"/> у изоляции трубопроводов
    /// </summary>
    public bool SharedNameForPipeInsulationsExists(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : SharedNameForCategoryExists(document, BuiltInCategory.OST_PipeInsulations);
    }


    /// <summary>
    /// Проверяет, присутствует ли в документе у заданной категории общий параметр <see cref="CombinedNameParam"/>
    /// </summary>
    private bool SharedNameForCategoryExists(Document document, BuiltInCategory builtInCategory) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : document.IsExistsSharedParam(CombinedNameParam.Name) && document
            .GetSharedParamBinding(CombinedNameParam.Name)
            .Binding
            .GetCategories()
            .Select(category => category.GetBuiltInCategory())
            .Contains(builtInCategory);
    }

    /// <summary>
    /// Возвращает название типоразмера элемента
    /// </summary>
    private string GetMepElementTypeName(Element element, string defaultValue) {
        return element is null
            ? throw new ArgumentNullException(nameof(element))
            : element.HasElementType() ? element.GetElementType().Name : defaultValue;
    }

    /// <summary>
    /// Возвращает значение параметра "Размер" из воздуховода или трубы
    /// </summary>
    private string GetMepCurveElementSize(Element element, string defaultValue) {
        return element is null
            ? throw new ArgumentNullException(nameof(element))
            : element.GetParamValueOrDefault(BuiltInParameter.RBS_CALCULATED_SIZE, defaultValue);
    }

    /// <summary>
    /// Конвертирует футы в мм
    /// </summary>
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
    private double ConvertSquareFeetToSquareMeters(double squareFeetValue) {
#if REVIT_2020_OR_LESS

        return UnitUtils.ConvertFromInternalUnits(squareFeetValue, DisplayUnitType.DUT_SQUARE_METERS);
#else
        return UnitUtils.ConvertFromInternalUnits(squareFeetValue, UnitTypeId.SquareMeters);
#endif
    }
}
