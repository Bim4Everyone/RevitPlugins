using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitUnmodelingMep.Models.Entities;


#nullable enable
namespace RevitUnmodelingMep.Models;

internal class UnmodelingCreator {
    private const string _familyName = "_Якорный элемент";
    private const string _emptyDescription = "Пустая строка";
    private const string _conumableDescription = "Временная заглушка";
    private const string _obsoletteConsumableDescriptionName = "ФОП_ВИС_Назначение"; 

    private readonly Document _doc;
    private WorksetId? _ws_id;
    private FamilySymbol? _famylySymbol;
    private readonly string _libDir;
    
    private const double _coordinateStep = 0.001;
    private double _maxLocationY = 0;

    public UnmodelingCreator(Document doc) {
        _doc = doc;
        _libDir = GetLibFolder();
    }

    public FamilySymbol Symbol {
        get {
            if(_famylySymbol is null) {
                throw new InvalidOperationException(
                    "Символ семейства якорного элемента не инициализирован. Вызовите StartupChecks() перед расчетом.");
            }

            return _famylySymbol;
        }
    }


    public void StartupChecks() {

        List<RevitParam> revitParams = [
            SharedParamsConfig.Instance.EconomicFunction,
                        SharedParamsConfig.Instance.VISSystemName,
                        SharedParamsConfig.Instance.BuildingWorksBlock,
                        SharedParamsConfig.Instance.BuildingWorksSection,
                        SharedParamsConfig.Instance.BuildingWorksLevel,
                        SharedParamsConfig.Instance.VISSettings,
                        SharedParamsConfig.Instance.Description
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

    private XYZ GetNextLocation() {
        if(_maxLocationY == 0) {
            var genericModels = GetGenericCollection();

            if(genericModels.Count == 0) {
                return new XYZ(0, 0, 0);
            }

            foreach(Element element in genericModels) {
                if(element.Location is not LocationPoint locationPoint) {
                    continue;
                }
                XYZ point = locationPoint.Point;
                double yValue = point.Y;
                if (yValue > _maxLocationY) {
                    _maxLocationY = yValue;
                }
            }
        }
        _maxLocationY = _maxLocationY + _coordinateStep;
        return new XYZ(0, _maxLocationY, 0);
    }

    public void CreateNewPosition(NewRowElement newRowElement) {

        newRowElement.Number = Math.Round(newRowElement.Number, 2, MidpointRounding.AwayFromZero);

        if(newRowElement.Number == 0) {
            return;
        }

        XYZ location = GetNextLocation();

        FamilyInstance familyInstance = _doc.Create.NewFamilyInstance(
            location,
            _famylySymbol,
            StructuralType.NonStructural);

        Parameter instWorkset = familyInstance.GetParam(BuiltInParameter.ELEM_PARTITION_PARAM);

        if(_ws_id is null)
            throw new InvalidOperationException("Рабочий набор немоделируемых не инициализирован");
        instWorkset.Set(_ws_id.IntegerValue);

        string group = $"{newRowElement.Group}" +
            $"_{newRowElement.Name}" +
            $"_{newRowElement.Mark}" +
            $"_{newRowElement.Maker}" +
            $"_{newRowElement.Code}";

        SharedParam numberParam;
        if(_doc.IsExistsParam(SharedParamsConfig.Instance.VISSpecNumbersCurrency)) {
            numberParam = SharedParamsConfig.Instance.VISSpecNumbersCurrency;
        } else {
            numberParam = SharedParamsConfig.Instance.VISSpecNumbers;
        }

        Parameter descriptionParam = familyInstance.GetParam(SharedParamsConfig.Instance.Description);

        void SetUnmodelingValue(FamilyInstance familyInstance, SharedParam sharedParam, object? value) {
            if(value is null)
                return;

            switch(value) {
                case string s:
                    familyInstance.SetParamValue(sharedParam, s);
                    break;

                case double d:
                    familyInstance.SetParamValue(sharedParam, d);
                    break;

                default:
                    throw new NotSupportedException(
                        $"Тип {value.GetType().Name} не поддерживается");
            }
        }

        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISSystemName, newRowElement.System);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISGrouping, group);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISCombinedName, newRowElement.Name);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISMarkNumber, newRowElement.Mark);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISItemCode, newRowElement.Code);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISManufacturer, newRowElement.Maker);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISUnit, newRowElement.Unit);
        SetUnmodelingValue(familyInstance, numberParam, newRowElement.Number);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISMass, newRowElement.Mass);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISNote, newRowElement.Note);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.EconomicFunction, newRowElement.Function);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.BuildingWorksBlock, newRowElement.SmrBlock);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.BuildingWorksSection, newRowElement.SmrSection);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.BuildingWorksLevel, newRowElement.SmrFloor);
        SetUnmodelingValue(familyInstance,
            SharedParamsConfig.Instance.BuildingWorksLevelCurrency,
            newRowElement.SmrFloorDE);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.Description, _conumableDescription);
    }


    private FamilySymbol GetFamilySymbol() {
        FamilySymbol? symbol = FindFamilySymbol(_familyName);

        if(symbol != null) {
            return symbol;
        }

        string familyPath = Path.Combine(_libDir, _familyName + ".rfa");

        using(var t = _doc.StartTransaction("Загрузка семейства")) {
            if(_doc.LoadFamily(familyPath, out Family loadedFamily) && loadedFamily != null) {
                t.Commit();

                ElementId symbolId = loadedFamily.GetFamilySymbolIds().FirstOrDefault();
                if(symbolId != ElementId.InvalidElementId && symbolId != null) {
                    return (FamilySymbol) _doc.GetElement(symbolId);
                }

                symbol = FindFamilySymbol(_familyName);
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

    public void RemoveUnmodeling() {
        EditorChecker editorChecker = new EditorChecker(_doc);
        List<FamilyInstance> genericCollection = GetGenericCollection();

        foreach(FamilyInstance familyInstance in genericCollection) {
            editorChecker.GetReport(familyInstance);

            if(!string.IsNullOrEmpty(editorChecker.FinalReport)) {
                MessageBox.Show(editorChecker.FinalReport, "Настройки немоделируемых",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw new OperationCanceledException();
            }
        }


        foreach(FamilyInstance familyInstance in genericCollection) {
            string currentDescription =
                familyInstance.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.Description);

            currentDescription = !string.IsNullOrEmpty(currentDescription)
                ? currentDescription
                : familyInstance.GetParamValueOrDefault<string>(_obsoletteConsumableDescriptionName);

            if(currentDescription == _conumableDescription) {
                _doc.Delete(familyInstance.Id);
            }
        }


    }

    private List<FamilyInstance> GetGenericCollection() {
        var genericModels = new FilteredElementCollector(_doc)
            .OfCategory(BuiltInCategory.OST_GenericModel)
            .WhereElementIsNotElementType()
            .OfClass(typeof(FamilyInstance))
            .Cast<FamilyInstance>()
            .Where(fi => fi.Symbol != null &&
                         fi.Symbol.Family != null &&
                         fi.Symbol.Family.Name.Equals(_familyName, StringComparison.Ordinal))
            .ToList();

        return genericModels;
    }
}
