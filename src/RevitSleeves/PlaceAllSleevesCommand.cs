using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
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
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;
using RevitSleeves.Services.Placing;
using RevitSleeves.Services.Placing.FamilySymbolFinder;
using RevitSleeves.Services.Placing.Intersections;
using RevitSleeves.Services.Placing.LevelFinder;
using RevitSleeves.Services.Placing.ParamsSetter;
using RevitSleeves.Services.Placing.ParamsSetterFinder;
using RevitSleeves.Services.Placing.PlacingOptsProvider;
using RevitSleeves.Services.Placing.PointFinder;
using RevitSleeves.Services.Placing.RotationFinder;
using RevitSleeves.ViewModels.Filtration;
using RevitSleeves.ViewModels.Placing;
using RevitSleeves.Views.Filtration;
using RevitSleeves.Views.Placing;

namespace RevitSleeves;
[Transaction(TransactionMode.Manual)]
internal class PlaceAllSleevesCommand : BasePluginCommand {
    public PlaceAllSleevesCommand() {
        PluginName = "Расставить все гильзы";
    }


    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

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
        kernel.Bind<SleevePlacementSettingsConfig>()
            .ToMethod(c => SleevePlacementSettingsConfig.GetPluginConfig(
                new RevitClashConfigSerializer(
                    new RevitClashesSerializationBinder(), uiApplication.ActiveUIDocument.Document)));

        BindServices(kernel);
        BindWindows(kernel);

        kernel.UseWpfUIThemeUpdater();

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Run(kernel);

