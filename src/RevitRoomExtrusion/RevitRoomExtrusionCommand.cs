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

using RevitRoomExtrusion.Models;
using RevitRoomExtrusion.ViewModels;
using RevitRoomExtrusion.Views;

namespace RevitRoomExtrusion;
[Transaction(TransactionMode.Manual)]
public class RevitRoomExtrusionCommand : BasePluginCommand {
    public RevitRoomExtrusionCommand() {
        PluginName = "Выдавить помещения";
    }

    protected override void Execute(UIApplication uiApplication) {
        using var kernel = uiApplication.CreatePlatformServices();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<RoomChecker>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<FamilyLoader>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<FamilyCreator>()
            .ToSelf()
            .InSingletonScope();
        kernel.Bind<FamilyLoadOptions>()
            .ToSelf()
            .InSingletonScope();

        // Настройка конфигурации плагина
        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig(c.Kernel.Get<IConfigSerializer>()));

        // Настройка запуска окон
        kernel.BindMainWindow<MainViewModel, MainWindow>();
        kernel.BindMainWindow<ErrorViewModel, ErrorWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var roomChecker = kernel.Get<RoomChecker>();

        var messageBoxService = kernel.Get<IMessageBoxService>();
        var localizationService = kernel.Get<ILocalizationService>();

        if(roomChecker.CheckSelection()) {
            if(roomChecker.CheckRooms()) {
                Notification(kernel.Get<MainWindow>());
            } else {
                var errorWindow = kernel.Get<ErrorWindow>();
                errorWindow.Show();
                throw new OperationCanceledException();
            }
        } else {
            string stringMessegeBody = localizationService.GetLocalizedString("Command.MessegeBody");
            string stringMessegeTitle = localizationService.GetLocalizedString("Command.MessegeTitle");
            messageBoxService.Show(
                stringMessegeBody, stringMessegeTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            throw new OperationCanceledException();
        }
    }
}
