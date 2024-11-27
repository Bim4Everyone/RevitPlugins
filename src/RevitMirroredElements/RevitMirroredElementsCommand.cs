using System.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;

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
                Document document = uiApplication.ActiveUIDocument.Document;
                View activeView = document.ActiveView;

                if(!IsSupportedView(activeView)) {
                    TaskDialog.Show("Ошибка", "Данный плагин не поддерживает работу в текущем виде.");
                    return;
                }

                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                var mainWindow = kernel.Bind<MainWindow>()
                    .ToSelf()
                    .InTransientScope()
                    .WithPropertyValue(nameof(MainWindow.DataContext), c => c.Kernel.Get<MainViewModel>())
                    .WithPropertyValue(nameof(MainWindow.Title), PluginName);

                Notification(kernel.Get<MainWindow>());
            }

        }

        private bool IsSupportedView(View view) {
            var supportedViewTypes = new[] {
                ViewType.ThreeD,
                ViewType.FloorPlan,
                ViewType.Section,
                ViewType.Elevation,
                ViewType.CeilingPlan,
                ViewType.Schedule
            };

            return supportedViewTypes.Contains(view.ViewType);
        }
    }
}
