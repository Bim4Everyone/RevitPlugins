using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitScheduleImport.Models;
using RevitScheduleImport.Services;
using RevitScheduleImport.ViewModels;
using RevitScheduleImport.Views;

namespace RevitScheduleImport {
    [Transaction(TransactionMode.Manual)]
    public class RevitScheduleImportCommand : BasePluginCommand {
        public RevitScheduleImportCommand() {
            PluginName = "Импорт Excel";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ExcelReader>()
                    .ToSelf()
                    .InSingletonScope();
                kernel.Bind<ScheduleImporter>()
                    .ToSelf()
                    .InTransientScope();
                kernel.Bind<Services.LengthConverter>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.UseWpfUIThemeUpdater();

                kernel.BindMainWindow<MainViewModel, MainWindow>();

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                kernel.UseWpfUIMessageBox<MainViewModel>();
                kernel.UseWpfLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));
                var localizationService = kernel.Get<ILocalizationService>();
                kernel.UseXtraOpenFileDialog<MainViewModel>(
                    title: localizationService.GetLocalizedString("OpenFileDialog.Title"),
                    filter: localizationService.GetLocalizedString("OpenFileDialog.Filter")
                    );

                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
