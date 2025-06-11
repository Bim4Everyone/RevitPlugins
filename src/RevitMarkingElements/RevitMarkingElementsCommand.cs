using System;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitMarkingElements.Models;
using RevitMarkingElements.ViewModels;
using RevitMarkingElements.Views;

namespace RevitMarkingElements;
[Transaction(TransactionMode.Manual)]
public class RevitMarkingElementsCommand : BasePluginCommand {
    public RevitMarkingElementsCommand() {
        PluginName = "Маркировка элементов";
    }

    protected override void Execute(UIApplication uiApplication) {
        // Создание контейнера зависимостей плагина с сервисами из платформы
        using var kernel = uiApplication.CreatePlatformServices();

        var document = uiApplication.ActiveUIDocument.Document;
        var activeView = document.ActiveView;

        // Настройка доступа к Revit
        _ = kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        _ = kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Используем сервис обновления тем для WinUI
        _ = kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        _ = kernel.BindMainWindow<MainViewModel, MainWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        _ = kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var revitRepository = kernel.Get<RevitRepository>();
        var localizationService = kernel.Get<ILocalizationService>();

        ValidateSelectedElements(revitRepository, localizationService);
        Notification(kernel.Get<MainWindow>());
    }

    private void ValidateSelectedElements(RevitRepository revitRepository, ILocalizationService localizationService) {
        var selectedElement = revitRepository.GetSelectedElements();

        if(selectedElement.Count == 0) {
            string title = localizationService.GetLocalizedString("GeneralSettings.ErrorMessage");
            string message = localizationService.GetLocalizedString("GeneralSettings.ErrorNoSelectedElements");

            _ = TaskDialog.Show(title, message);
            throw new OperationCanceledException();
        }
    }
}
