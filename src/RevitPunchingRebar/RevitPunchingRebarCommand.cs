using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Annotations;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Ninject;

using RevitPunchingRebar.Models;
using RevitPunchingRebar.Models.Interfaces;
using RevitPunchingRebar.ViewModels;
using RevitPunchingRebar.Views;

namespace RevitPunchingRebar;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitPunchingRebarCommand : BasePluginCommand {
    /// <summary>
    /// Инициализирует команду плагина.
    /// </summary>
    public RevitPunchingRebarCommand() {
        PluginName = "Поперечное армирование";
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
        try {
            // Создание контейнера зависимостей плагина с сервисами из платформы
            using IKernel kernel = uiApplication.CreatePlatformServices();
            //IKernel kernel = uiApplication.CreatePlatformServices();

            // Настройка доступа к Revit
            kernel.Bind<RevitRepository>()
                .ToSelf()
                .InSingletonScope();

            // Настройка конфигурации плагина
            kernel.Bind<PluginConfig>()
                .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

            // Используем сервис обновления тем для WinUI
            kernel.UseWpfUIThemeUpdater();


            kernel.Bind<IPunchingRebarPlacementService>()
                .To<PunchingRebarPlacementService>()
                .InSingletonScope();

            kernel.Bind<IFrameParams>()
                .To<FrameParams>()
                .InSingletonScope();

            kernel.Bind<IPylon>()
                .To<StructColumnPylon>()
                .InSingletonScope();

            // Настройка запуска окна
            kernel.BindMainWindow<MainViewModel, MainWindow>();
            var vm = kernel.Get<MainViewModel>();

            vm.SelectHandler = new SelectHandler(vm);
            vm.SelectExternalEvent = ExternalEvent.Create(vm.SelectHandler);

            vm.PlacementHandler = new PlacementHandler(kernel.Get<RevitRepository>());
            vm.PlacementExternalEvent = ExternalEvent.Create(vm.PlacementHandler);

            // Настройка локализации,
            // получение имени сборки откуда брать текст
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            // Настройка локализации,
            // установка дефолтной локализации "ru-RU"
            kernel.UseWpfLocalization(
                $"/{assemblyName};component/assets/localization/language.xaml",
                CultureInfo.GetCultureInfo("ru-RU"));

            var revitRepository = kernel.Get<RevitRepository>();

            //Notification(kernel.Get<MainWindow>());
            kernel.Get<MainWindow>().Show();


        } catch(Exception ex) { MessageBox.Show(ex.ToString()); }
        
       
        


    }
}
