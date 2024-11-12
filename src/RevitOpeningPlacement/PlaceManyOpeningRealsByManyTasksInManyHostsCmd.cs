using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.RealOpeningArPlacement;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.Checkers;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.Checkers;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement {
    /// <summary>
    /// Команда для размещения чистовых отверстий АР в выбранных конструкциях, которые пересекаются с выбранными заданиями на отверстия.
    /// При этом для каждого задания создается отдельное чистовое отверстие, то есть объединения не происходит.
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class PlaceManyOpeningRealsByManyTasksInManyHostsCmd : OpeningRealPlacerCmd {
        public PlaceManyOpeningRealsByManyTasksInManyHostsCmd() : base("Авторазмещение отверстий по заданиям") { }


        public void ExecuteCommand(UIApplication uiApplication) {
            Execute(uiApplication);
        }


        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<UIApplication>()
                    .ToSelf()
                    .InSingletonScope();
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

                var revitRepository = kernel.Get<RevitRepository>();
                var bimPartsHandler = kernel.Get<IDocTypesHandler>();
                var docType = bimPartsHandler.GetDocType(revitRepository.Doc);
                switch(docType) {
                    case DocTypeEnum.AR: {
                        if(!ModelCorrect(new RealOpeningsArChecker(revitRepository))) {
                            return;
                        }
                        var placer = new RealOpeningArPlacer(revitRepository);
                        placer.PlaceSingleOpeningsInManyHosts();
                        break;
                    }

                    case DocTypeEnum.KR: {
                        if(!ModelCorrect(new RealOpeningsKrChecker(revitRepository))) {
                            return;
                        }
                        var config = OpeningRealsKrConfig.GetOpeningConfig(revitRepository.Doc);
                        var placer = new RealOpeningKrPlacer(revitRepository, config);
                        placer.PlaceSingleOpeningsInManyHosts();
                        break;
                    }

                    default: {
                        revitRepository.ShowErrorMessage(
                            "Команда предназначена только для АР/КР." +
                            "\nПроверьте наименование файла на соответствие BIM-стандарту A101 или откройте другой файл.");
                        break;
                    }
                }
            }
        }
    }
}
