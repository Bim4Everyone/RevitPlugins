using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    internal class CopyFoundationSlabCommand : CopyStandartsCommand {
        public CopyFoundationSlabCommand(Document source, Document target)
            : base(source, target) {
        }

        protected override IEnumerable<Element> FilterElements(IList<Element> elements) {
            return elements.Cast<FloorType>().Where(item => item.IsFoundationSlab);
        }

        protected override FilteredElementCollector GetFilteredElementCollector() {
            return base.GetFilteredElementCollector()
                .OfClass(typeof(FloorType));
        }
    }
}
