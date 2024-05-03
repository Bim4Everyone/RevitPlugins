using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    /// <summary>
    /// Копирует материал Manage -> Materials
    /// </summary>
    internal class CopyMaterialsCommand : CopyStandartsCommand {
        public CopyMaterialsCommand(Document source, Document destination)
            : base(source, destination) {
        }

        public override string Name => "Материал";

        protected override FilteredElementCollector GetFilteredElementCollector() {
            return base.GetFilteredElementCollector()
                .OfClass(typeof(Material));
        }
    }
}
