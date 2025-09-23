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
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Ninject;

using RevitAxonometryViews.Models;
using RevitAxonometryViews.ViewModels;
using RevitAxonometryViews.Views;

namespace RevitAxonometryViews;
[Transaction(TransactionMode.Manual)]
public class RevitAxonometryViewsCommand : BasePluginCommand {
    public RevitAxonometryViewsCommand() {
        PluginName = "Создать схемы";
    }
    protected override void Execute(UIApplication uiApplication) {
        // Здесь мы биндим классы в словарь Kernel, который сам будет их инициализировать через Get<Имя из словаря>, без вызова конструкторов
        // Которые он обрабатывает самостоятельно
        // Например Kernel.Get<MainViewModel>() требует на вход RevitRepository. Kernel самостоятельно ищет его по биндингам и подает в конструктор
        using IKernel kernel = uiApplication.CreatePlatformServices();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        // Настройка сервиса окошек сообщений
        kernel.UseWpfUIMessageBox<MainViewModel>();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<MainViewModel, MainWindow>();
        //kernel.Bind<MainViewModel>().ToSelf()
        //    .InSingletonScope();
        //kernel.Bind<MainWindow>().ToSelf()
        //    .WithPropertyValue(nameof(Window.DataContext),
        //        c => c.Kernel.Get<MainViewModel>())
        //    .WithPropertyValue(nameof(Window.Title), PluginName);


        kernel.Bind<CollectorOperator>().ToSelf()
            .InSingletonScope();
        kernel.Bind<ViewFactory>().ToSelf()
            .InSingletonScope();
        var servise = GetPlatformService<IMessageBoxService>();
        CheckDocument(uiApplication.ActiveUIDocument.Document, servise);

        var revitRepository = kernel.Get<RevitRepository>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/Localization/Language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        Notification(kernel.Get<MainWindow>());
    }

    /// <summary>
    /// Проверяет, не в семействе ли мы и что активен нужный вид
    /// </summary>
    private void CheckDocument(Document document, IMessageBoxService service) {
        string report = string.Empty;

        if(document.ActiveView is not (View3D or
            ViewPlan)) {
            report = "Должен быть активным 2D/3D вид";
        }

        if(document.IsFamilyDocument) {
            report = "Плагин не предназначен для работы с семействами";
        }
        if(!string.IsNullOrEmpty(report)) {
            service.Show(report, "Генерация схем", MessageBoxButton.OK, MessageBoxImage.Error);
            throw new OperationCanceledException();
        }
    }
}

