using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitCreateViewSheet.Models;
using RevitCreateViewSheet.Services;
using RevitCreateViewSheet.ViewModels;
using RevitCreateViewSheet.Views;

namespace RevitCreateViewSheet {
    [Transaction(TransactionMode.Manual)]
    public class CreateViewSheetCommand : BasePluginCommand {
        public CreateViewSheetCommand() {
            PluginName = "Менеджер листов";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<SheetsSaver>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.UseWpfUIThemeUpdater();

                kernel.BindMainWindow<MainViewModel, MainWindow>();

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                kernel.UseWpfLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));
                kernel.UseWpfUIProgressDialog<MainViewModel>();

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
