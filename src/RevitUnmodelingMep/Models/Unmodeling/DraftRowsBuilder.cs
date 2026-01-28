using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using Newtonsoft.Json.Linq;

using RevitUnmodelingMep.Models.Entities;
using RevitUnmodelingMep.ViewModels;

namespace RevitUnmodelingMep.Models.Unmodeling;

internal sealed class DraftRowsBuilder {
    private readonly Document _doc;
    private readonly CalculationElementFactory _calculationElementFactory;
    private readonly PlaceholderReplacer _placeholderReplacer;

    public DraftRowsBuilder(Document doc, CalculationElementFactory calculationElementFactory, PlaceholderReplacer placeholderReplacer) {
        _doc = doc;
        _calculationElementFactory = calculationElementFactory;
        _placeholderReplacer = placeholderReplacer;
    }

    public Dictionary<string, List<NewRowElement>> Build(ConsumableTypeItem config) {
        if(config == null) {
            throw new ArgumentNullException(nameof(config));
        }

        JArray assignedTypes = config.AssignedElementIds;
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
                (string system, string economicFunction) = ResolveSystemAndFunction(element);

                string smrBlock =
                    element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.BuildingWorksBlock, "");
                string smrSection =
                    element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.BuildingWorksSection, "");
                string smrFloor =
                    element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.BuildingWorksLevel, "");
                string smrFloorCurrnecy =
                    element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.BuildingWorksLevelCurrency, "");

                CalculationElementBase calcElement = _calculationElementFactory.Create(element);

                string name = _placeholderReplacer.Replace(config.Name, calcElement);
                string mark = config.Mark;
                string code = config.Code;
                string maker = config.Maker;
                string unit = config.Unit;

                string group = $"{config.Grouping}_{name}_{mark}_{code}_{maker}";

                NewRowElement newRow = new NewRowElement {
                    System = system,
                    Function = economicFunction,
                    Element = element,
                    CalculationElement = calcElement,
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

    private (string System, string EconomicFunction) ResolveSystemAndFunction(Element element) {
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
                string economicFunction =
                    firstCurve.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.EconomicFunction, "");
                string system =
                    firstCurve.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
                return (system, economicFunction);
            }

            return (string.Empty, string.Empty);
        }

        string ef = element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.EconomicFunction, "");
        string sys = element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
        return (sys, ef);
    }
}

