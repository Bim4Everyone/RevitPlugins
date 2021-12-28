using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    internal class CopyElementIdsCommand : CopyStandartsCommand {
        public CopyElementIdsCommand(Document source, Document target)
            : base(source, target) {
        }

        public override string Name => "Идентификаторы";
        public IReadOnlyCollection<Element> CopyElements { get; set; }

        protected override IEnumerable<Element> GetElements() {
            return CopyElements;
        }
    }
}
