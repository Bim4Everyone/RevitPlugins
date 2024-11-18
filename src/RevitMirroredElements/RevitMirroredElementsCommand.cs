using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.Templates;

using Ninject;

using RevitMirroredElements.Models;
using RevitMirroredElements.ViewModels;
using RevitMirroredElements.Views;

namespace RevitMirroredElements {
    [Transaction(TransactionMode.Manual)]
    public class RevitMirroredElementsCommand : BasePluginCommand {
        public RevitMirroredElementsCommand() {
            PluginName = "Проверка на зеркальность";
        }

        protected override void Execute(UIApplication uiApplication) {
            using(IKernel kernel = uiApplication.CreatePlatformServices()) {

                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();


                kernel.Bind<CategoriesViewModel>()
                   .ToSelf()
                   .InTransientScope()
                   .WithPropertyValue(nameof(Window.Title), PluginName)
                   .WithPropertyValue(nameof(Window.DataContext), c => c.Kernel.Get<CategoriesViewModel>())
                   .WithPropertyValue(nameof(Window.Owner), c => c.Kernel.Get<MainWindow>());


                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                var mainWindow = kernel.Get<MainWindow>();
                var mainViewModel = kernel.Get<MainViewModel>();
                mainViewModel.MainWindow = mainWindow;
                mainWindow.DataContext = mainViewModel;
                mainWindow.Title = PluginName;

                UpdateParams(uiApplication);

                Notification(mainWindow);
            }
        }
        private static void UpdateParams(UIApplication uiApplication) {
            ProjectParameters projectParameters = ProjectParameters.Create(uiApplication.Application);
            projectParameters.SetupRevitParams(uiApplication.ActiveUIDocument.Document,
                SharedParamsConfig.Instance.ElementMirroring);
        }
    }
}
