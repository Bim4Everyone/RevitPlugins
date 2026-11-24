using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models;

internal partial class RevitRepository(UIApplication uiApplication, SystemPluginConfig systemPluginConfig,
    IParamService paramService, ILocalizationService localization) {
    private IEnumerable<RevitFamily> _lintelFamilies;
    private IEnumerable<RevitFamily> _openingFamilies;
    private IEnumerable<ElementId> _phaseIds;
    private IEnumerable<RevitWall> _revitWalls;
    
    
    public UIApplication UIApplication { get; } = uiApplication;
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document { get; } = uiApplication.ActiveUIDocument.Document;

    /// <summary>Проверяет идентичность открытого исходного проекта по экземпляру документа Revit.</summary>
    public bool IsSourceDocumentActive(UIApplication app) {
        return Document.IsValidObject && app.ActiveUIDocument?.Document is { IsValidObject: true } activeDocument
            && Document.Equals(activeDocument);
    }
    
    
    public IEnumerable<RevitFamily> LintelFamilies => _lintelFamilies ??= GetLintelFamilies();
    public IEnumerable<RevitFamily> OpeningFamilies => _openingFamilies ??= GetOpeningFamilies();
    public IEnumerable<RevitWall> RevitWalls => _revitWalls ??= GetRevitWalls().ToArray();
    public IEnumerable<ElementId> PhaseIds => _phaseIds ??= GetPhaseIds();
    
    /// <summary>
    /// Метод получения стадии по имени
    /// </summary>
    public ElementId GetPhaseIdByName(string name) {
        var phaseIds = GetPhaseIds();
        return phaseIds
            .FirstOrDefault(phaseId =>
                phaseId != ElementId.InvalidElementId 
                && Document.GetElement(phaseId)?.Name == name);
    }

    /// <summary>Читает ключи типов согласованных категорий в исходном документе.</summary>
    public IReadOnlyList<StructureTypeSelection> GetStructureTypes() {
        var categories = systemPluginConfig.ReinforcedConcreteCategories.Union(systemPluginConfig.SteelCategories);
        var filter = new ElementMulticategoryFilter(categories.ToList());
        using var collector = new FilteredElementCollector(Document);
        return collector.WherePasses(filter).WhereElementIsElementType().Cast<ElementType>()
            .Where(type => type.Category is not null)
            .Select(type => (Type: type, Category: type.Category.GetBuiltInCategory()))
            .Where(item => item.Category != BuiltInCategory.OST_Walls || item.Type is WallType)
            .Where(item => item.Type is not FamilySymbol symbol || symbol.Family is not null)
            .Select(item => new StructureTypeSelection(
                new StructureTypeSettings(item.Category.ToString(), (item.Type as FamilySymbol)?.Family.Name,
                    item.Type.Name), StructureTypeResolution.Resolved, item.Type.UniqueId))
            .ToList().AsReadOnly();
    }

    /// <summary>Запоминает исходные вид и выделение и определяет диапазон по подходящим экземплярам.</summary>
    public ManagerSessionState CaptureInitialState(ManagerConfigSnapshot config) {
        var uiDocument = ActiveUIDocument;
        var view = uiDocument.ActiveView;
        var selection = uiDocument.Selection.GetElementIds();
        var phaseId = GetReadPhaseId(config);
        var scope = HasScopeElements(ManagerScope.Selected, view, selection, config, phaseId) ? ManagerScope.Selected
            : HasScopeElements(ManagerScope.View, view, selection, config, phaseId) ? ManagerScope.View
            : ManagerScope.Project;
        return new ManagerSessionState(config, Document.Title, view?.UniqueId, GetSelectionUniqueIds(selection), scope);
    }

    /// <summary>Читает контекст действующего диапазона из проверки исходного документа на Idling.</summary>
    public ManagerScopeStamp CaptureScopeStamp(ManagerScope scope) {
        var uiDocument = ActiveUIDocument;
        var selection = scope == ManagerScope.Selected ? uiDocument.Selection.GetElementIds()
            : Array.Empty<ElementId>();
        string viewUniqueId = scope == ManagerScope.View ? uiDocument.ActiveView?.UniqueId : null;
        return CreateScopeStamp(scope, viewUniqueId, selection);
    }

    /// <summary>Читает снимки выбранного диапазона только из Execute обработчика ExternalEvent.</summary>
    /// <param name="app">Приложение текущего API-вызова.</param>
    /// <param name="request">Неизменяемая идентичность запроса.</param>
    /// <param name="state">Зафиксированные настройки, исходные вид и выделение сессии.</param>
    /// <returns>Независимый контекст или null при чужом/недоступном документе.</returns>
    public ManagerDocumentContext ReadDocumentContext(UIApplication app, ManagerRequest request,
        ManagerSessionState state) {
        var activeDocument = app.ActiveUIDocument;
        if(!Document.IsValidObject || activeDocument?.Document?.IsValidObject != true
            || !Document.Equals(activeDocument.Document) || Document.IsReadOnly || Document.IsModifiable) {
            return null;
        }
        var phaseId = GetReadPhaseId(state.Config);
        var view = activeDocument.ActiveView;
        if(request.UseInitialContext && request.Scope == ManagerScope.View) {
            view = state.InitialViewUniqueId is string initialView
                ? ReadOptional(() => Document.GetElement(initialView) as View) : null;
        }
        var selection = GetScopeSelection(activeDocument, request, state);
        using var collector = CreateScopeCollector(request.Scope, view, selection);
        var project = ReadScopeSnapshot(state.Config, phaseId, request.Scope, collector);
        string viewUniqueId = view?.UniqueId;
        return new ManagerDocumentContext(request.DocumentToken, request.Generation, Document.Title,
            viewUniqueId, view?.Name, GetSelectionUniqueIds(selection), project,
            CreateScopeStamp(request.Scope, viewUniqueId, selection), GetRemovedKeys(request));
    }

    // Уточняет исчезновение по UniqueId, чтобы сохранить состояние после Undo до обновления.
    private IEnumerable<ManagerItemKey> GetRemovedKeys(ManagerRequest request) {
        return request.DeletionCandidates.Where(key => key.DocumentToken == request.DocumentToken
            && (IsMissing(key.OpeningUniqueId) || IsMissing(key.LintelUniqueId)));
    }

    // Проверка выполняется только в исходном документе и в том же запросе, что и чтение диапазона.
    private bool IsMissing(string uniqueId) {
        return uniqueId is not null && Document.GetElement(uniqueId)?.IsValidObject != true;
    }

    /// <summary>Освобождает API-каталоги этапа старта перед немодальной работой.</summary>
    public void ReleaseCatalogs() {
        _lintelFamilies = null;
        _openingFamilies = null;
        _phaseIds = null;
        _revitWalls = null;
    }

    // Получает каталог семейств перемычек.
    private IEnumerable<RevitFamily> GetLintelFamilies() {
        return GetRevitFamilies(systemPluginConfig.DefaultLintelCats.ToList());
    }

    // Получает каталог семейств проёмов.
    private IEnumerable<RevitFamily> GetOpeningFamilies() {
        return GetRevitFamilies(systemPluginConfig.DefaultOpeningCats.ToList());
    }

    // Получает типы семейств допустимых категорий.
    private IEnumerable<FamilySymbol> GetFamilySymbols(IEnumerable<BuiltInCategory> categories) {
        return categories
            .SelectMany(category =>
                new FilteredElementCollector(Document)
                    .OfCategory(category)
                    .WhereElementIsElementType()
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>())
            .Where(symbol => symbol.Family is not null)
            .GroupBy(symbol => symbol.Id)
            .Select(group => group.First());
    }
    
    // Получает экземпляры допустимых категорий.
    private IEnumerable<FamilyInstance> GetFamilyInstances(IEnumerable<BuiltInCategory> categories) {
        return categories
            .SelectMany(category =>
                new FilteredElementCollector(Document)
                    .OfCategory(category)
                    .WhereElementIsNotElementType()
                    .OfClass(typeof(FamilyInstance))
                    .Cast<FamilyInstance>())
            .Where(instance => instance.Symbol?.Family != null)
            .GroupBy(instance => instance.Id)
            .Select(group => group.First());
    }
    
    // Материализует каталог семейств с отложенным чтением параметров.
    private IEnumerable<RevitFamily> GetRevitFamilies(IList<BuiltInCategory> categories) {
        var symbols = GetFamilySymbols(categories);

        var instancesByFamilyId = GetFamilyInstances(categories)
            .ToLookup(instance => instance.Symbol.Family.Id);

        return symbols
            .GroupBy(symbol => symbol.Family.Id)
            .Select(group => {
                var firstSymbol = group.First();
                var instances = instancesByFamilyId[group.Key].ToList();
                var firstInstance = instances.FirstOrDefault();

                var family = new RevitFamily {
                    Family = firstSymbol.Family,
                    FamilySymbols = group.ToList(),
                    FamilyInstances = instances
                };
                family.SetParameterProvider(() =>
                    paramService.CreateRevitParams(Document, firstSymbol, firstInstance));
                return family;
            }).ToArray();
    }
    
    // Получает типы стен исходного документа.
    private IEnumerable<WallType> GetWallTypes() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(WallType))
            .Cast<WallType>()
            .GroupBy(x => x.Id)
            .Select(x => x.First());
    }

    // Получает экземпляры стен исходного документа.
    private IEnumerable<Wall> GetWalls() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(Wall))
            .WhereElementIsNotElementType()
            .Cast<Wall>()
            .GroupBy(x => x.Id)
            .Select(x => x.First());
    }

    // Сопоставляет типы стен с экземплярами.
    private IEnumerable<RevitWall> GetRevitWalls() {
        var wallTypes = GetWallTypes();

        var wallsByTypeId = GetWalls()
            .ToLookup(x => x.GetTypeId());

        return wallTypes.Select(wallType => new RevitWall {
            WallType = wallType,
            Walls = wallsByTypeId[wallType.Id].ToList()
        });
    }
    
    // Метод получения стадий проекта
    private IEnumerable<ElementId> GetPhaseIds() {
        var phases = Document.Phases;
        return phases
            .Cast<Phase>()
            .Select(phase => phase.Id);
    }
}
