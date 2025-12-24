using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

using CodingSeb.ExpressionEvaluator;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using Newtonsoft.Json.Linq;

using Ninject.Activation;

using RevitUnmodelingMep.Models.Entities;
using RevitUnmodelingMep.ViewModels;

namespace RevitUnmodelingMep.Models;

internal class UnmodelingCalculator {
    private readonly Document _doc;
    private readonly ILocalizationService _localizationService;
    private readonly double _stockDuctPipe;
    private readonly double _stockDuctIns;
    private readonly double _stockPipeIns;

    public UnmodelingCalculator(Document doc, ILocalizationService localizationService) {
        _doc = doc;
        _localizationService = localizationService;
        _stockDuctPipe = GetStocks(BuiltInCategory.OST_DuctCurves);
        _stockPipeIns = GetStocks(BuiltInCategory.OST_PipeInsulations);
        _stockDuctIns = GetStocks(BuiltInCategory.OST_DuctInsulations);
    }

    public List<NewRowElement> GetElementsToGenerate(ConsumableTypeItem config) {
        Dictionary<string, List<NewRowElement>> keyNewRowPairs = GetSplitedNewRows(config);

        List<NewRowElement> rowsToGenerate = new List<NewRowElement>();
        foreach(KeyValuePair<string, List<NewRowElement>> pair in keyNewRowPairs) {
            string key = pair.Key;
            List<NewRowElement> empyNumberRows = pair.Value;

            NewRowElement rowToGenerate = GetFinalRow(empyNumberRows, config);
            rowsToGenerate.Add(rowToGenerate);
        }
        return rowsToGenerate;
    }

    private double GetStocks(BuiltInCategory category) {
        if(category == BuiltInCategory.OST_PipeCurves || category == BuiltInCategory.OST_DuctCurves) {
            return 1 + _doc.ProjectInformation.GetParamValueOrDefault<double>(
                SharedParamsConfig.Instance.VISPipeDuctReserve) / 100;
        }
        if(category == BuiltInCategory.OST_PipeInsulations) {
            return 1 + _doc.ProjectInformation.GetParamValueOrDefault<double>(
                SharedParamsConfig.Instance.VISPipeInsulationReserve) / 100;
        }
        if(category == BuiltInCategory.OST_DuctInsulations) {
            return 1 + _doc.ProjectInformation.GetParamValueOrDefault<double>(
                SharedParamsConfig.Instance.VISDuctInsulationReserve) / 100;
        }

        return 1;
    }
            

