using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

namespace RevitSleeves;
[Transaction(TransactionMode.Manual)]
internal class PlaceAllSleevesCommand : BasePluginCommand {
    protected override void Execute(UIApplication uiApplication) {
        TaskDialog.Show("Place all", "Place all");
    }
}
