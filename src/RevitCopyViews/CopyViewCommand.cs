using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.Templates;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitCopyViews.Models;
using RevitCopyViews.ViewModels;
using RevitCopyViews.Views;

namespace RevitCopyViews;

[Transaction(TransactionMode.Manual)]
public class CopyViewCommand : BasePluginCommand {
    public CopyViewCommand() {
        PluginName = "Копирование видов";
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

        // Настройка запуска окна
        kernel.BindMainWindow<CopyViewViewModel, CopyViewWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // используем message box без привязки к окну,
        // потому что он вызывается до запуска основного окна
        kernel.UseWpfUIMessageBox();
        
        // Проверка запуска плагина
        Check(kernel);

        // Вызывает стандартное уведомление
        Notification(kernel.Get<CopyViewWindow>());
    }
    
    private static void Check(IKernel kernel) {
        var revitRepository = kernel.Get<RevitRepository>();
        var messageBoxService = kernel.Get<IMessageBoxService>();
        var localizationService = kernel.Get<ILocalizationService>();
        
        var copyViews = revitRepository.GetUserViews(revitRepository.GetSelectedCopyViews()).ToArray();
        if(copyViews.Length == 0) {
            string title = localizationService.GetLocalizedString("CopyView.NotSelectedCopyViewTitle");
            string message = localizationService.GetLocalizedString("CopyView.NotSelectedCopyViewMessage");

            messageBoxService.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            throw new System.OperationCanceledException();
        }
    }
}
