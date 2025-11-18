using System;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using DevExpress.Xpf.CodeView.Margins;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitRooms.Models;
using RevitRooms.Services;
using RevitRooms.ViewModels;
using RevitRooms.Views;

namespace RevitRooms;
[Transaction(TransactionMode.Manual)]
public class RoomsCommand : BasePluginCommand {
    public RoomsCommand() {
        PluginName = "Квартирография Стадии П";
    }

    protected override void Execute(UIApplication uiApplication) {
        using IKernel kernel = uiApplication.CreatePlatformServices();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<ErrorWindowService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<CheckProjectParams>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<RoomsConfig>()
            .ToMethod(c => RoomsConfig.GetRoomsConfig(c.Kernel.Get<IConfigSerializer>()));

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        // Настройка запуска окна
        kernel.BindMainWindow<RoomsViewModel, RoomsWindow>();
        kernel.BindOtherWindow<WarningsViewModel, WarningsWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        // Вызывает стандартное уведомление
        Notification(kernel.Get<RoomsWindow>());
    }
}
