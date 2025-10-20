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
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitFamilyParameterAdder.Models;
using RevitFamilyParameterAdder.ViewModels;
using RevitFamilyParameterAdder.Views;

namespace RevitFamilyParameterAdder;
[Transaction(TransactionMode.Manual)]
public class RevitFamilyParameterAdderCommand : BasePluginCommand {
    public RevitFamilyParameterAdderCommand() {
        PluginName = "Добавление параметров в семейство";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

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
            $"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // используем message box без привязки к окну,
        // потому что он вызывается до запуска основного окна
        kernel.UseWpfUIMessageBox();

        // Проверка возможности запуска плагина
        Check(kernel);

        Notification(kernel.Get<MainWindow>());
    }

    private static void Check(IKernel kernel) {
        var revitRepository = kernel.Get<RevitRepository>();
        var messageBoxService = kernel.Get<IMessageBoxService>();
        var localizationService = kernel.Get<ILocalizationService>();
        string title = localizationService.GetLocalizedString("MessageBox.Title");

        if(!revitRepository.IsFamilyFile()) {
            string message = localizationService.GetLocalizedString("MessageBox.IsNotFamilyFile");
            messageBoxService.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            throw new OperationCanceledException();
        }
        if(!revitRepository.IsSharedParametersFileConnected()) {
            string message = localizationService.GetLocalizedString("MessageBox.IsNotSharedParametersFileConnected");
            messageBoxService.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            throw new OperationCanceledException();
        }
    }
}
