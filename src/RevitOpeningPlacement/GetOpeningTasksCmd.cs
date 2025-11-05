using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Navigator.Checkers;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.Services;
using RevitOpeningPlacement.ViewModels.Links;
using RevitOpeningPlacement.ViewModels.Navigator;
using RevitOpeningPlacement.Views;

namespace RevitOpeningPlacement;
/// <summary>
/// Команда для просмотра размещенных в текущем файле исходящих заданий на отверстия 
/// и полученных из связей входящих заданий
/// </summary>
[Transaction(TransactionMode.Manual)]
public class GetOpeningTasksCmd : BasePluginCommand {
    public GetOpeningTasksCmd() {
        PluginName = "Навигатор по заданиям";
    }


    public void ExecuteCommand(UIApplication uiApplication) {
        Execute(uiApplication);
    }


    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<UIApplication>()
            .ToSelf()
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
        kernel.Bind<IDocTypesHandler>()
            .To<DocTypesHandler>()
            .InSingletonScope();
        kernel.Bind<LinksSelectorViewModel>()
            .ToSelf()
            .InTransientScope();
        kernel.Bind<LinksSelectorWindow>()
            .ToSelf()
            .InTransientScope()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<LinksSelectorViewModel>());
        kernel.UseWpfUIThemeUpdater();
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization($"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var repo = kernel.Get<RevitRepository>();

        if(!ModelCorrect(repo)) {
            return;
        }
        GetOpeningsTask(kernel);
    }


    /// <summary>
    /// Логика вывода окна навигатора по заданиям на отверстия в зависимости от раздела проекта
    /// </summary>
    private void GetOpeningsTask(IKernel kernel) {
        var bimPartsHandler = kernel.Get<IDocTypesHandler>();
        var activeDoc = kernel.Get<UIApplication>().ActiveUIDocument.Document;
        var docType = bimPartsHandler.GetDocType(activeDoc);
        switch(docType) {
            case DocTypeEnum.AR:
                GetOpeningsTaskInDocumentAR(kernel);
                break;
            case DocTypeEnum.KR:
                GetOpeningsTaskInDocumentKR(kernel);
                break;
            case DocTypeEnum.MEP:
                GetOpeningsTaskInDocumentMEP(kernel);
                break;
            case DocTypeEnum.KOORD:
                GetOpeningsTaskInDocumentKoord(kernel);
                break;
            default:
                GetOpeningsTaskInDocumentNotDefined(kernel);
                break;
        }
    }

    /// <summary>
    /// Логика вывода окна навигатора по заданиям на отверстия в файле архитектуры
    /// </summary>
    private void GetOpeningsTaskInDocumentAR(IKernel kernel) {
        kernel.Bind<IDocTypesProvider>()
            .ToMethod(c => {
                return new DocTypesProvider(new DocTypeEnum[] { DocTypeEnum.MEP });
            })
            .InSingletonScope();
        kernel.Bind<IRevitLinkTypesSetter>()
            .To<UserSelectedLinksSetter>()
            .InTransientScope();
        kernel.Bind<IConstantsProvider>()
            .To<ConstantsProvider>()
            .InSingletonScope();
        kernel.Bind<ArchitectureNavigatorForIncomingTasksViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<NavigatorMepIncomingView>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<ArchitectureNavigatorForIncomingTasksViewModel>())
            .WithPropertyValue(nameof(Window.Title), PluginName);

        kernel.Get<IRevitLinkTypesSetter>().SetRevitLinkTypes();

        var window = kernel.Get<NavigatorMepIncomingView>();
        var uiApplication = kernel.Get<UIApplication>();
        var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };
        window.Show();
    }


    private bool ModelCorrect(RevitRepository revitRepository) {
        var checker = new NavigatorCheckers(revitRepository);
        var errors = checker.GetErrorTexts();
        if(errors == null || errors.Count == 0) {
            return true;
        }

        TaskDialog.Show("BIM", $"{string.Join($"{Environment.NewLine}", errors)}");
        return false;
    }

    private KrNavigatorMode GetKrNavigatorMode() {
        var navigatorModeDialog = new TaskDialog("Навигатор по заданиям для КР") {
            MainInstruction = "Режим навигатора:"
        };
        navigatorModeDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Задания от АР");
        navigatorModeDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Задания от ВИС");
        navigatorModeDialog.CommonButtons = TaskDialogCommonButtons.Close;
        navigatorModeDialog.DefaultButton = TaskDialogResult.Close;
        var result = navigatorModeDialog.Show();
        return result switch {
            TaskDialogResult.CommandLink1 => KrNavigatorMode.IncomingAr,
            TaskDialogResult.CommandLink2 => KrNavigatorMode.IncomingMep,
            _ => throw new OperationCanceledException(),
        };
    }

    /// <summary>
    /// Логика вывода окна навигатора по заданиям на отверстия в файле несущих конструкций
    /// </summary>
    private void GetOpeningsTaskInDocumentKR(IKernel kernel) {
        kernel.Bind<OpeningRealsKrConfig>()
            .ToMethod(c => {
                var repo = c.Kernel.Get<RevitRepository>();
                return OpeningRealsKrConfig.GetOpeningConfig(repo.Doc);
            });

        var navigatorMode = GetKrNavigatorMode();
        var config = kernel.Get<OpeningRealsKrConfig>();
        DocTypeEnum[] docTypes;
        switch(navigatorMode) {
            case KrNavigatorMode.IncomingAr: {
                config.PlacementType = OpeningRealKrPlacementType.PlaceByAr;
                config.SaveProjectConfig();
                docTypes = new DocTypeEnum[] { DocTypeEnum.AR };
                break;
            }
            case KrNavigatorMode.IncomingMep: {
                config.PlacementType = OpeningRealKrPlacementType.PlaceByMep;
                config.SaveProjectConfig();
                docTypes = new DocTypeEnum[] { DocTypeEnum.MEP };
                break;
            }
            default:
                throw new OperationCanceledException();
        }
        kernel.Bind<IDocTypesProvider>()
            .ToMethod(c => {
                return new DocTypesProvider(docTypes);
            })
            .InSingletonScope();
        kernel.Bind<IRevitLinkTypesSetter>()
            .To<UserSelectedLinksSetter>()
            .InTransientScope();
        kernel.Bind<IConstantsProvider>()
            .To<ConstantsProvider>()
            .InSingletonScope();
        kernel.Bind<ConstructureNavigatorForIncomingTasksViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<NavigatorArIncomingView>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<ConstructureNavigatorForIncomingTasksViewModel>())
            .WithPropertyValue(nameof(Window.Title), PluginName);
        kernel.Bind<NavigatorMepToKrIncomingView>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<ConstructureNavigatorForIncomingTasksViewModel>())
            .WithPropertyValue(nameof(Window.Title), PluginName);

        kernel.Get<IRevitLinkTypesSetter>().SetRevitLinkTypes();

        var window = navigatorMode == KrNavigatorMode.IncomingAr
            ? kernel.Get<NavigatorArIncomingView>()
            : (Window) kernel.Get<NavigatorMepToKrIncomingView>();
        var uiApplication = kernel.Get<UIApplication>();
        var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };
        window.Show();
    }

    private void GetOpeningsTaskInDocumentMEP(IKernel kernel) {
        kernel.Bind<IDocTypesProvider>()
            .ToMethod(c => {
                return new DocTypesProvider(new DocTypeEnum[] { DocTypeEnum.AR, DocTypeEnum.KR });
            })
            .InSingletonScope();
        kernel.Bind<IRevitLinkTypesSetter>()
            .To<UserSelectedLinksSetter>()
            .InTransientScope();
        kernel.Bind<OpeningConfig>()
            .ToMethod(c => {
                var repo = c.Kernel.Get<RevitRepository>();
                return OpeningConfig.GetOpeningConfig(repo.Doc);
            });
        kernel.Bind<IConstantsProvider>()
            .To<ConstantsProvider>()
            .InSingletonScope();
        kernel.Bind<ISolidProviderUtils>()
            .To<SolidProviderUtils>()
            .InSingletonScope();
        kernel.Bind<IOpeningInfoUpdater<OpeningMepTaskOutcoming>>()
            .To<MepTaskOutcomingInfoUpdater>()
            .InTransientScope();
        kernel.Bind<ILengthConverter>()
            .To<LengthConverterService>()
            .InSingletonScope();
        kernel.Bind<OutcomingTaskGeometryProvider>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<GeometryUtils>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<PipeOffsetFinder>()
            .ToSelf()
            .InTransientScope();
        kernel.Bind<DuctOffsetFinder>()
            .ToSelf()
            .InTransientScope();
        kernel.Bind<ConduitOffsetFinder>()
            .ToSelf()
            .InTransientScope();
        kernel.Bind<CableTrayOffsetFinder>()
            .ToSelf()
            .InTransientScope();
        kernel.Bind<FamilyInstanceOffsetFinder>()
            .ToSelf()
            .InTransientScope();
        kernel.Bind<IOutcomingTaskOffsetFinder>()
            .To<ElementOffsetFinder>()
            .InTransientScope();

        kernel.Bind<MepNavigatorForOutcomingTasksViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<NavigatorMepOutcomingView>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<MepNavigatorForOutcomingTasksViewModel>())
            .WithPropertyValue(nameof(Window.Title), PluginName);

        kernel.Get<IRevitLinkTypesSetter>().SetRevitLinkTypes();

        var window = kernel.Get<NavigatorMepOutcomingView>();
        var uiApplication = kernel.Get<UIApplication>();
        var helper = new WindowInteropHelper(window) { Owner = uiApplication.MainWindowHandle };
        window.Show();
    }

    /// <summary>
    /// Логика вывода окна навигатора по заданиям на отверстия в файле с неопределенным разделом проектирования
    /// </summary>
    private void GetOpeningsTaskInDocumentNotDefined(IKernel kernel) {
        var revitRepository = kernel.Get<RevitRepository>();
        TaskDialog.Show("BIM",
            $"Название файла: \"{revitRepository.GetDocumentName()}\" не удовлетворяет BIM стандарту А101. " +
            $"Скорректируйте название и запустите команду снова.");
        throw new OperationCanceledException();
    }

    /// <summary>
    /// Логика вывода окна навигатора по заданиям на отверстия в координационном файле
    /// </summary>
    private void GetOpeningsTaskInDocumentKoord(IKernel kernel) {
        var revitRepository = kernel.Get<RevitRepository>();
        TaskDialog.Show(
            "BIM",
            $"Команда не может быть запущена в координационном файле \"{revitRepository.GetDocumentName()}\"");
        throw new OperationCanceledException();
    }
}

/// <summary>
/// Перечисление режимов навигатора по заданиям на отверстия в файле КР
/// </summary>
internal enum KrNavigatorMode {
    /// <summary>
    /// Просмотр входящих заданий от АР
    /// </summary>
    IncomingAr,
    /// <summary>
    /// Просмотр входящих заданий от ВИС
    /// </summary>
    IncomingMep
}
