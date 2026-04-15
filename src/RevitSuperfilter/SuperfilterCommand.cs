using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;
using Ninject.Parameters;

using RevitSuperfilter.Models;
using RevitSuperfilter.Models.Selections;
using RevitSuperfilter.Services;
using RevitSuperfilter.ViewModels;
using RevitSuperfilter.Views;

namespace RevitSuperfilter;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class SuperfilterCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public SuperfilterCommand() {
        PluginName = "Пример команды";
    }

    /// <summary>
    /// Метод выполнения основного кода плагина.
    /// </summary>
    /// <param name="uiApplication">Интерфейс взаимодействия с Revit.</param>
    /// <remarks>
    /// В случаях, когда не используется конфигурация
    /// или локализация требуется удалять их использование полностью во всем проекте.
    /// </remarks>
    protected override void Execute(UIApplication uiApplication) {
        // Создание контейнера зависимостей плагина с сервисами из платформы
        IKernel kernel = uiApplication.CreatePlatformServices();

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
        kernel.BindMainWindow<MainViewModel, MainWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // kernel.Bind<ISelectionElements>()
        //     .To<DBSelection>()
        //     .InSingletonScope();

        kernel.Bind<ISelectionElements>()
            .To<DBViewSelection>()
            .InSingletonScope();

        // kernel.Bind<ISelectionElements>()
        //     .To<SelectedOnViewSelection>()
        //     .InSingletonScope();

        kernel.Bind<IReadOnlyCollection<ISuperfilterService>>()
            .ToMethod(c => c.Kernel.GetAll<ISelectionElements>()
                .Select(item => new SuperfilterService(
                    c.Kernel.Get<UIApplication>(),
                    item,
                    c.Kernel.Get<ILocalizationService>()))
                .ToArray())
            .InSingletonScope();

        kernel.Get<MainWindow>().Show();

        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
    }

    private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
        string sss = e.ExceptionObject.ToString();
    }
}
