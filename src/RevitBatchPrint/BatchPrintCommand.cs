using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitBatchPrint.Models;
using RevitBatchPrint.Services;
using RevitBatchPrint.ViewModels;
using RevitBatchPrint.Views;

namespace RevitBatchPrint;

[Transaction(TransactionMode.Manual)]
public class BatchPrintCommand : BasePluginCommand {
    public BatchPrintCommand() {
        PluginName = "Пакетная печать";
    }

    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<PrintManager>()
            .ToConstant(uiApplication.ActiveUIDocument.Document.PrintManager)
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Настройка сервиса окошек сообщений
        kernel.UseWpfUIMessageBox<MainViewModel>();

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
            $"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        kernel.Bind<Document>()
            .ToMethod(c => c.Kernel.Get<RevitRepository>().Document);

        kernel.Bind<IPrinterService>()
            .To<PrinterService>()
            .OnActivation(c => c.Load());

        kernel.Bind<RevitPrint>()
            .ToSelf()
            .InTransientScope();

        kernel.Bind<RevitExportToPdf>()
            .ToSelf()
            .InTransientScope();

        // Вызывает стандартное уведомление
        Notification(kernel.Get<MainWindow>());
    }
}
