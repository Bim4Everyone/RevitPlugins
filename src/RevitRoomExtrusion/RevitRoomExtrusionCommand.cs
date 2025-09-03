using System;
using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.Xpf.Core.Ninject;

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

        kernel.Bind<PluginConfig>()
            .ToMethod(c => PluginConfig.GetPluginConfig());

        kernel.Bind<MainViewModel>().ToSelf();
        kernel.Bind<MainWindow>().ToSelf()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<MainViewModel>())
            .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                c => c.Kernel.Get<ILocalizationService>());

        kernel.Bind<ErrorViewModel>().ToSelf();
        kernel.Bind<ErrorWindow>().ToSelf()
            .WithPropertyValue(nameof(Window.DataContext),
                c => c.Kernel.Get<ErrorViewModel>())
            .WithPropertyValue(nameof(PlatformWindow.LocalizationService),
                c => c.Kernel.Get<ILocalizationService>());

        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        kernel.UseXtraLocalization(
            $"/{assemblyName};component/Localization/Language.xaml",
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
