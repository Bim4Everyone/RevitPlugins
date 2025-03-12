using System.Linq;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

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

                // Настройка доступа к Revit
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                // Настройка конфигурации плагина
                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig());

                // Используем сервис обновления тем для WinUI
                kernel.UseWpfUIThemeUpdater();

                // Настройка запуска окна
                kernel.BindMainWindow<MainViewModel, MainWindow>();

                // Вызывает стандартное уведомление
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
