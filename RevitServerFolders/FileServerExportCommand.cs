using System.Windows;

using Autodesk.Revit.Attributes;

using dosymep.Bim4Everyone;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.SimpleServices;

using Ninject;

using RevitServerFolders.Models;
using RevitServerFolders.ViewModels;
using RevitServerFolders.Views;

namespace RevitServerFolders {
    [Transaction(TransactionMode.Manual)]
    internal sealed class FileServerExportCommand : BasePluginCommand {
        public FileServerExportCommand() {
            PluginName = "Экспорт rvt файлов из RS";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();
					
                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());
				
                kernel.Bind<MainViewModel>().ToSelf();
                kernel.Bind<MainWindow>().ToSelf()
                    .WithPropertyValue(nameof(Window.Title), PluginName)
                    .WithPropertyValue(nameof(Window.DataContext),
                        c => c.Kernel.Get<MainViewModel>());
				
                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
