using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement {
    [Transaction(TransactionMode.Manual)]
    internal class PlaceOpeningTasksBySelectionCmd : PlaceOpeningTasksCmd {
        public PlaceOpeningTasksBySelectionCmd() {
            PluginName = "Создать задания по выбранным элементам";
        }


        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToMethod(m => new RevitRepository(
                        uiApplication.Application,
                        uiApplication.ActiveUIDocument.Document))
                    .InSingletonScope();

                var revitRepository = kernel.Get<RevitRepository>();
                var selectedMepElements = revitRepository
                    .PickMepElements(OpeningConfig.GetOpeningConfig(revitRepository.Doc).Categories);

                PlaceOpeningTasks(uiApplication, revitRepository, selectedMepElements);
            }
        }
    }
}
