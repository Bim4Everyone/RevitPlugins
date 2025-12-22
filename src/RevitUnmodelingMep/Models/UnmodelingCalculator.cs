using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using Newtonsoft.Json.Linq;

using Ninject.Activation;

using RevitUnmodelingMep.Models.Entities;
using RevitUnmodelingMep.ViewModels;

namespace RevitUnmodelingMep.Models;

internal class UnmodelingCalculator {
    private readonly Document _doc;

    public UnmodelingCalculator(Document doc) {
        _doc = doc;
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
                string economicFunction =
                    element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.EconomicFunction, "");
                string system =
                    element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
                string name = config.Name;
                string mark = config.Mark;
                string code = config.Code;
                string maker = config.Maker;
                string unit = config.Unit;

                string group = $"{config.Grouping}_{name}_{mark}_{code}_{maker}";

                NewRowElement newRow = new NewRowElement {
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

               

                string key = $"{config.Grouping}_{smrBlock}_{smrSection}_{smrFloor}_{name}_{mark}_{code}_{maker}";

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

        foreach(NewRowElement draftRow in draftRows) {
            CalculationElement calcElement = GetGeometricDescription(draftRow.Element, config);
            draftRow.Number = CalculateFormula(config.Formula, calcElement);
        }

        NewRowElement baseRow = draftRows.First();
        double totalNumber = draftRows.Sum(r => r.Number);
        Console.WriteLine(baseRow.Name);
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
            Note = baseRow.Note,
            Number = totalNumber,
            System = baseRow.System,
            Function = baseRow.Function,
            Description = baseRow.Description,
            Mass = baseRow.Mass
        };
    }

    private CalculationElement GetGeometricDescription(Element element, ConsumableTypeItem config) {
        CalculationElement calcElement = new CalculationElement();

        calcElement.Formula = config.Formula;
        calcElement.Element = element;

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
            return new CalculationElement();
        }
        if(element.Category.IsId(BuiltInCategory.OST_PipingSystem)) {
            return new CalculationElement();
        }

        return new CalculationElement();
    }

    private double CalculateFormula(string formula, CalculationElement calElement) {
        if(formula is null) {
            throw new ArgumentNullException(nameof(formula));
        }

        if(calElement is null) {
            throw new ArgumentNullException(nameof(calElement));
        }

        return 1;
    }

    private CalculationElement GetDuctGeoDesc(MEPCurve duct) {
        DuctType ductType = (DuctType) duct.GetElementType();
        CalculationElement calculationElement = new CalculationElement();
        if(ductType.Shape == ConnectorProfileType.Round) {
            calculationElement.IsRound = true;
            calculationElement.Diameter = duct.Diameter;
            calculationElement.Perimeter = Math.PI * 2 * duct.Diameter;

        } else {
            calculationElement.IsRound = false;
            calculationElement.Width = duct.Width;
            calculationElement.Height = duct.Height;
            calculationElement.Perimeter = duct.Width * 2 + duct.Height * 2;
        }


        calculationElement.Area = duct.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
        calculationElement.InsulationThikness =
            duct.GetParamValueOrDefault<double>(BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS);
        if(calculationElement.InsulationThikness == 0) {
            calculationElement.IsInsulated = false;
        } else {
            calculationElement.IsInsulated = true;
            ElementFilter insulationFilter = new ElementClassFilter(typeof(DuctInsulation));
            ICollection<ElementId> dependentInsulations = duct.GetDependentElements(insulationFilter);
            double insulationArea = 0;
            foreach(ElementId insulationId in dependentInsulations) {
                Element insulation = _doc.GetElement(insulationId);
                double area =
                    insulation.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                insulationArea += area;
            }
            calculationElement.InsulationArea = insulationArea;
        }

        calculationElement.Length = duct.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH);
        return calculationElement;
    }

    private CalculationElement GetPipeGeoDesc(Pipe pipe) {
        CalculationElement calculationElement = new CalculationElement();
        double length = pipe.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH);
        double outDiameter = pipe.GetParamValueOrDefault<double>(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);

        calculationElement.IsRound = true;
        calculationElement.Diameter = pipe.Diameter;
        calculationElement.OutDiameter = outDiameter;
        calculationElement.Area = Math.PI * outDiameter * length;
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
            double insulationArea = 0;
            foreach(ElementId insulationId in dependentInsulations) {
                Element insulation = _doc.GetElement(insulationId);
                double area =
                    insulation.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
                insulationArea += area;
            }
            calculationElement.InsulationArea = insulationArea;
        }

        return calculationElement;
    }

    private CalculationElement GetPipeInsGeoDes(PipeInsulation pipeIns) {
        CalculationElement calculationElement = new CalculationElement();
        Pipe pipe = (Pipe) _doc.GetElement(pipeIns.HostElementId);
        double inDiameter = pipe.Diameter;

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
        CalculationElement calculationElement = new CalculationElement();
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
        calculationElement.Area = ductIns.GetParamValueOrDefault<double>(BuiltInParameter.RBS_CURVE_SURFACE_AREA);
        calculationElement.Length = ductIns.GetParamValueOrDefault<double>(BuiltInParameter.CURVE_ELEM_LENGTH);


        return calculationElement;
    }
}
