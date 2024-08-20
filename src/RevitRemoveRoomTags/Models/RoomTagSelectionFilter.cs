using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;

namespace RevitRemoveRoomTags.Models {
    internal class RoomTagSelectionFilter : ISelectionFilter {
        public bool AllowElement(Element element) {
            return element is RoomTag;
        }

        public bool AllowReference(Reference refer, XYZ point) {
            return false;
        }
    }
}
