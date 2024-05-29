using System;

using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitOpeningPlacement.Models.Interfaces;

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


        private protected bool ModelCorrect(IChecker checker) {
            if(checker.IsCorrect()) {
                return true;
            } else {
                TaskDialog.Show("BIM", checker.GetErrorMessage());
                return false;
            }
        }
    }
}
