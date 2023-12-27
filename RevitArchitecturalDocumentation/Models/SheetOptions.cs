using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitArchitecturalDocumentation.Models {
    internal sealed class SheetOptions {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        public SheetOptions(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
        }

        public bool WorkWithSheets { get; set; }
        public ElementType SelectedTitleBlock { get; set; }
        public string SelectedTitleBlockName { get; set; }
        public string SheetNamePrefix { get; set; }
    }
}
