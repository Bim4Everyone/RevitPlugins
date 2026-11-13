using System;
using System.Globalization;
using System.Reflection;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using Bim4Everyone.RevitFiltration.Ninject;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;

using Microsoft.WindowsAPICodePack.Dialogs;

using Ninject;

using RevitMarkAllDocuments.Models;
using RevitMarkAllDocuments.Services;
using RevitMarkAllDocuments.Services.Export;
using RevitMarkAllDocuments.ViewModels;
using RevitMarkAllDocuments.Views;

namespace RevitMarkAllDocuments;

/// <summary>
/// Класс команды Revit плагина.
/// </summary>
/// <remarks>
/// В данном классе должна быть инициализация контейнера плагина и указание названия команды.
/// </remarks>
[Transaction(TransactionMode.Manual)]
public class RevitMarkAllDocsByConfigCommand : BasePluginCommand {
    public RevitMarkAllDocsByConfigCommand() {
        PluginName = "Маркировать всё на основе конфига";
    }

    protected override void Execute(UIApplication uiApplication) {
        // Создание контейнера зависимостей плагина с сервисами из платформы
        using IKernel kernel = uiApplication.CreatePlatformServices();

        kernel.UseLogicalFilterFactory();
        kernel.UseLogicalFilterProviderFactory();
        kernel.UseFilterContextParser();

        // Настройка доступа к Revit
        kernel.Bind<RevitRepository>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<JsonSerializerService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<DocumentService>()
            .ToSelf()
            .InSingletonScope();

        kernel.Bind<WindowsService>()
            .ToSelf()
            .InSingletonScope();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();
        kernel.UseWpfWindowsTheme();
        kernel.Bind<IHasTheme>().To<HasTheme>().InSingletonScope();
        kernel.Bind<IHasLocalization>().To<HasLocalization>().InSingletonScope();
        kernel.UseWpfOpenFileDialog(filter: "JSON files (*.json)|*.json");
        kernel.UseWpfUIMessageBox();

        kernel.Bind<MarkListWindow>().ToSelf();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));

        string filePath = SelectFile(kernel);
        var windowService = kernel.Get<WindowsService>();
        var documentService = kernel.Get<DocumentService>();
        var revitRepository = kernel.Get<RevitRepository>();
        var jsonService = kernel.Get<JsonSerializerService>();

        var markData = jsonService.ImportMarkData(filePath);
        string currentDocName = documentService.GetDocumentFullName(revitRepository.Document);
        var markDataForCurrentDoc = markData.GetDataByDocument(currentDocName);

        Notification(windowService.ShowMarkListWindow(markDataForCurrentDoc, markData.MarkRevitParam));
    }

    private string SelectFile(IKernel kernel) {
        var dialog = kernel.Get<IOpenFileDialogService>();

        if(!dialog.ShowDialog()) {
            throw new OperationCanceledException();
        }

        return dialog.File.FullName;
    }
}
