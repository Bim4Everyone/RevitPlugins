using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement {
    class RevitPlaceOpeningTaskCommand : BasePluginCommand {
        public RevitPlaceOpeningTaskCommand() {
            PluginName = "Настройка ";
        }

        protected override void Execute(UIApplication uiApplication) {
            var openingConfig = OpeningConfig.GetOpeningConfig();

        }
    }
}
