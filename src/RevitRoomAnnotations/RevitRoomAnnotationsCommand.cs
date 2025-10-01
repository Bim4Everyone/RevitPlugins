using System;
using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitRoomAnnotations.Models;
using RevitRoomAnnotations.Services;
using RevitRoomAnnotations.ViewModels;
using RevitRoomAnnotations.Views;


namespace RevitRoomAnnotations;

[Transaction(TransactionMode.Manual)]
public class RevitRoomAnnotationsCommand : BasePluginCommand {

    public RevitRoomAnnotationsCommand() {
        PluginName = "Обновить помещения в ЭОМ";
    }

    protected override void Execute(UIApplication uiApplication) {
        // Создание контейнера зависимостей плагина с сервисами из платформы
        using var kernel = uiApplication.CreatePlatformServices();

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

        // Настройка сервиса окошек сообщений
        kernel.UseWpfUIMessageBox<MainViewModel>();

        kernel.Bind<IRoomAnnotationMapService>()
            .To<RoomAnnotationMapService>()
            .InSingletonScope();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        var localizationService = kernel.Get<ILocalizationService>();
        var messageBoxService = kernel.Get<IMessageBoxService>();
        var revitRepository = kernel.Get<RevitRepository>();

        // Загрузка параметров проекта        
        bool isParamChecked = new CheckProjectParams(uiApplication.Application, uiApplication.ActiveUIDocument.Document)
            .CopyProjectParams()
            .GetIsChecked();

        if(!isParamChecked) {
            messageBoxService.Show(
                localizationService.GetLocalizedString("Command.ParamErrorMessageBody"),
                localizationService.GetLocalizedString("Command.TitleErrorMessage"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            throw new OperationCanceledException();
        }

        // Проверка загрузки основного семейства
        if(revitRepository.GetRoomAnnotationSymbol() == null) {
            messageBoxService.Show(
                localizationService.GetLocalizedString("Command.ErrorAnnotationTypeNotFound", RevitRepository.RoomAnnotationName),
                localizationService.GetLocalizedString("Command.TitleErrorMessage"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            throw new OperationCanceledException();
        }

        // Вызывает стандартное уведомление
        Notification(kernel.Get<MainWindow>());
    }
}
