using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using Bim4Everyone.RevitFiltration.Ninject;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitMarkAllDocuments.Models;
using RevitMarkAllDocuments.Services;
using RevitMarkAllDocuments.Services.Export;
using RevitMarkAllDocuments.ViewModels;
using RevitMarkAllDocuments.Views;

using Wpf.Ui.Abstractions;

namespace RevitMarkAllDocuments;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitMarkAllDocumentsCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitMarkAllDocumentsCommand() {
        PluginName = "Маркировать всё";
    }

    protected override void Execute(UIApplication uiApplication) {
        // Создание контейнера зависимостей плагина с сервисами из платформы
        using IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.UseLogicalFilterFactory();
        kernel.UseLogicalFilterProviderFactory();
        kernel.UseFilterContextParser();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        kernel.Bind<INavigationViewPageProvider>()
            .To<NavigationViewPageProvider>()
            .InSingletonScope();

        kernel.Bind<JsonSerializerService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<WindowsService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<CategoryContext>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<DocumentService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ParamValidationService>()
            .ToSelf()
            .InSingletonScope();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<MainViewModel, MainWindow>();
        kernel.BindOtherWindow<CategoriesWindowVM, CategoriesWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));


        var categoriesWindow = kernel.Get<CategoriesWindow>();
        if(categoriesWindow.ShowDialog() == true) {
            Notification(kernel.Get<MainWindow>());
        }
    }
}
