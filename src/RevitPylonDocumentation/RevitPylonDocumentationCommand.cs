using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.ViewModels;
using RevitPylonDocumentation.Views;

namespace RevitPylonDocumentation {
    [Transaction(TransactionMode.Manual)]
    public class RevitPylonDocumentationCommand : BasePluginCommand {
        public RevitPylonDocumentationCommand() {
            PluginName = "Создание документации пилонов";
        }

        protected override void Execute(UIApplication uiApplication) {

            using(IKernel kernel = uiApplication.CreatePlatformServices()) {
                kernel.Bind<RevitRepository>()
                    .ToSelf()
                    .InSingletonScope();

                // Настройка конфигурации плагина
                kernel.Bind<PluginConfig>()
                    .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

                // Используем сервис обновления тем для WinUI
                kernel.UseWpfUIThemeUpdater();

                // Настройка запуска окна
                kernel.BindMainWindow<MainViewModel, MainWindow>();

                // Настройка локализации,
                // получение имени сборки откуда брать текст
                string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

                // Настройка локализации,
                // установка дефолтной локализации "ru-RU"
                kernel.UseWpfLocalization(
                    $"/{assemblyName};component/assets/Localization/Language.xaml",
                    CultureInfo.GetCultureInfo("ru-RU"));


                // Вызывает стандартное уведомление
                Notification(kernel.Get<MainWindow>());
            }
        }
    }
}
