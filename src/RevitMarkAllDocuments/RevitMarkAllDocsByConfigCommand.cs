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

using Ninject;

using RevitMarkAllDocuments.Models;
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
    }
}
