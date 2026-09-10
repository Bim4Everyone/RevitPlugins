using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using Bim4Everyone.RevitFiltration.Ninject;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

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
using RevitOpeningPlacement.Services.Utils;

namespace RevitOpeningPlacement;
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

        kernel.UseLogicalFilterFactory();
        kernel.Bind<RevitClashDetective.Models.RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RevitEventHandler>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ParameterFilterProvider>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ISolidProviderUtils>()
            .To<SolidProviderUtils>()
            .InSingletonScope();
        kernel.Bind<OpeningRealsArConfig>()
            .ToMethod(c => OpeningRealsArConfig.GetOpeningConfig(uiApplication.ActiveUIDocument.Document));
        kernel.Bind<OpeningRealsKrConfig>()
            .ToMethod(c => OpeningRealsKrConfig.GetOpeningConfig(uiApplication.ActiveUIDocument.Document));
        kernel.Bind<RealOpeningKrPlacer>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RealOpeningArPlacer>()
            .ToSelf()
            .InSingletonScope();
        kernel.UseWpfUIThemeUpdater();
        kernel.UseWpfWindowsTheme();
        kernel.Bind<IHasTheme>().To<HasTheme>().InSingletonScope();
        kernel.Bind<IHasLocalization>().To<HasLocalization>().InSingletonScope();
        kernel.UseWpfUIProgressDialog();
        kernel.UseWpfUIMessageBox();
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization($"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var revitRepository = kernel.Get<RevitRepository>();
        var bimPartsHandler = kernel.Get<IDocTypesHandler>();
        var docType = bimPartsHandler.GetDocType(revitRepository.Doc);
        switch(docType) {
            case DocTypeEnum.AR: {
                if(!ModelCorrect(new RealOpeningsArChecker(revitRepository))) {
                    return;
                }

                kernel.Get<RealOpeningArPlacer>().PlaceSingleOpeningsInManyHosts();
                break;
            }

            case DocTypeEnum.KR: {
                if(!ModelCorrect(new RealOpeningsKrChecker(revitRepository))) {
                    return;
                }

                kernel.Get<RealOpeningKrPlacer>().PlaceSingleOpeningsInManyHosts();
                break;
            }

            default: {
                var localization = kernel.Get<ILocalizationService>();
                kernel.Get<IMessageBoxService>()
                    .Show(
                        localization.GetLocalizedString("Errors.ArKrOnly"),
                        localization.GetLocalizedString("OpeningTasks"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                break;
            }
        }
    }
}