        Notification(true);
    }

    private void Run(IKernel kernel) {
        var repo = kernel.Get<RevitRepository>();
        var oldSleeves = repo.GetSleeves();

        var localizationService = kernel.Get<ILocalizationService>();
        var cleanupService = kernel.Get<ISleeveCleanupService>();

        var newSleeves = CreateSleeves(kernel);

        var cleanedSleeves = CleanupSleeves(kernel, oldSleeves, newSleeves);

        MergeSleeves(kernel, cleanedSleeves);

        if(kernel.Get<SleevePlacementSettingsConfig>().ShowPlacingErrors
            && kernel.Get<IPlacingErrorsService>().ContainsErrors()) {
            kernel.Get<PlacingErrorsWindow>().Show();
        }
    }

    private ICollection<SleeveModel> CreateSleeves(IKernel kernel) {

        var repo = kernel.Get<RevitRepository>();
        var localizationService = kernel.Get<ILocalizationService>();
        var cleanupService = kernel.Get<ISleeveCleanupService>();

        ICollection<SleeveModel> newSleeves;
        try {
            repo.Application.FailuresProcessing += cleanupService.FailureProcessor;
            var opts = kernel.Get<ISleevePlacingOptsService>().GetOpts();
            string msg = localizationService.GetLocalizedString("Transaction.SleevesPlacing");
            using var placeTrans = repo.Document.StartTransaction(msg);

            using var dialogCreate = kernel.Get<IProgressDialogService>();
            dialogCreate.StepValue = 50;
            dialogCreate.MaxValue = opts.Count;
            dialogCreate.DisplayTitleFormat = $"{msg} [{{0}}/{{1}}] ...";
            var progressCreate = dialogCreate.CreateProgress();
            var ctCreate = dialogCreate.CreateCancellationToken();
            dialogCreate.Show();

            newSleeves = kernel.Get<ISleevePlacerService>().PlaceSleeves(opts, progressCreate, ctCreate);
            var options = placeTrans.GetFailureHandlingOptions()
                .SetForcedModalHandling(false)
                .SetDelayedMiniWarnings(true);
            placeTrans.Commit(options);
        } finally {
            repo.Application.FailuresProcessing -= cleanupService.FailureProcessor;
        }
        return newSleeves;
    }

    private ICollection<SleeveModel> CleanupSleeves(IKernel kernel,
        ICollection<SleeveModel> oldSleeves,
        ICollection<SleeveModel> newSleeves) {

        var repo = kernel.Get<RevitRepository>();
        var localizationService = kernel.Get<ILocalizationService>();
        var cleanupService = kernel.Get<ISleeveCleanupService>();

        ICollection<SleeveModel> cleanedSleeves;
        string msg = localizationService.GetLocalizedString("Transaction.SleevesCleanup");
        using var cleanupTrans = repo.Document.StartTransaction(msg);

        using var dialogCleanup = kernel.Get<IProgressDialogService>();
        dialogCleanup.StepValue = 50;
        dialogCleanup.MaxValue = newSleeves.Count;
        dialogCleanup.DisplayTitleFormat = $"{msg} [{{0}}/{{1}}] ...";
        var progressCleanup = dialogCleanup.CreateProgress();
        var ctCleanup = dialogCleanup.CreateCancellationToken();
        dialogCleanup.Show();

        cleanedSleeves = cleanupService.CleanupSleeves(oldSleeves, newSleeves, progressCleanup, ctCleanup);
        cleanupTrans.Commit();
        return cleanedSleeves;
    }

    private void MergeSleeves(IKernel kernel, ICollection<SleeveModel> cleanedSleeves) {
        var repo = kernel.Get<RevitRepository>();
        var localizationService = kernel.Get<ILocalizationService>();
        var cleanupService = kernel.Get<ISleeveCleanupService>();

        string msg = localizationService.GetLocalizedString("Transaction.SleevesMerging");
        using var mergeTrans = repo.Document.StartTransaction(msg);

        using var dialogMerge = kernel.Get<IProgressDialogService>();
        dialogMerge.StepValue = 50;
        dialogMerge.MaxValue = cleanedSleeves.Count;
        dialogMerge.DisplayTitleFormat = $"{msg} [{{0}}/{{1}}] ...";
        var progressMerge = dialogMerge.CreateProgress();
        var ctMerge = dialogMerge.CreateCancellationToken();
        dialogMerge.Show();

        kernel.Get<ISleeveMergeService>().MergeSleeves(cleanedSleeves, progressMerge, ctMerge);
        mergeTrans.Commit();
    }

    private void BindServices(IKernel kernel) {
        BindCoreServices(kernel);
        BindElementsServices(kernel);

        BindFamilySymbolFinders(kernel);
        BindIntersectionsFinders(kernel);
        BindLevelFinders(kernel);
        BindParamsSetters(kernel);
        BindParamsSetterFinders(kernel);
        BindPlacingOptsProviders(kernel);
        BindPointFinders(kernel);
        BindRotationFinders(kernel);
    }

    private void BindWindows(IKernel kernel) {
        kernel.BindOtherWindow<PlacingErrorsViewModel, PlacingErrorsWindow>();
    }

    private void BindCoreServices(IKernel kernel) {
        kernel.Bind<ISleevePlacingOptsService>()
            .To<SleevePlacingOptsService>()
            .InSingletonScope();
        kernel.Bind<ISleevePlacerService>()
            .To<SleevePlacerService>()
            .InSingletonScope();
        kernel.Bind<ISleeveCleanupService>()
            .To<SleeveCleanupService>()
            .InSingletonScope();
        kernel.Bind<ISleeveMergeService>()
            .To<SleeveMergeService>()
            .InSingletonScope();
        kernel.Bind<IPlacingErrorsService>()
            .To<PlacingErrorsService>()
            .InSingletonScope();
        kernel.Bind<IOpeningGeometryProvider>()
            .To<OpeningGeometryProvider>()
            .InSingletonScope();
        kernel.Bind<IGeometryUtils>()
            .To<GeometryUtils>()
            .InSingletonScope();
        kernel.Bind<IView3DProvider>()
            .To<SleeveView3dProvider>()
            .InSingletonScope();
    }

    protected virtual void BindElementsServices(IKernel kernel) {
        kernel.Bind<IMepElementsProvider>()
            .To<AllMepElementsProvider>()
            .InSingletonScope();
        kernel.Bind<IStructureLinksProvider>()
            .To<UserSelectedStructureLinks>()
            .InSingletonScope();
        kernel.BindOtherWindow<StructureLinksSelectorViewModel, StructureLinksSelectorWindow>();
    }

    private void BindFamilySymbolFinders(IKernel kernel) {
        kernel.Bind<IFamilySymbolFinder<SleeveMergeModel>>()
            .To<MergeModelFamilySymbolFinder>()
            .InSingletonScope();
        kernel.Bind<IFamilySymbolFinder<ClashModel<Pipe, Floor>>>()
            .To<PipeFloorFamilySymbolFinder>()
            .InSingletonScope();
        kernel.Bind<IFamilySymbolFinder<ClashModel<Pipe, Wall>>>()
            .To<PipeWallFamilySymbolFinder>()
            .InSingletonScope();
    }

    private void BindIntersectionsFinders(IKernel kernel) {
        kernel.Bind<IClashFinder<Pipe, Floor>>()
            .To<PipeFloorIntersectionsFinder>()
            .InSingletonScope();
        kernel.Bind<IClashFinder<Pipe, Wall>>()
            .To<PipeWallIntersectionsFinder>()
            .InSingletonScope();
    }

    private void BindLevelFinders(IKernel kernel) {
        kernel.Bind<ILevelFinder<SleeveMergeModel>>()
            .To<MergeModelLevelFinder>()
            .InSingletonScope();
        kernel.Bind<ILevelFinder<ClashModel<Pipe, Floor>>>()
            .To<PipeFloorLevelFinder>()
            .InSingletonScope();
        kernel.Bind<ILevelFinder<ClashModel<Pipe, Wall>>>()
            .To<PipeWallLevelFinder>()
            .InSingletonScope();
    }

    private void BindParamsSetters(IKernel kernel) {
        kernel.Bind<IParamsSetter<SleeveMergeModel>>()
            .To<MergeModelParamsSetter>()
            .InTransientScope();
        kernel.Bind<IParamsSetter<ClashModel<Pipe, Floor>>>()
            .To<PipeFloorParamsSetter>()
            .InTransientScope();
        kernel.Bind<IParamsSetter<ClashModel<Pipe, Wall>>>()
            .To<PipeWallParamsSetter>()
            .InTransientScope();
    }

    private void BindParamsSetterFinders(IKernel kernel) {
        kernel.Bind<IParamsSetterFinder<SleeveMergeModel>>()
            .To<MergeModelParamsSetterFinder>()
            .InSingletonScope();
        kernel.Bind<IParamsSetterFinder<ClashModel<Pipe, Floor>>>()
            .To<PipeFloorParamsSetterFinder>()
            .InSingletonScope();
        kernel.Bind<IParamsSetterFinder<ClashModel<Pipe, Wall>>>()
            .To<PipeWallParamsSetterFinder>()
            .InSingletonScope();
    }

    private void BindPlacingOptsProviders(IKernel kernel) {
        kernel.Bind<IPlacingOptsProvider<SleeveMergeModel>>()
            .To<MergeModelPlacingOptsProvider>()
            .InSingletonScope();
        kernel.Bind<IPlacingOptsProvider<ClashModel<Pipe, Floor>>>()
            .To<PipeFloorPlacingOptsProvider>()
            .InSingletonScope();
        kernel.Bind<IPlacingOptsProvider<ClashModel<Pipe, Wall>>>()
            .To<PipeWallPlacingOptsProvider>()
            .InSingletonScope();
    }

    private void BindPointFinders(IKernel kernel) {
        kernel.Bind<IPointFinder<SleeveMergeModel>>()
            .To<MergeModelPointFinder>()
            .InSingletonScope();
        kernel.Bind<IPointFinder<ClashModel<Pipe, Floor>>>()
            .To<PipeFloorPointFinder>()
            .InSingletonScope();
        kernel.Bind<IPointFinder<ClashModel<Pipe, Wall>>>()
            .To<PipeWallPointFinder>()
            .InSingletonScope();
    }

    private void BindRotationFinders(IKernel kernel) {
        kernel.Bind<IRotationFinder<SleeveMergeModel>>()
            .To<MergeModelRotationFinder>()
            .InSingletonScope();
        kernel.Bind<IRotationFinder<ClashModel<Pipe, Floor>>>()
            .To<PipeFloorRotationFinder>()
            .InSingletonScope();
        kernel.Bind<IRotationFinder<ClashModel<Pipe, Wall>>>()
            .To<PipeWallRotationFinder>()
            .InSingletonScope();
    }
}
