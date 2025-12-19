using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using Autodesk.Revit.DB;

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
        return new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .Where(e => e.GetTypeId() == typeId)
            .ToList();
    }
}
