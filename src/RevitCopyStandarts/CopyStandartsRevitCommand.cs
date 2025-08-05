#region Namespaces

using System.IO;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitCopyStandarts.ViewModels;

#endregion

namespace RevitCopyStandarts;

[Transaction(TransactionMode.Manual)]
public class CopyStandartsRevitCommand : BasePluginCommand {
    public CopyStandartsRevitCommand() {
        PluginName = "Копирование стандартов";
    }

    protected override void Execute(UIApplication uiApplication) {
        var uiDocument = uiApplication.ActiveUIDocument;
        var application = uiApplication.Application;
        var document = uiDocument.Document;

        string mainFolder =
            @"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101";

        mainFolder =
            Path.Combine(mainFolder, ModuleEnvironment.RevitVersion, nameof(RevitCopyStandarts));

        var mainWindow = new MainWindow { DataContext = new BimCategoriesViewModel(mainFolder, document, application) };

        mainWindow.ShowDialog();
    }
}
