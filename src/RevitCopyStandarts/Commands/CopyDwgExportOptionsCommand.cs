using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Commands {
    internal class CopyDwgExportOptionsCommand : ICopyStandartsCommand {
        private readonly Document _source;
        private readonly Document _target;

        public CopyDwgExportOptionsCommand(Document source, Document target) {
            _source = source;
            _target = target;
        }

        public void Execute() {
            var sourceOptions = DWGExportOptions.GetPredefinedOptions(_source, "");
            var targetOptions = DWGExportOptions.GetPredefinedOptions(_target, "");
        }
    }
}
