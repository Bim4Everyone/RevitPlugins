using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

internal class RevitRepository(UIApplication uiApplication, SystemPluginConfig systemPluginConfig, IParamService paramService) {
    private IEnumerable<RevitFamily> _lintelFamilies;
    private IEnumerable<RevitFamily> _openingFamilies;
    private IEnumerable<ElementId> _phaseIds;
    private IEnumerable<RevitWall> _revitWalls;
    
    
    public UIApplication UIApplication { get; } = uiApplication;
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document { get; } = uiApplication.ActiveUIDocument.Document;
    
    
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

    /// <summary>Сохраняет исходный UI-контекст до будущей подготовки служебного вида.</summary>
    public ManagerSessionState CaptureInitialState(ManagerConfigSnapshot config) {
        var selection = ActiveUIDocument.Selection.GetElementIds();
        var scope = selection.Count > 0 ? ManagerScope.Selected
            : HasManagerElementsOnActiveView(config) ? ManagerScope.View : ManagerScope.Project;
        return new ManagerSessionState(config, Document.Title, ActiveUIDocument.ActiveView?.UniqueId,
            selection.Select(id => Document.GetElement(id)?.UniqueId).Where(id => id is not null), scope);
    }

    /// <summary>Читает текущий вид и выделение только из Execute обработчика ExternalEvent.</summary>
    /// <param name="app">Приложение текущего API-вызова.</param>
    /// <param name="request">Неизменяемая идентичность запроса.</param>
    /// <returns>Независимый контекст или null при чужом/недоступном документе.</returns>
    public ManagerDocumentContext ReadDocumentContext(UIApplication app, ManagerRequest request) {
        var activeDocument = app.ActiveUIDocument;
        if(!Document.IsValidObject || activeDocument?.Document?.IsValidObject != true
            || !Document.Equals(activeDocument.Document) || Document.IsReadOnly || Document.IsModifiable) {
            return null;
        }
        var view = activeDocument.ActiveView;
        return new ManagerDocumentContext(request.DocumentToken, request.Generation, Document.Title,
            view?.UniqueId, view?.Name, activeDocument.Selection.GetElementIds()
                .Select(id => Document.GetElement(id)?.UniqueId).Where(id => id is not null));
    }

    /// <summary>Освобождает API-каталоги этапа старта перед немодальной работой.</summary>
    public void ReleaseCatalogs() {
        _lintelFamilies = null;
        _openingFamilies = null;
        _phaseIds = null;
        _revitWalls = null;
    }

    // Проверяет наличие экземпляров настроенных семейств на исходном виде, не выполняя анализ или запись.
    private bool HasManagerElementsOnActiveView(ManagerConfigSnapshot config) {
        var view = ActiveUIDocument.ActiveView;
        if(view is null || !FilteredElementCollector.IsViewValidForElementIteration(Document, view.Id)) {
            return false;
        }
        var families = new HashSet<string>(config.OpeningFamilies.Select(family => family.UniqueId)) {
            config.LintelFamily.UniqueId
        };
        using var collector = new FilteredElementCollector(Document, view.Id);
        return collector.OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType()
            .Cast<FamilyInstance>().Any(instance => families.Contains(instance.Symbol.Family.UniqueId));
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
