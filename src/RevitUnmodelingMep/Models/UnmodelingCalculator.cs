using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

using Newtonsoft.Json.Linq;


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
                string name = config.Naming;
                string mark = config.Brand;
                string code = config.Code;
                string maker = config.Factory;
                string unit = config.Unit;

                string group = $"{config.Grouping}_{name}_{mark}_{code}_{maker}";

                NewRowElement newRow = new NewRowElement {
                    Element = element,
                    Group = group,
                    SmrBlock = smrBlock,
                    SmrSection = smrSection,
                    SmrFloor = smrFloor,
                    SmrFloorDE = smrFloorCurrnecy,
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
            draftRow.Number = CalculateFormula(config.NumberFormula, calcElement);
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
            SmrFloorDE = baseRow.SmrFloorDE,
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

        calcElement.Formula = config.NumberFormula;
        calcElement.Element = element;

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
}
