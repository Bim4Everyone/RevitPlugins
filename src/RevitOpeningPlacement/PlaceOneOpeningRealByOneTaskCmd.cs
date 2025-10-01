using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
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

namespace RevitOpeningPlacement;
/// <summary>
/// Команда для размещения одного чистового отверстия в АР/КР по одному полученному заданию на отверстия из связи ВИС
/// </summary>
[Transaction(TransactionMode.Manual)]
public class PlaceOneOpeningRealByOneTaskCmd : OpeningRealPlacerCmd {
    public PlaceOneOpeningRealByOneTaskCmd() : base("Принять одно задание на отверстие") { }


    public void ExecuteCommand(UIApplication uiApplication) {
        Execute(uiApplication);
    }


    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
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
                placer.PlaceSingleOpeningByOneTask();
                break;
            }

            case DocTypeEnum.KR: {
                if(!ModelCorrect(new RealOpeningsKrChecker(revitRepository))) {
                    return;
                }
                var config = OpeningRealsKrConfig.GetOpeningConfig(revitRepository.Doc);
                var placer = new RealOpeningKrPlacer(revitRepository, config);
                placer.PlaceSingleOpeningByOneTask();
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
