using System;

using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.Checkers;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Базовый класс команды для размещения чистовых отверстий АР. Служит для проверки корректности документа для запуска плагина
    /// </summary>
    public abstract class OpeningRealPlacerCmd : BasePluginCommand {
        protected OpeningRealPlacerCmd(string pluginName) {
            if(string.IsNullOrWhiteSpace(pluginName)) {
                throw new ArgumentNullException(nameof(pluginName));
            }
            PluginName = pluginName;
        }


        private protected bool ModelCorrect(RevitRepository revitRepository) {
            var checker = new RealOpeningsChecker(revitRepository);
            var errors = checker.GetErrorTexts();
            if(errors == null || errors.Count == 0) {
                return true;
            }

            TaskDialog.Show("BIM", $"{string.Join($"{Environment.NewLine}", errors)}");
            return false;
        }
    }
}
