using System.Globalization;
using System.Reflection;
using System.Windows;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using Bim4Everyone.RevitFiltration.Ninject;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;
using dosymep.WPF.Views;
using dosymep.WpfCore.Ninject;
using dosymep.WpfUI.Core.Ninject;
using dosymep.Xpf.Core.Ninject;

using Microsoft.WindowsAPICodePack.Dialogs;

using Ninject;

using RevitMarkAllDocuments.Models;
using RevitMarkAllDocuments.Models.Export;
using RevitMarkAllDocuments.Services;
using RevitMarkAllDocuments.ViewModels;
using RevitMarkAllDocuments.Views;

using Wpf.Ui.Abstractions;

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

        kernel.Bind<DocumentService>()
            .ToSelf()
            .InSingletonScope();

        // Используем сервис обновления тем для WinUI
        kernel.UseWpfUIThemeUpdater();

        kernel.BindMainWindow<MarkListViewModel, MarkListWindow>();

        // Настройка локализации,
        // получение имени сборки откуда брать текст
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

        // Настройка локализации,
        // установка дефолтной локализации "ru-RU"
        kernel.UseWpfLocalization(
            $"/{assemblyName};component/assets/localization/language.xaml",
            CultureInfo.GetCultureInfo("ru-RU"));


        string filePath = SelectFile();
        if(!string.IsNullOrEmpty(filePath)) {
            var jsonService = new JsonSerializerService();

            var markData = jsonService.ImportMarkData(filePath);
            kernel.Bind<MarkData>()
                .ToConstant(markData);

            Notification(kernel.Get<MarkListWindow>());
        } else {
            Notification(false);
        }
    }

    public string SelectFile() {
        var dialog = new CommonOpenFileDialog();

        if(dialog.ShowDialog() == CommonFileDialogResult.Ok) {
            return dialog.FileName;
        }

        return string.Empty;
    }
}
