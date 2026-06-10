using System;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

namespace RevitPylonLoadAreas;

[Transaction(TransactionMode.Manual)]
public class GetLandThicknessOnLoadAreaCommand : BasePluginCommand {
    public GetLandThicknessOnLoadAreaCommand() {
        PluginName = "Импорт LandXML";
    }

    protected override void Execute(UIApplication uiApplication) {
        throw new NotImplementedException();
    }
}
