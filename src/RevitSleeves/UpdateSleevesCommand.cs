using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;
using RevitClashDetective.Models.Interfaces;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Services.Core;
using RevitSleeves.Services.Placing.FamilySymbolFinder;
using RevitSleeves.Services.Update;
using RevitSleeves.ViewModels.Placing;
using RevitSleeves.Views.Placing;

namespace RevitSleeves;


[Transaction(TransactionMode.Manual)]
internal class UpdateSleevesCommand : BasePluginCommand {
    public UpdateSleevesCommand() {
        PluginName = "Обновление гильз";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RevitEventHandler>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<ParameterFilterProvider>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<IView3DProvider>()
            .To<SleeveView3dProvider>()
            .InSingletonScope();
        kernel.Bind<ISleeveUpdaterService>()
            .To<SleeveUpdaterService>()
            .InSingletonScope();
        kernel.Bind<IErrorsService>()
            .To<ErrorsService>()
            .InSingletonScope();
        kernel.Bind<IFamilySymbolFinder>()
            .To<FamilySymbolFinder>()
            .InSingletonScope();
        kernel.Bind<IDocumentChecker>()
            .To<DocumentChecker>()
            .InSingletonScope();

        kernel.Bind<SleevePlacementSettingsConfig>()
            .ToMethod(c => SleevePlacementSettingsConfig.GetPluginConfig(
                new RevitClashConfigSerializer(
                    new RevitClashesSerializationBinder(), uiApplication.ActiveUIDocument.Document)));

        kernel.BindOtherWindow<PlacingErrorsViewModel, PlacingErrorsWindow>();

        kernel.UseWpfUIThemeUpdater();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Run(kernel);

        Notification(true);
    }

    private void Run(IKernel kernel) {
        kernel.Get<IDocumentChecker>().CheckDocument();

        var repo = kernel.Get<RevitRepository>();
        var localizationService = kernel.Get<ILocalizationService>();

        string msg = localizationService.GetLocalizedString("Transaction.SleevesUpdating");
        using var trans = repo.Document.StartTransaction(msg);

        var sleeves = repo.GetSleeves();
        using var dialogUpdate = kernel.Get<IProgressDialogService>();
        dialogUpdate.StepValue = 50;
        dialogUpdate.MaxValue = sleeves.Count;
        dialogUpdate.DisplayTitleFormat = $"{msg} [{{0}}/{{1}}] ...";
        var progressUpdate = dialogUpdate.CreateProgress();
        var ctUpdate = dialogUpdate.CreateCancellationToken();
        dialogUpdate.Show();

        kernel.Get<ISleeveUpdaterService>().UpdateSleeves(sleeves, progressUpdate, ctUpdate);
        trans.Commit();

        if(kernel.Get<IErrorsService>().ContainsErrors()) {
            kernel.Get<PlacingErrorsWindow>().Show();
        }
    }
}
