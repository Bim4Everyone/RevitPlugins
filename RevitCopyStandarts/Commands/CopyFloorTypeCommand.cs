using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    internal class CopyFloorTypeCommand : CopyStandartsCommand {
        public CopyFloorTypeCommand(Document source, Document target)
            : base(source, target) {
        }

        public override string Name { get; set; } = "Типы перекрытий";

        protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
            return elements.Cast<FloorType>().Where(item => item.IsFoundationSlab == false);
        }

        protected override FilteredElementCollector GetFilteredElementCollector() {
            return base.GetFilteredElementCollector()
                .OfClass(typeof(FloorType));
        }
    }
}
