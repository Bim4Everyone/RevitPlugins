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

        List<Element> elements = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .Where(e => e.GetTypeId() == typeId)
            .ToList();

        if(type.InAnyCategory(
            [
            BuiltInCategory.OST_PipeInsulations,
            BuiltInCategory.OST_DuctInsulations
        ])) {
            elements = elements
                .Where(e => {
                    if(e is PipeInsulation pipeInsulation)
                        return pipeInsulation.HostElementId != ElementId.InvalidElementId;

                    if(e is DuctInsulation ductInsulation)
                        return ductInsulation.HostElementId != ElementId.InvalidElementId;

                    return false;
                })
                .ToList();
        }

        return elements;
    }
}
