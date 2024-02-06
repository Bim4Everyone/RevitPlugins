using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitCopyStandarts.Commands {
    internal class CopyProjectParametersCommand : CopyStandartsCommand {
        public CopyProjectParametersCommand(Document source, Document target)
            : base(source, target) {
        }

        protected override FilteredElementCollector GetFilteredElementCollector() {
            return base.GetFilteredElementCollector().OfClass(typeof(ParameterElement));
        }

        protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
            return base.FilterElements(elements).OfType<ParameterElement>().Where(item=> item.IsProjectParam());
        }
    }
}