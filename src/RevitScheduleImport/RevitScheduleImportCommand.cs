using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using Ninject;

using RevitScheduleImport.Models;
using RevitScheduleImport.Services;

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

                //kernel.Bind<MainViewModel>().ToSelf();
                //kernel.Bind<MainWindow>().ToSelf()
                //    .WithPropertyValue(nameof(Window.DataContext),
                //        c => c.Kernel.Get<MainViewModel>())
                //    .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                //        c => c.Kernel.Get<ILocalizationService>());

                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                var openFileDialog = kernel.Get<IOpenFileDialogService>();
                var dialogResult = openFileDialog.ShowDialog();
                if(dialogResult) {
                    kernel.Get<ScheduleImporter>().ImportSchedule(openFileDialog.File.FullName, out string[] _);
                }
                Notification(dialogResult);

                //kernel.UseXtraLocalization(
                //    $"/{assemblyName};component/Localization/Language.xaml",
                //    CultureInfo.GetCultureInfo("ru-RU"));

                //Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
