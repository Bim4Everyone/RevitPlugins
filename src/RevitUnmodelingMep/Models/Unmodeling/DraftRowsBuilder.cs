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
        return Build(config, null);
    }

    public Dictionary<string, List<NewRowElement>> Build(
        ConsumableTypeItem config,
        UnmodelingCalcCache cache) {
        if(config == null) {
            throw new ArgumentNullException(nameof(config));
        }

        JArray assignedTypes = config.AssignedElementIds;
        Dictionary<string, List<NewRowElement>> elementsByGrouping = new();
        UnmodelingCalcCache localCache = cache ?? new UnmodelingCalcCache();

        foreach(JToken assignedTypeId in assignedTypes) {
#if REVIT_2024_OR_GREATER
            long idValue = assignedTypeId.Value<long>();
            ElementId typeId = new ElementId(idValue);
#else
            int idValue = assignedTypeId.Value<int>();
            ElementId typeId = new ElementId(idValue);
#endif

            if(!localCache.ElementsByTypeId.TryGetValue(typeId, out List<Element> elements)) {
                elements = CollectionGenerator.GetTypeInstances(_doc, typeId);
                localCache.ElementsByTypeId[typeId] = elements;
            }
            foreach(Element element in elements) {
                (string system, string economicFunction) = ResolveSystemAndFunction(element, localCache);

                ElementId elementId = element.Id;
                if(!localCache.SmrParamsById.TryGetValue(elementId, out var smrParams)) {
                    smrParams = (
                        element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.BuildingWorksBlock, ""),
                        element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.BuildingWorksSection, ""),
                        element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.BuildingWorksLevel, ""),
                        element.GetParamValueOrDefault<double>(SharedParamsConfig.Instance.BuildingWorksLevelCurrency, 0));
                    localCache.SmrParamsById[elementId] = smrParams;
                }

                if(!localCache.CalcElementsById.TryGetValue(elementId, out CalculationElementBase calcElement)) {
                    calcElement = _calculationElementFactory.Create(element);
                    localCache.CalcElementsById[elementId] = calcElement;
                }

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
                    SmrBlock = smrParams.Block,
                    SmrSection = smrParams.Section,
                    SmrFloor = smrParams.Floor,
                    SmrFloorCurrency = smrParams.Currency,
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

    private (string System, string EconomicFunction) ResolveSystemAndFunction(
        Element element,
        UnmodelingCalcCache cache) {
        if(cache != null && cache.SystemInfoById.TryGetValue(element.Id, out var cached)) {
            return cached;
        }

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
                var result = (system, economicFunction);
                if(cache != null && !cache.SystemInfoById.ContainsKey(element.Id)) {
                    cache.SystemInfoById[element.Id] = result;
                }
                return result;
            }

            return (string.Empty, string.Empty);
        }

        string ef = element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.EconomicFunction, "");
        string sys = element.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "");
        if(cache != null && !cache.SystemInfoById.ContainsKey(element.Id)) {
            cache.SystemInfoById[element.Id] = (sys, ef);
        }
        return (sys, ef);
    }
}

