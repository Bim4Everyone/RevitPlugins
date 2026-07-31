using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

internal class RevitRepository(UIApplication uiApplication, SystemPluginConfig systemPluginConfig, ParamService paramService) {
    private IEnumerable<RevitFamily> _lintelFamilies;
    private IEnumerable<RevitFamily> _openingFamilies;
    private IEnumerable<ElementId> _phaseIds;
    
    
    public UIApplication UIApplication { get; } = uiApplication;
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document => ActiveUIDocument.Document;
    
    
    public IEnumerable<RevitFamily> LintelFamilies => _lintelFamilies ??= GetLintelFamilies();
    public IEnumerable<RevitFamily> OpeningFamilies => _openingFamilies ??= GetOpeningFamilies();
    public IEnumerable<RevitWall> RevitWalls => GetRevitWalls();
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

    private IEnumerable<RevitFamily> GetLintelFamilies() {
        return GetRevitFamilies(systemPluginConfig.DefaultLintelCats.ToList());
    }

    private IEnumerable<RevitFamily> GetOpeningFamilies() {
        return GetRevitFamilies(systemPluginConfig.DefaultOpeningCats.ToList());
    }

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

                return new RevitFamily {
                    Family = firstSymbol.Family,
                    FamilySymbols = group.ToList(),
                    FamilyInstances = instancesByFamilyId[group.Key].ToList(),
                    OrderedParams = paramService.CreateRevitParams(Document, firstSymbol, firstInstance)
                };
            });
    }
    
    private IEnumerable<WallType> GetWallTypes() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(WallType))
            .Cast<WallType>()
            .GroupBy(x => x.Id)
            .Select(x => x.First());
    }

    private IEnumerable<Wall> GetWalls() {
        return new FilteredElementCollector(Document)
            .OfClass(typeof(Wall))
            .WhereElementIsNotElementType()
            .Cast<Wall>()
            .GroupBy(x => x.Id)
            .Select(x => x.First());
    }

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
