using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    /// <summary>
    /// Копирует легенды в документ (создает дубликаты)
    /// </summary>
    internal class CopyViewLegendsCommand : CopyStandartsCommand {
        public CopyViewLegendsCommand(Document source, Document destination)
            : base(source, destination) {
        }

        public override string Name => "Легенда";

        protected override IEnumerable<Element> FilterElements(IList<Element> elements) {
            return base.FilterElements(elements)
                .Cast<View>()
                .Where(item => item.ViewType == ViewType.Legend);
        }

        protected override FilteredElementCollector GetFilteredElementCollector() {
            return base.GetFilteredElementCollector()
                .OfClass(typeof(View));
        }
    }
}
