﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitArchitecturalDocumentation.Models {
    internal class ScheduleSelectionFilter : ISelectionFilter {
        public bool AllowElement(Element element) {
            if(element is ScheduleSheetInstance) {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference refer, XYZ point) {
            return false;
        }
    }
}
