using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitUnmodelingMep.Models.Entities;
using RevitUnmodelingMep.Models.Unmodeling;


#nullable enable
namespace RevitUnmodelingMep.Models;

internal class UnmodelingCreator {
    private const string _familyName = "_Якорный элемент";
    private const string _emptyDescription = "Пустая строка";
    private const string _conumableDescription = "Расчет расходников";
    private const string _obsoletteConsumableDescriptionName = "ФОП_ВИС_Назначение";
    private readonly List<string> _obsoletteConsumableDescriptions = 
        ["Расчет краски и креплений", "Расходники изоляции"];

    private readonly Document _doc;
    private readonly ILocalizationService _localizationService;
    public IMessageBoxService MessageBoxService { get; }
    private WorksetId? _ws_id;
    private FamilySymbol? _famylySymbol;
    private readonly string _libDir;
    private bool _numberParamInitialized;
    private SharedParam? _numberParam;
    
    private const double _coordinateStep = 0.001;
    private double _maxLocationY = 0;

    public UnmodelingCreator(
        Document doc,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService) {
        _doc = doc;
        _localizationService = localizationService;
        MessageBoxService = messageBoxService;
        _libDir = GetLibFolder();
    }

    public FamilySymbol Symbol {
        get {
            if(_famylySymbol is null) {
                throw new InvalidOperationException(
                    "_famylySymbol is null");
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
                        SharedParamsConfig.Instance.BuildingWorksLevelCurrency,
                        SharedParamsConfig.Instance.VISSettings
            ];

        ProjectParameters projectParameters = ProjectParameters.Create(_doc.Application);
        projectParameters.SetupRevitParams(_doc, revitParams);

        CheckWorksets();

        _famylySymbol = GetFamilySymbol();

        if(CheckSymbol(_famylySymbol) == false) {
            string title = _localizationService.GetLocalizedString("MainWindow.Title");
            string message = _localizationService.GetLocalizedString("UnmodelingCreator.FamilyOutdatedError");
            UserMessageException.Throw(MessageBoxService, title, message);
        }
    }

    private IList<string> GetFamilySharedParameterNames(Family family) {
        // Открываем документ семейства для редактирования
        Document familyDoc = _doc.EditFamily(family);

        try {
            FamilyManager familyManager = familyDoc.FamilyManager;

            IList<string> sharedParameters = new List<string>();

            foreach(FamilyParameter param in familyManager.Parameters) {
                if(param.IsShared) {
                    sharedParameters.Add(param.Definition.Name);
                }
            }

            return sharedParameters;
        } finally {
            // Закрываем документ семейства без сохранения изменений
            familyDoc.Close(false);
        }
    }

    private bool CheckSymbol(FamilySymbol famylySymbol) {
        string paraName = SharedParamsConfig.Instance.Description.Name;
        Family family = famylySymbol.Family;

        IList<string> paraNames = GetFamilySharedParameterNames(family);

        return paraNames.Contains(paraName);
    }

    private string GetLibFolder() {
        return VisSettingsStorage.GetLibFolder();
    }

    private void CheckWorksets() {
        string targetName = "99_Немоделируемые элементы";
        string warningText = _localizationService.GetLocalizedString("UnmodelingCreator.VisibilityWarning");
        string warningCaption = _localizationService.GetLocalizedString("UnmodelingCreator.VisibilityCaption");
        string transctionName = _localizationService.GetLocalizedString("UnmodelingCreator.WSSetup");

        if(WorksetTable.IsWorksetNameUnique(_doc, targetName)) {
            using(var t = _doc.StartTransaction(transctionName)) {

                Workset newWS = Workset.Create(_doc, targetName);
                _ws_id = newWS.Id;

                MessageBoxService.Show(warningText, warningCaption, MessageBoxButton.OK, MessageBoxImage.Warning);

                t.Commit();
            }
        } else {
            List<Workset> userWSs = [.. new FilteredWorksetCollector(_doc).OfKind(WorksetKind.UserWorkset)];

            foreach (Workset ws in userWSs) {
                if(ws.Name == targetName) {
                    _ws_id = ws.Id;
                    if(ws.IsVisibleByDefault) {
                        MessageBoxService.Show(warningText, warningCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
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
            throw new InvalidOperationException("_ws_id is null");
        instWorkset.Set(_ws_id.IntegerValue);

        if(!_numberParamInitialized) {
            _numberParam = _doc.IsExistsParam(SharedParamsConfig.Instance.VISSpecNumbersCurrency)
                ? SharedParamsConfig.Instance.VISSpecNumbersCurrency
                : SharedParamsConfig.Instance.VISSpecNumbers;
            _numberParamInitialized = true;
        }
        if(_numberParam is null) {
            throw new InvalidOperationException("_numberParam is null");
        }

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
                        "Unsupported value type");
            }
        }
        string finalDescription = $"{_conumableDescription}_{newRowElement.Element.Id.ToString()}";

        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISSystemName, newRowElement.System);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISGrouping, newRowElement.Group);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISCombinedName, newRowElement.Name);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISMarkNumber, newRowElement.Mark);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISItemCode, newRowElement.Code);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISManufacturer, newRowElement.Maker);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISUnit, newRowElement.Unit);
        SetUnmodelingValue(familyInstance, _numberParam, newRowElement.Number);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISMass, newRowElement.Mass);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.VISNote, newRowElement.Note);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.EconomicFunction, newRowElement.Function);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.BuildingWorksBlock, newRowElement.SmrBlock);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.BuildingWorksSection, newRowElement.SmrSection);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.BuildingWorksLevel, newRowElement.SmrFloor);
        SetUnmodelingValue(familyInstance,
            SharedParamsConfig.Instance.BuildingWorksLevelCurrency,
            newRowElement.SmrFloorCurrency);
        SetUnmodelingValue(familyInstance, SharedParamsConfig.Instance.Description, finalDescription);
    }


    private FamilySymbol GetFamilySymbol() {
        FamilySymbol? symbol = FindFamilySymbol(_familyName);

        if(symbol != null) {
            return symbol;
        }

        string familyPath = Path.Combine(_libDir, _familyName + ".rfa");
        string transactionName = _localizationService.GetLocalizedString(
            "UnmodelingCreator.LoadFamilyTransactionName");

        using(var t = _doc.StartTransaction(transactionName)) {
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
        string title = _localizationService.GetLocalizedString("MainWindow.Title");
        string message = _localizationService.GetLocalizedString("UnmodelingCreator.LoadFamilyError");
        UserMessageException.Throw(MessageBoxService, title, message);
        throw new UserMessageException("Unreachable");
    }

    private FamilySymbol? FindFamilySymbol(string familyName) {
        return new FilteredElementCollector(_doc)
        .OfCategory(BuiltInCategory.OST_GenericModel)
        .OfClass(typeof(FamilySymbol))
        .Cast<FamilySymbol>()
        .FirstOrDefault(s => s.Family.Name.Equals(familyName, StringComparison.Ordinal));
    }

    public void RemoveUnmodeling() {
        EditorChecker editorChecker = new EditorChecker(_doc, _localizationService);
        List<FamilyInstance> genericCollection = GetGenericCollection();
        string transactionName = _localizationService.GetLocalizedString("MainWindow.Title");

        foreach(FamilyInstance familyInstance in genericCollection) {
            editorChecker.GetReport(familyInstance);

            if(!string.IsNullOrEmpty(editorChecker.FinalReport)) {
                UserMessageException.Throw(MessageBoxService, "Настройки немоделируемых",
                    editorChecker.FinalReport);
            }
        }

        
        foreach(FamilyInstance familyInstance in genericCollection) {
            string currentDescription =
                familyInstance.GetParamValueOrDefault<string>(SharedParamsConfig.Instance.Description);

            currentDescription = !string.IsNullOrEmpty(currentDescription)
                ? currentDescription
                : familyInstance.GetParamValueOrDefault<string>(_obsoletteConsumableDescriptionName);
            

            if(string.IsNullOrEmpty(currentDescription) || currentDescription.Contains(_conumableDescription) 
                || _obsoletteConsumableDescriptions.Contains(currentDescription)) {
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


