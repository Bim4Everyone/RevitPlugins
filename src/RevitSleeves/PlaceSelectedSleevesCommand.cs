using Autodesk.Revit.Attributes;

using Ninject;

using RevitSleeves.Services.Core;

namespace RevitSleeves;
[Transaction(TransactionMode.Manual)]
internal class PlaceSelectedSleevesCommand : PlaceAllSleevesCommand {
    public PlaceSelectedSleevesCommand() {
        PluginName = "Расставить гильзы по выбору";
    }

    protected override void BindElementsServices(IKernel kernel) {
        kernel.Bind<IMepElementsProvider>()
            .To<SelectedMepElementsProvider>()
            .InSingletonScope();
        kernel.Bind<MepSelectionFilter>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<IStructureLinksProvider>()
            .To<AllLoadedStructureLinksProvider>()
            .InSingletonScope();
    }
}
