using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models;

/// <summary>Каталоги семейств, стен, стадий и типов конструкций для настройки проекта.</summary>
internal sealed class SettingsCatalog(RevitRepository source, SystemPluginConfig systemPluginConfig,
    IParamService paramService) : ISettingsCatalog {
    private IEnumerable<RevitFamily> _lintelFamilies;
    private IEnumerable<RevitFamily> _openingFamilies;
    private IEnumerable<RevitWall> _revitWalls;

    private Document Document => source.Document;
    public string ProjectTitle => Document.Title;
    public IEnumerable<string> PhaseNames => Document.Phases.Cast<Phase>().Select(phase => phase.Name);

    /// <inheritdoc />
    public ConfigElementKey GetPhaseKey(ElementId id) {
        var phase = Document.GetElement(id);
        return new ConfigElementKey(phase.UniqueId, phase.Name);
    }

    public IEnumerable<RevitFamily> LintelFamilies => _lintelFamilies ??= GetLintelFamilies();
    public IEnumerable<RevitFamily> OpeningFamilies => _openingFamilies ??= GetOpeningFamilies();
    public IEnumerable<RevitWall> RevitWalls => _revitWalls ??= GetRevitWalls().ToArray();

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
        using var filter = new ElementMulticategoryFilter(categories.ToList());
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

    /// <summary>Освобождает API-каталоги этапа старта перед немодальной работой.</summary>
    public void ReleaseCatalogs() {
        _lintelFamilies = null;
        _openingFamilies = null;
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
