using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitGenLookupTables.Models;
using RevitGenLookupTables.Services;
using RevitGenLookupTables.ViewModels;
using RevitGenLookupTables.Views;

using Wpf.Ui;

namespace RevitGenLookupTables {
    [Transaction(TransactionMode.Manual)]
    public class RevitGenLookupTablesCommand : BasePluginCommand {
        public RevitGenLookupTablesCommand() {
            PluginName = "Генерация таблицы выбора";
        }

        protected override void Execute(UIApplication uiApplication) {
            // Создание контейнера зависимостей плагина с сервисами из платформы
            using IKernel kernel = uiApplication.CreatePlatformServices();

            // Настройка доступа к Revit
            kernel.Bind<RevitRepository>()
                .ToSelf()
                .InSingletonScope();

            // Настройка конфигурации плагина
            kernel.Bind<PluginConfig>()
                .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

            // Используем сервис обновления тем для WinUI
            kernel.UseWpfUIThemeUpdater();
            
            // Используем сервис месседж боксов
            kernel.UseWpfUIMessageBox();

            // Настройка запуска окна
            kernel.BindMainWindow<MainViewModel, MainWindow>();

            // Настройка локализации,
            // получение имени сборки откуда брать текст
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            // Настройка локализации,
            // установка дефолтной локализации "ru-RU"
            kernel.UseWpfLocalization(
                $"/{assemblyName};component/assets/localizations/Language.xaml",
                CultureInfo.GetCultureInfo("ru-RU"));

            kernel.Bind<IContentDialogService>()
                .To<ContentDialogService>()
                .InSingletonScope();

            kernel.Bind<ISelectFamilyParamsService>()
                .To<SelectFamilyParamsService>()
                .InTransientScope();

            // Вызывает стандартное уведомление
            Check(kernel);
            Notification(kernel.Get<MainWindow>());
        }

        private void Check(IKernel kernel) {
            var revitRepository = kernel.Get<RevitRepository>();
            if(!revitRepository.IsFamilyDocument) {
                var messageBoxService = kernel.Get<IMessageBoxService>();
                var localizationService = kernel.Get<ILocalizationService>();

                messageBoxService.Show(
                    localizationService.GetLocalizedString("Startup.NotFamilyMessage"),
                    localizationService.GetLocalizedString("Startup.NotFamilyTitle"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                
                throw new OperationCanceledException();
            }
        }
    }
}
