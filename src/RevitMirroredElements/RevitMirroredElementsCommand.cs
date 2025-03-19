using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;
using Ninject.Activation;

using RevitMirroredElements.Interfaces;
using RevitMirroredElements.Models;
using RevitMirroredElements.Services;
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

                kernel.Bind<ICategorySelectionService>()
                    .To<CategorySelectionService>()
                    .InSingletonScope()
                    .WithConstructorArgument<Func<CategoriesWindow>>(ctx => () => ctx.Kernel.Get<CategoriesWindow>());

                // Используем сервис обновления тем для WinUI
                kernel.UseWpfUIThemeUpdater();

                kernel.BindMainWindow<MainViewModel, MainWindow>();
                kernel.BindOtherWindow<CategoriesViewModel, CategoriesWindow>();

                // Настройка локализации,
                // получение имени сборки откуда брать текст
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                // Настройка локализации,
                // установка дефолтной локализации "ru-RU"
                kernel.UseWpfLocalization(
                    $"/{assemblyName};component/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));

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
