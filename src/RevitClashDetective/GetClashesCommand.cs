using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitClashDetective.Models;
using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

using Wpf.Ui;

namespace RevitClashDetective;
[Transaction(TransactionMode.Manual)]
public class GetClashesCommand : BasePluginCommand {
    public GetClashesCommand() {
        PluginName = "Навигатор";
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
        kernel.Bind<ReportsViewModel>()
            .ToSelf()
            .InSingletonScope()
            .WithConstructorArgument("selectedFile", (string) null);
        kernel.Bind<IContentDialogService>()
            .To<ContentDialogService>()
            .InSingletonScope();
        kernel.Bind<SettingsConfig>()
            .ToMethod(c => SettingsConfig.GetSettingsConfig(c.Kernel.Get<IConfigSerializer>()));
        kernel.UseWpfOpenFileDialog<ReportsViewModel>(
            filter: "NavisClashReport (*.xml)|*.xml|PluginClashReport (*.json)|*.json");
        kernel.UseWpfSaveFileDialog<ReportsViewModel>();
        kernel.UseXtraMessageBox<ReportsViewModel>();
        kernel.Bind<NavigatorView>()
            .ToSelf()
            .InSingletonScope()
            .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<ReportsViewModel>());

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        kernel.Get<NavigatorView>().Show();
    }
}
