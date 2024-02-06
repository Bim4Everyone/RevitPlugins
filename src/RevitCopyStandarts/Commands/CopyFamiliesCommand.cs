using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    internal class CopyFamiliesCommand : CopyStandartsCommand {
        public CopyFamiliesCommand(Document source, Document destination)
            : base(source, destination) {
        }

        public override string Name => "Семейство";

        protected override FilteredElementCollector GetFilteredElementCollector() {
            return base.GetFilteredElementCollector()
                .OfClass(typeof(Family));
        }

        protected override bool IsAllowCommit(Element newElement, Element sourceElement) {
            return newElement.Name.Equals(sourceElement.Name);
        }
    }
}
