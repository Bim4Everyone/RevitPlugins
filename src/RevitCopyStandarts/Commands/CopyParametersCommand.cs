using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitCopyStandarts.Commands {
    public class CopyParametersCommand : CopyStandartsCommand {
        public CopyParametersCommand(Document source, Document target)
            : base(source, target) {
        }

        protected override FilteredElementCollector GetFilteredElementCollector() {
            return base.GetFilteredElementCollector().OfClass(typeof(ParameterElement));
        }

        protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
            return base.FilterElements(elements)
                .Where(item => !_target.IsExistsParam(item.Name));
        }
    }
}