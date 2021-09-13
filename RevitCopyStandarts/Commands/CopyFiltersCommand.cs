using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    /// <summary>
    /// Копирует фильтры в файл View -> Filters
    /// </summary>
    internal class CopyFiltersCommand : CopyStandartsCommand {
        public CopyFiltersCommand(Document source, Document destination)
            : base(source, destination) {
        }

        public override string Name => "Фильтр";

        protected override FilteredElementCollector GetFilteredElementCollector() {
            return base.GetFilteredElementCollector()
                .OfClass(typeof(FilterElement));
        }
    }
}
