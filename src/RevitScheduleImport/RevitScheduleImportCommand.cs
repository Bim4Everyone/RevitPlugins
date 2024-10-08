using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitScheduleImport.Models;
using RevitScheduleImport.Services;
using RevitScheduleImport.ViewModels;

namespace RevitScheduleImport {
    [Transaction(TransactionMode.Manual)]
    public class RevitScheduleImportCommand : BasePluginCommand {
        public RevitScheduleImportCommand() {
            PluginName = "RevitScheduleImport";
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
                kernel.Bind<LengthConverter>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                kernel.Bind<MainViewModel>().ToSelf();

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                kernel.UseXtraMessageBox<MainViewModel>();
                kernel.UseXtraLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));
                var localizationServise = kernel.Get<ILocalizationService>();
                kernel.UseXtraOpenFileDialog<MainViewModel>(
                    title: localizationServise.GetLocalizedString("OpenFileDialog.Title"),
                    filter: localizationServise.GetLocalizedString("OpenFileDialog.Filter")
                    );

                Notification(kernel.Get<MainViewModel>().ExecuteImportCommand());
            }
        }
    }
}
