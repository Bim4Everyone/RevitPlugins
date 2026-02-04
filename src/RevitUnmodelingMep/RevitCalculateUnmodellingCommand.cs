using System;
using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitUnmodelingMep.Models;
using RevitUnmodelingMep.ViewModels;

namespace RevitUnmodelingMep {
    [Transaction(TransactionMode.Manual)]
    public class RevitCalculateUnmodellingCommand : BasePluginCommand {
        public RevitCalculateUnmodellingCommand() {
            PluginName = "Расчет расходников";
        }

        protected override void Execute(UIApplication uiApplication) {
            // Создание контейнера зависимостей плагина с сервисами из платформы
            using IKernel kernel = uiApplication.CreatePlatformServices();

            // Настройка локализации,
            // получение имени сборки откуда брать текст
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            // Настройка локализации,
            // установка дефолтной локализации "ru-RU"
            kernel.UseWpfLocalization(
                $"/{assemblyName};component/assets/Localization/Language.xaml",
                CultureInfo.GetCultureInfo("ru-RU"));

            // Настройка доступа к Revit
            kernel.Bind<RevitRepository>()
                .ToSelf()
                .InSingletonScope();

            kernel.Bind<Document>()
                .ToMethod(ctx => ctx.Kernel.Get<UIApplication>().ActiveUIDocument.Document)
                .InSingletonScope();

            kernel.UseWpfUIMessageBox<MainViewModel>();

            kernel.Bind<VisSettingsStorage>()
                .ToSelf()
                .InSingletonScope();

            kernel.Bind<UnmodelingCreator>()
                .ToSelf()
                .InSingletonScope();

            kernel.Bind<UnmodelingCalculator>()
                .ToSelf()
                .InSingletonScope();

            // Настройка конфигурации плагина
            kernel.Bind<PluginConfig>()
                .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

            var localizationService = kernel.Get<ILocalizationService>();

            var servise = GetPlatformService<IMessageBoxService>();
            CheckDocument(uiApplication.ActiveUIDocument.Document, servise, localizationService);

            var repository = kernel.Get<RevitRepository>();
            repository.CalculateUnmodeling(titleKey => CreatePercentProgressDialog(titleKey, localizationService));
        }

        private void CheckDocument(
            Document document,
            IMessageBoxService service,
            ILocalizationService localizationService) {
            if(!document.IsWorkshared)
                ShowErrorAndCancel("MainWindow.WorkSharedError", service, localizationService);

            if(document.IsFamilyDocument)
                ShowErrorAndCancel("MainWindow.DocTypeError", service, localizationService);
        }

        private void ShowErrorAndCancel(
            string messageKey,
            IMessageBoxService service,
            ILocalizationService localizationService) {
            string report = localizationService.GetLocalizedString(messageKey);
            string title = localizationService.GetLocalizedString("MainWindow.Title");

            service.Show(
                report,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            throw new OperationCanceledException();
        }

        private IProgressDialogService CreatePercentProgressDialog(
            string titleKey,
            ILocalizationService localizationService) {
            var dialog = GetPlatformService<IProgressDialogService>();
            dialog.StepValue = 1;
            dialog.DisplayTitleFormat =
                $"{localizationService.GetLocalizedString(titleKey)} [{{0}}%]";
            dialog.MaxValue = 100;
            dialog.Show();
            return dialog;
        }
    }
}
