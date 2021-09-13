using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    /// <summary>
    /// Копирует спецификации
    /// </summary>
    internal class CopyViewSchedulesCommand : CopyStandartsCommand {
        public CopyViewSchedulesCommand(Document source, Document destination)
            : base(source, destination) {
        }

        public override string Name => "Спецификация";

        protected override FilteredElementCollector GetFilteredElementCollector() {
            return base.GetFilteredElementCollector()
                .OfClass(typeof(ViewSchedule));
        }
    }
}
