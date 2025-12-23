using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Revit;

namespace RevitUnmodelingMep.Models;

internal static class CollectionGenerator {
    public static List<Element> GetElementsByCategory(Document doc, BuiltInCategory category) {
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

                return host.Category.IsId(BuiltInCategory.OST_PipeFitting)
                       || host.Category.IsId(BuiltInCategory.OST_DuctFitting);
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

        return elements;
    }

}
