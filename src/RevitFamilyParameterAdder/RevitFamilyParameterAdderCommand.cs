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
        Check(kernel);

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization($"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        kernel.Bind<MainViewModel>().ToSelf();
        kernel.Bind<MainWindow>().ToSelf()
            .WithPropertyValue(nameof(Window.Title), PluginName)
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<MainViewModel>());

        Notification(kernel.Get<MainWindow>());
    }

    private void Check(IKernel kernel) {
        var revitRepositiry = kernel.Get<RevitRepository>();
        if(!revitRepositiry.IsFamilyFile()) {
            TaskDialog.Show(PluginName, $"Данный скрипт работает только в файле семейства.");
            throw new OperationCanceledException();
        }
        if(!revitRepositiry.IsSharedParametersFileConnected()) {
            TaskDialog.Show(PluginName, $"Файл общих параметров не найден. Проверьте, что ФОП подключен к проекту.");
            throw new OperationCanceledException();
        }
    }
}
