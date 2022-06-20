using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitWindowGapPlacement.Model;
using RevitWindowGapPlacement.ViewModels;
using RevitWindowGapPlacement.Views;

namespace RevitWindowGapPlacement {
    [Transaction(TransactionMode.Manual)]
    public class NavigateWindowGapsCommand : BasePluginCommand {
        public NavigateWindowGapsCommand() {
            PluginName = "Навигатор проемов окон";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(var t = uiApplication.ActiveUIDocument.Document.StartTransaction("Shit")) {
                var revitRepository = new RevitRepository(uiApplication);

                var windowGapType = revitRepository.GetWindowGapType();
                if(!windowGapType.IsActive) {
                    windowGapType.Activate();
                }
                
                FamilyInstance windowGap =
                    uiApplication.ActiveUIDocument.Document.Create.NewFamilyInstance(
                        new XYZ(30.183727034122, 9.02230971129252, 30 + 15.748031496063),
                        windowGapType,
                        uiApplication.ActiveUIDocument.Document.GetElement(new ElementId(6895552)),
                        (Level) revitRepository.Document.GetElement(new ElementId(3991953)),
                        StructuralType.NonStructural);

                t.Commit();
            }
        }
    }
}