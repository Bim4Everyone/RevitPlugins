using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using Autodesk.Revit.DB;

namespace RevitAxonometryViews.Models {
    public static class CollectorOperator {
        public static List<Element> GetParameterFilterElements(this Document document) {
            return new FilteredElementCollector(document).OfClass(typeof(ParameterFilterElement)).ToList();
        }

        public static List<Element> GetElementsByCategory(this Document document, BuiltInCategory category) {
            return new FilteredElementCollector(document)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .ToList();
        }
    }
}
