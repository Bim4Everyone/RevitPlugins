using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;
using dosymep.SimpleServices;

#nullable enable
namespace RevitUnmodelingMep.Models;

internal class UnmodelingProcessor {
    private readonly Document _doc;
    private WorksetId? _ws_id;
    private FamilySymbol? _famylySymbol;
    private readonly string _libDir;
    public UnmodelingProcessor(Document doc) {
        _doc = doc;
        _libDir = GetLibFolder();
    }

    public void StartupChecks() {

        List<RevitParam> revitParams = [
            SharedParamsConfig.Instance.EconomicFunction,
                        SharedParamsConfig.Instance.VISSystemName,
                        SharedParamsConfig.Instance.BuildingWorksBlock,
                        SharedParamsConfig.Instance.BuildingWorksSection,
                        SharedParamsConfig.Instance.BuildingWorksLevel,
                        SharedParamsConfig.Instance.VISSettings
            ];

        ProjectParameters projectParameters = ProjectParameters.Create(_doc.Application);
        projectParameters.SetupRevitParams(_doc, revitParams);

        CheckWorksets();

        _famylySymbol = GetFamilySymbol();
    }

    private string GetLibFolder() {
        var assembly = Assembly.GetExecutingAssembly();
        string assemblyPath = string.Empty;

        if(!string.IsNullOrWhiteSpace(assembly.CodeBase)) {
            assemblyPath = new Uri(assembly.CodeBase).LocalPath;
        }

        if(string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(assembly.Location)) {
            assemblyPath = assembly.Location;
        }

        if(string.IsNullOrWhiteSpace(assemblyPath)) {
            assemblyPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        string dllDir = Path.GetDirectoryName(assemblyPath)
                ?? AppDomain.CurrentDomain.BaseDirectory;

        // Основной путь: профиль pyRevit -> Extensions -> 04.OV-VK.extension -> lib
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string defaultLibPath = Path.Combine(appData, "pyRevit", "Extensions", "04.OV-VK.extension", "lib");

        // Fallback: если dll действительно лежит в ...\ОВиВК.tab\bin, поднимаемся на две директории вверх и заходим в lib
        string fallbackLibPath = Path.GetFullPath(Path.Combine(dllDir, "..", "..", "lib"));

        string libDir = Directory.Exists(defaultLibPath) ? defaultLibPath : fallbackLibPath;
        //string defaultsPath = Path.Combine(libDir, "default_spec_settings.json");
        return libDir;
    }

    private void CheckWorksets() {
        string targetName = "99_Немоделируемые элементы";
        string warningText = "Рабочий набор \"99_Немоделируемые элементы\" на данный момент отображается на всех видах.\n\n" +
                "Откройте диспетчер рабочих наборов и снимите галочку с параметра \"Видимый на всех видах\".\n\n" +
                "В данном рабочем наборе будут создаваться немоделируемые элементы и требуется исключить их видимость.";
        string warningCaption = "Рабочие наборы";

        if(WorksetTable.IsWorksetNameUnique(_doc, targetName)) {
            using(var t = _doc.StartTransaction("Настройка рабочих наборов")) {

                Workset newWS = Workset.Create(_doc, targetName);
                _ws_id = newWS.Id;

                MessageBox.Show(warningText, warningCaption, MessageBoxButton.OK, MessageBoxImage.Warning);

                t.Commit();
            }
        } else {
            List<Workset> userWSs = [.. new FilteredWorksetCollector(_doc).OfKind(WorksetKind.UserWorkset)];

            foreach (Workset ws in userWSs) {
                if(ws.Name == targetName) {
                    _ws_id = ws.Id;
                    if(ws.IsVisibleByDefault) {
                        MessageBox.Show(warningText, warningCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
        }
    }

    private FamilySymbol GetFamilySymbol() {
        string familyName = "_Якорный элемент";

        FamilySymbol? symbol = FindFamilySymbol(familyName);

        if(symbol != null) {
            return symbol;
        }

        string familyPath = Path.Combine(_libDir, familyName + ".rfa");

        using(var t = _doc.StartTransaction("Загрузка семейства")) {
            if(_doc.LoadFamily(familyPath, out Family loadedFamily) && loadedFamily != null) {
                t.Commit();

                ElementId symbolId = loadedFamily.GetFamilySymbolIds().FirstOrDefault();
                if(symbolId != ElementId.InvalidElementId && symbolId != null) {
                    return (FamilySymbol) _doc.GetElement(symbolId);
                }

                symbol = FindFamilySymbol(familyName);
                if(symbol != null) {
                    return symbol;
                }
            } else {
                t.Commit();
            }
        }
        MessageBox.Show("Не удалось загрузить семейство якорного элемента");
        throw new OperationCanceledException();
    }

    private FamilySymbol? FindFamilySymbol(string familyName) {
        return new FilteredElementCollector(_doc)
        .OfCategory(BuiltInCategory.OST_GenericModel)
        .OfClass(typeof(FamilySymbol))
        .Cast<FamilySymbol>()
        .FirstOrDefault(s => s.Family.Name.Equals(familyName, StringComparison.Ordinal));
    }
}
