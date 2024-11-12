using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement {
    [Transaction(TransactionMode.Manual)]
    internal class PlaceOpeningTasksBySelectionCmd : PlaceOpeningTasksCmd {
        public PlaceOpeningTasksBySelectionCmd() {
            PluginName = "Создать задания по выбранным элементам";
        }


        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<IDocTypesProvider>()
                    .ToMethod(c => {
                        return new DocTypesProvider(new DocTypeEnum[] { DocTypeEnum.AR, DocTypeEnum.KR });
                    })
                    .InSingletonScope();
                kernel.Bind<IRevitLinkTypesSetter>()
                    .To<DocTypeLoadedLinksSetter>()
                    .InTransientScope();
                kernel.Bind<IDocTypesHandler>()
                    .To<DocTypesHandler>()
                    .InSingletonScope();
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<RevitClashDetective.Models.RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<RevitEventHandler>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ParameterFilterProvider>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Get<IRevitLinkTypesSetter>().SetRevitLinkTypes();

                var revitRepository = kernel.Get<RevitRepository>();
                var selectedMepElements = revitRepository
                    .PickMepElements(OpeningConfig.GetOpeningConfig(revitRepository.Doc).Categories);

                PlaceOpeningTasks(uiApplication, revitRepository, selectedMepElements);
            }
        }
    }
}
