using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties;

namespace RevitUnmodelingMep.Models;

internal static class CollectionGenerator {
    public static List<Element> GetElementTypesByCategory(Document doc, BuiltInCategory category) {
        return new FilteredElementCollector(doc)
            .OfCategory(category)
            .WhereElementIsElementType()
            .ToElements()
            .ToList();
    }

    public static List<Element> GetTypeInstances(Document doc, ElementId typeId) {
        ElementType type = doc.GetElement(typeId) as ElementType;
        if(type == null)
            return new List<Element>();

        List<Element> elements = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .Where(e => e.GetTypeId() == typeId)
            .ToList();

        if(type.InAnyCategory([BuiltInCategory.OST_PipingSystem, BuiltInCategory.OST_DuctSystem])) {
            var unique = new Dictionary<string, Element>();

            foreach(var element in elements) {
                var mepSystem = element as MEPSystem;
                if(mepSystem == null)
                    continue;

                ElementSet network = null;

                if(mepSystem.Category.IsId(BuiltInCategory.OST_PipingSystem)) {
                    network = ((PipingSystem) mepSystem).PipingNetwork;
                } else if(mepSystem.Category.IsId(BuiltInCategory.OST_DuctSystem)) {
                    network = ((MechanicalSystem) mepSystem).DuctNetwork;
                }

                if(network == null || network.IsEmpty)
                    continue;

                var firstCurve = network
                    .Cast<Element>()
                    .FirstOrDefault(e => e is MEPCurve);

                if(firstCurve == null)
                    continue;

                var systemName = firstCurve
                    .GetParamValueOrDefault<string>(SharedParamsConfig.Instance.VISSystemName, "")
                    ?.Trim();

                if(string.IsNullOrEmpty(systemName))
                    continue;

                if(!unique.ContainsKey(systemName)) {
                    unique[systemName] = element;
                }
            }

            // получили список только уникальных по systemName систем
            return unique.Values.ToList();
        }

        if(type.InAnyCategory(
            new[]
            {
            BuiltInCategory.OST_PipeInsulations,
            BuiltInCategory.OST_DuctInsulations
            })) {
            bool IsValidHost(ElementId hostId) {
                if(hostId == ElementId.InvalidElementId)
                    return false;

                Element host = doc.GetElement(hostId);
                if(host?.Category == null)
                    return false;

                return host.Category.IsId(BuiltInCategory.OST_PipeCurves)
                       || host.Category.IsId(BuiltInCategory.OST_DuctCurves);
            }

            elements = elements
                .Where(e => {
                    if(e is PipeInsulation pipeInsulation)
                        return IsValidHost(pipeInsulation.HostElementId);

                    if(e is DuctInsulation ductInsulation)
                        return IsValidHost(ductInsulation.HostElementId);

                    return false;
                })
                .ToList();
        }

        if(type.Category.IsId(BuiltInCategory.OST_DuctCurves)) {
            bool IsValidForm(MEPCurve duct) {
                DuctType ductType = (DuctType) duct.GetElementType();
                if(ductType.Shape == ConnectorProfileType.Oval) {
                    return false;
                }
                return true;
            }

            elements = elements
                .Where(e => {
                    if(e is Duct duct)
                        return IsValidForm(duct);

                    return false;
                })
                .ToList();
        }

        return elements;
    }

}