    private Dictionary<string, List<NewRowElement>> GetSplitedNewRows(ConsumableTypeItem config) {
        int categoryId = int.Parse(config.CategoryId);
        BuiltInCategory category = (BuiltInCategory) categoryId;
        JArray assignedTypes = config.AssignedElementIds;

        //List<Element> elements = CollectionGenerator.GetElementsByCategory(_doc, category);
        Dictionary<string, List<NewRowElement>> elementsByGrouping = new();
        foreach(JToken assignedTypeId in assignedTypes) {

#if REVIT_2024_OR_GREATER
            long idValue = assignedTypeId.Value<long>();
            ElementId typeId = new ElementId(idValue);
#else
            int idValue = assignedTypeId.Value<int>();
            ElementId typeId = new ElementId(idValue);
#endif
            
            List<Element> elements = CollectionGenerator.GetTypeInstances(_doc, typeId);

            foreach(Element element in elements) {
                string smrBlock = 
                    element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.BuildingWorksBlock, "");
                string smrSection =
                    element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.BuildingWorksSection, "");
                string smrFloor =
                    element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.BuildingWorksLevel, "");
                string smrFloorCurrnecy =
                    element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.BuildingWorksLevelCurrency, "");


                string economicFunction = string.Empty;
                string system = string.Empty;

                if(element.InAnyCategory([BuiltInCategory.OST_PipingSystem, BuiltInCategory.OST_DuctSystem])) {
                    MEPSystem mepSystem = element as MEPSystem;
                    ElementSet network = new ElementSet();

                    if(mepSystem != null && mepSystem.Category.IsId(BuiltInCategory.OST_PipingSystem)) {
                        network = ((PipingSystem) mepSystem).PipingNetwork;
                    } else if(mepSystem != null && mepSystem.Category.IsId(BuiltInCategory.OST_DuctSystem)) {
                        network = ((MechanicalSystem) mepSystem).DuctNetwork;
                    }

                    Element firstCurve = network
                        .Cast<Element>()
                        .FirstOrDefault(e => e is MEPCurve);

                    if(firstCurve != null) {
                        economicFunction =
                            firstCurve.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.EconomicFunction, "");
                        system =
                            firstCurve.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
                    }
                } else {
                    economicFunction =
                        element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.EconomicFunction, "");
                    system =
                        element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
                }


                string name = config.Name;
                string mark = config.Mark;
                string code = config.Code;
                string maker = config.Maker;
                string unit = config.Unit;

                string group = $"{config.Grouping}_{name}_{mark}_{code}_{maker}";

                NewRowElement newRow = new NewRowElement {
                    System = system,
                    Function = economicFunction,
                    Element = element,
                    Group = group,
                    SmrBlock = smrBlock,
                    SmrSection = smrSection,
                    SmrFloor = smrFloor,
                    SmrFloorCurrency = smrFloorCurrnecy,
                    Name = name,
                    Code = code,
                    Mark = mark,
                    Maker = maker,
                    Unit = unit
                };

                // TODO: Нужно ли в ключе делить по СМР?
                string key = $"{config.Grouping}_{system}_{economicFunction}_{name}_{mark}_{code}_{maker}";

                if(!elementsByGrouping.TryGetValue(key, out List<NewRowElement> groupingElements)) {
                    groupingElements = new List<NewRowElement>();
                    elementsByGrouping[key] = groupingElements;
                }

                groupingElements.Add(newRow);
            }
        }
        return elementsByGrouping;
    }

    private NewRowElement GetFinalRow(List<NewRowElement> draftRows, ConsumableTypeItem config) {
        if(draftRows == null || draftRows.Count == 0) {
            return new NewRowElement();
        }

        List<CalculationElement> calculationElements = new List<CalculationElement>();

        foreach(NewRowElement draftRow in draftRows) {
            CalculationElement calcElement = GetGeometricDescription(draftRow.Element);
            draftRow.Number = CalculateFormula(config, calcElement);
            calculationElements.Add(calcElement);
        }

        NewRowElement baseRow = draftRows.First();
        double totalNumber = draftRows.Sum(r => r.Number);
        
        return new NewRowElement {
            Element = baseRow.Element,
            Group = baseRow.Group,
            SmrBlock = baseRow.SmrBlock,
            SmrSection = baseRow.SmrSection,
            SmrFloor = baseRow.SmrFloor,
            SmrFloorCurrency = baseRow.SmrFloorCurrency,
            Name = baseRow.Name,
            Code = baseRow.Code,
            Mark = baseRow.Mark,
            Maker = baseRow.Maker,
            Unit = baseRow.Unit,
            Number = totalNumber,
            System = baseRow.System,
            Function = baseRow.Function,
            Description = baseRow.Description,
            Mass = baseRow.Mass,
            Note = CreateNote(config, calculationElements)
        };
    }

    private string CreateNote(ConsumableTypeItem config, List<CalculationElement> calculationElements) {
        string noteFormula = config.Note ?? string.Empty;
        double sumArea = calculationElements.Sum(r => r.Area ?? throw new InvalidOperationException("Area is null."));
        double sumLength = calculationElements.Sum(r => r.Length ?? throw new InvalidOperationException("Length is null."));
        double count = calculationElements.Count();

        string FormatValue(double value) {
            double rounded = Math.Round(value, 2);
            return rounded.ToString("0.##", CultureInfo.InvariantCulture);
        }

        string result = noteFormula
            .Replace("sumArea", FormatValue(sumArea))
            .Replace("sumLength", FormatValue(sumLength))
            .Replace("count", FormatValue(count));

        return result;
    }

    private CalculationElement GetGeometricDescription(Element element) {
        Category category = element.Category;

        if(element.Category.IsId(BuiltInCategory.OST_DuctCurves)) {
            return GetDuctGeoDesc((MEPCurve) element);
        }
        if(element.Category.IsId(BuiltInCategory.OST_PipeCurves)) {
            return GetPipeGeoDesc((Pipe) element);
        }
        if(element.Category.IsId(BuiltInCategory.OST_PipeInsulations)) {
            return GetPipeInsGeoDes((PipeInsulation) element);
        }
        if(element.Category.IsId(BuiltInCategory.OST_DuctInsulations)) {
            return GetDuctInsGeoDesc((DuctInsulation) element);
        }
        if(element.Category.IsId(BuiltInCategory.OST_DuctSystem)) {
            return GetSystemDesc(element);
        }
        if(element.Category.IsId(BuiltInCategory.OST_PipingSystem)) {
            return GetSystemDesc(element);
        }

        throw new InvalidOperationException(
           $"GetGeometricDescription unexpected element type: (Id {element?.Id})");
    }

    private void BuildDebugLog(CalculationElement calcElement, string formula) {
        StringBuilder logBuilder = new StringBuilder();
        string errorMessage = _localizationService.GetLocalizedString("UnmodelingCalculator.Error");
        logBuilder.AppendLine(errorMessage);
        logBuilder.AppendLine($"ElementId: {calcElement.Element.Id}");
        logBuilder.AppendLine($"Formula: {formula}");

        foreach(var property in calcElement.GetType().GetProperties()) {
            if(!property.CanRead) {
                continue;
            }

            object value = property.GetValue(calcElement);

            string valueText = value switch {
                double d => d.ToString(CultureInfo.InvariantCulture),
                float f => f.ToString(CultureInfo.InvariantCulture),
                null => "null",
                _ => value.ToString()
            };
            logBuilder.AppendLine($"{property.Name} = {valueText}");
        }

        Console.WriteLine(logBuilder.ToString());

        throw new OperationCanceledException(errorMessage);
    }

    private BuiltInCategory ParseCategory(ConsumableTypeItem config) {
        int categoryId = int.Parse(config.CategoryId);
        return (BuiltInCategory) categoryId;

    }

    private double CalculateFormula(ConsumableTypeItem config, CalculationElement calElement) {
        double projectStock = 1;
        BuiltInCategory category = ParseCategory(config);

        if(
        category == BuiltInCategory.OST_PipeCurves || category == BuiltInCategory.OST_DuctCurves) {
            projectStock = _stockDuctPipe;
        }
        if(category == BuiltInCategory.OST_PipeInsulations) {
            projectStock = _stockPipeIns;
        }
        if(category == BuiltInCategory.OST_DuctInsulations) {
            projectStock = _stockDuctIns;
        }

        string formula = config.Formula;
        var evaluator = new ExpressionEvaluator();

        foreach(var property in calElement.GetType().GetProperties()) {
            if(!property.CanRead) {
                continue;
            }

            object value = property.GetValue(calElement);
            evaluator.Variables[property.Name] = value;
        }

        try {
            object result = evaluator.Evaluate(formula);
            if(config.UseCategoryReserve) {
                return Convert.ToDouble(result) * projectStock;
            }
            return Convert.ToDouble(result);
        } catch(Exception) {
            BuildDebugLog(calElement, formula);
            throw;
        }
    }

    private CalculationElement GetDuctGeoDesc(MEPCurve duct) {
        DuctType ductType = (DuctType) duct.GetElementType();
        CalculationElement calculationElement = new CalculationElement(duct);

        calculationElement.SystemSharedName = 
            duct.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
        calculationElement.SystemTypeName = duct.MEPSystem.GetElementType().Name;
        double length = duct.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH);
        if(ductType.Shape == ConnectorProfileType.Round) {
            calculationElement.IsRound = true;
            calculationElement.Diameter = duct.Diameter;
            calculationElement.Perimeter = Math.PI * 2 * duct.Diameter;
            double crossSectionArea = Math.PI * Math.Pow(duct.Diameter / 2, 2);
            double volume = crossSectionArea * length;
            calculationElement.Volume = volume;

        } else {
            calculationElement.IsRound = false;
            calculationElement.Width = duct.Width;
            calculationElement.Height = duct.Height;
            calculationElement.Perimeter = duct.Width * 2 + duct.Height * 2;
            double crossSectionArea = duct.Width * duct.Height;
            double volume = crossSectionArea * length;
            calculationElement.Volume = volume;
        }

        
        double area = duct.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);

        calculationElement.Area = area;
        calculationElement.Length = length;
        calculationElement.InsulationThikness =
            duct.GetParamValueOrDefault<double>(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS);
        if(calculationElement.InsulationThikness == 0) {
            calculationElement.IsInsulated = false;
        } else {
            calculationElement.IsInsulated = true;
            ElementFilter insulationFilter = new ElementClassFilter(typeof(DuctInsulation));
            ICollection<ElementId> dependentInsulations = duct.GetDependentElements(insulationFilter);
            double sumInsulationArea = 0;
            foreach(ElementId insulationId in dependentInsulations) {
                Element insulation = _doc.GetElement(insulationId);
                double insArea =
                    insulation.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                sumInsulationArea += insArea;
            }
            calculationElement.InsulationArea = sumInsulationArea;
        }

        
        return calculationElement;
    }

    private CalculationElement GetPipeGeoDesc(Pipe pipe) {
        CalculationElement calculationElement = new CalculationElement(pipe);

        calculationElement.SystemSharedName =
            pipe.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
        calculationElement.SystemTypeName = pipe.MEPSystem.GetElementType().Name;

        double length = pipe.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH);
        double outDiameter = pipe.GetParamValueOrDefault<double>(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
        double crossSectionArea = Math.PI * Math.Pow(pipe.Diameter / 2, 2);
        double surfaceArea = Math.PI * outDiameter * length;
        double volume = crossSectionArea * length;

        calculationElement.IsRound = true;
        calculationElement.Diameter = pipe.Diameter;
        calculationElement.OutDiameter = outDiameter;
        calculationElement.Area = surfaceArea;
        calculationElement.Volume = volume;
        calculationElement.Perimeter = Math.PI * 2 * pipe.Diameter;
        calculationElement.Length = length;

        calculationElement.InsulationThikness = 
            pipe.GetParamValueOrDefault<double>(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS);
        if(calculationElement.InsulationThikness == 0) {
            calculationElement.IsInsulated = false;
        } else {
            calculationElement.IsInsulated = true;
            ElementFilter insulationFilter = new ElementClassFilter(typeof(PipeInsulation));
            ICollection<ElementId> dependentInsulations = pipe.GetDependentElements(insulationFilter);
            double sumInsulationArea = 0;
            foreach(ElementId insulationId in dependentInsulations) {
                Element insulation = _doc.GetElement(insulationId);
                double insArea =
                    insulation.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                sumInsulationArea += insArea;
            }
            calculationElement.InsulationArea = sumInsulationArea;
        }

        return calculationElement;
    }

    private CalculationElement GetPipeInsGeoDes(PipeInsulation pipeIns) {
        CalculationElement calculationElement = new CalculationElement(pipeIns);
        Pipe pipe = (Pipe) _doc.GetElement(pipeIns.HostElementId);
        double inDiameter = pipe.Diameter;

        calculationElement.SystemSharedName =
            pipeIns.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
        calculationElement.SystemTypeName = pipeIns.MEPSystem.GetElementType().Name;

        calculationElement.InsulationThikness = pipeIns.Thickness;
        calculationElement.IsRound = true;
        calculationElement.Diameter = pipe.Diameter;
        calculationElement.OutDiameter = pipeIns.Diameter;
        ;
        calculationElement.Area 
            = pipeIns.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);

        calculationElement.Perimeter = Math.PI * 2 * pipeIns.Diameter;
        calculationElement.Length = pipeIns.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH);

        return calculationElement;

    }

    private CalculationElement GetDuctInsGeoDesc(DuctInsulation ductIns) {
        MEPCurve duct = (MEPCurve) _doc.GetElement(ductIns.HostElementId);
        DuctType ductType = (DuctType) duct.GetElementType();
        CalculationElement calculationElement = new CalculationElement(ductIns);
        if(ductType.Shape == ConnectorProfileType.Round) {
            calculationElement.IsRound = true;
            calculationElement.Diameter = ductIns.Diameter;
            calculationElement.Perimeter = Math.PI * 2 * ductIns.Diameter;

        } else {
            calculationElement.IsRound = false;
            calculationElement.Width = ductIns.Width;
            calculationElement.Height = ductIns.Height;
            calculationElement.Perimeter = ductIns.Width * 2 + ductIns.Height * 2;
        }

        calculationElement.SystemSharedName =
            ductIns.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
        calculationElement.SystemTypeName = ductIns.MEPSystem.GetElementType().Name;
        calculationElement.Area = ductIns.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
        calculationElement.Length = ductIns.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH);


        return calculationElement;
    }

    private CalculationElement GetSystemDesc(Element element) {
        CalculationElement calculationElement = new CalculationElement(element);
        calculationElement.Element = element;
        return calculationElement;
    }
}
