using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Bim4Everyone.RevitFiltration.Ninject;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Services;
using RevitOpeningPlacement.ViewModels.OpeningConfig;
using RevitOpeningPlacement.Views.Settings;
using RevitOpeningPlacement.Views.Settings.Mep;

using Wpf.Ui.Abstractions;

namespace RevitOpeningPlacement;
/// <summary>
/// Команда для задания настроек расстановки заданий на отверстия в файле ВИС
/// </summary>
[Transaction(TransactionMode.Manual)]
public class SetOpeningTasksPlacementConfigCmd : BasePluginCommand {
    public SetOpeningTasksPlacementConfigCmd() {
        PluginName = "Настройки расстановки заданий";
    }


    public void ExecuteCommand(UIApplication uiApplication) {
        Execute(uiApplication);
    }


    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<IFamilyGeometryProvider>()
            .To<FamilyGeometryProvider>()
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

        kernel.UseLogicalFilterFactory();
        kernel.UseLogicalFilterProviderFactory();
        kernel.UseFilterContextParser();

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
        kernel.Bind<INavigationViewPageProvider>()
            .To<NavigationViewPageProvider>()
            .InSingletonScope();
        kernel.Bind<OpeningConfig>()
            .ToMethod(c =>
                OpeningConfig.GetOpeningConfig(uiApplication.ActiveUIDocument.Document)
            )
            .InSingletonScope();
        kernel.Bind<MepNavigatorSettingsViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<MepPlacementSettingsViewModel>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<MepPlacementSettingsPage>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Page.DataContext), c => c.Kernel.Get<MepPlacementSettingsViewModel>());
        kernel.Bind<MepNavigatorSettingsPage>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Page.DataContext), c => c.Kernel.Get<MepNavigatorSettingsViewModel>());
        kernel.BindMainWindow<MepSettingsViewModel, MepSettingsWindow>();
        kernel.Bind<UnionTaskSettingsView>()
            .ToSelf()
            .InTransientScope()
            .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<MepPlacementSettingsViewModel>());
        // диалоговые сервисы регистрируются на вью модель страницы - она их и вызывает.
        // Вью модель окна переиспользует те же экземпляры, чтобы окно привязало к себе именно их
        kernel.UseWpfUIMessageBox<MepPlacementSettingsViewModel>()
            .UseWpfOpenFileDialog<MepPlacementSettingsViewModel>()
            .UseWpfSaveFileDialog<MepPlacementSettingsViewModel>();
        kernel.Bind<ConfigFileService>()
            .ToSelf()
            .InSingletonScope();
        kernel.UseWpfUIThemeUpdater();
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization($"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        kernel.Get<IRevitLinkTypesSetter>().SetRevitLinkTypes();

        Notification(kernel.Get<MepSettingsWindow>());
    }
}
