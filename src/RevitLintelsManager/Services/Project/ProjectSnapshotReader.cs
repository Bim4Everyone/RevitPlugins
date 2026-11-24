using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitLintelsManager.Models.Configs;

using static RevitLintelsManager.Models.RevitElementRead;
using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Models;

/// <summary>Собирает независимые снимки элементов и объединяет результаты анализа одного чтения.</summary>
internal sealed class ProjectSnapshotReader(RevitRepository source, SystemPluginConfig systemPluginConfig,
    IParamService paramService, Func<IGeometryReadContext> openGeometry, IPlacementGeometryReader placements,
    IAboveOpeningAnalyzer aboveOpenings, ISideBearingAnalyzer sideBearings) : IProjectSnapshotReader {
    private Document Document => source.Document;

    /// <inheritdoc />
    public bool HasMatchingFamilies(FilteredElementCollector collector, ManagerConfigSnapshot config,
        ElementId phaseId) {
        return GetMatchingFamilies(collector, config, phaseId).Any();
    }

    private sealed class StructureCatalog {
        public List<StructureCandidate> Upper { get; } = [];
        public List<StructureCandidate> Sides { get; } = [];
        public bool UpperIncomplete { get; set; }
        public bool SidesIncomplete { get; set; }
    }

    private sealed class ProjectFamilyType(FamilySymbol symbol, Family family, bool isOpening) {
        public FamilySymbol Symbol { get; } = symbol;
        public string FamilyUniqueId { get; } = family.UniqueId;
        public string FamilyName { get; } = family.Name;
        public string TypeUniqueId { get; } = symbol.UniqueId;
        public string TypeName { get; } = symbol.Name;
        public bool IsOpening { get; } = isOpening;
    }

    // Собирает факты экземпляров из переданного диапазона и опорные данные по всему проекту при его выборе.
    public ManagerProjectSnapshot ReadScopeSnapshot(ManagerConfigSnapshot config, ElementId phaseId,
        ManagerScope scope, FilteredElementCollector collector) {
        var openings = new List<ManagerOpeningSnapshot>();
        var lintels = new List<ManagerLintelSnapshot>();
        var walls = new List<ManagerWallSnapshot>();
        var structures = new List<ManagerStructureSnapshot>();
        var phaseNames = Document.Phases.Cast<Phase>().ToDictionary(item => item.Id, item => item.Name);
        HashSet<long> scopeIds = null;
        if(scope != ManagerScope.Project) {
            using var categories = new ElementMulticategoryFilter(
                systemPluginConfig.DefaultOpeningCats.Union(systemPluginConfig.DefaultLintelCats).ToList());
            scopeIds = collector is null ? [] : collector.WherePasses(categories).OfClass(typeof(FamilyInstance))
                .WhereElementIsNotElementType().ToElementIds().Select(id => (long) id.GetIdValue()).ToHashSet();
            if(scopeIds.Count == 0) {
                return new ManagerProjectSnapshot(openings, lintels, walls, structures, 0, []);
            }
        }
        using var projectCollector = scopeIds is null ? null : new FilteredElementCollector(Document);
        using var analysis = openGeometry();
        int unreadableCount = ReadFamilySnapshots(projectCollector ?? collector, config, phaseId, phaseNames,
            openings, lintels, analysis);
        var seeds = scopeIds is null ? null : openings.Select(o => o.Element).Concat(lintels.Select(l => l.Element))
            .Where(element => element.Information.ElementId is long id && scopeIds.Contains(id))
            .Select(element => element.UniqueId).ToHashSet();
        var matches = OpeningLintelMatcher.Match(openings, lintels, seeds, config.MatchLimits, LengthEpsilon,
            unreadableCount > 0, key => ReadOptional(() => analysis.GetBounds(Document.GetElement(key))));
        if(seeds is not null) {
            openings.RemoveAll(o => !seeds.Contains(o.Element.UniqueId));
            lintels.RemoveAll(l => !seeds.Contains(l.Element.UniqueId));
        }
        var rowOpenings = matches.Where(row => row.Opening is not null).Select(row => row.Opening).ToList();
        bool projectScope = scope == ManagerScope.Project;
        var catalog = new StructureCatalog();
        if(projectScope || rowOpenings.Any(o => o.Geometry?.Cut is not null)) {
            unreadableCount += ReadProjectStructures(config, phaseId, phaseNames, walls, structures,
                projectScope, catalog);
        }
        var above = aboveOpenings.ReadAboveOpenings(analysis, rowOpenings, catalog.Upper, catalog.UpperIncomplete,
            config.MinimalHeightAboveOpeningMm);
        var sides = sideBearings.ReadSideBearings(analysis, rowOpenings, catalog.Sides,
            catalog.SidesIncomplete, config);
        return new ManagerProjectSnapshot(openings, lintels, walls, structures, unreadableCount, matches, above, sides);
    }

    // Материализует экземпляры области с переиспользованием справочных данных за одно чтение.
    private int ReadFamilySnapshots(FilteredElementCollector collector, ManagerConfigSnapshot config, ElementId phaseId,
        IReadOnlyDictionary<ElementId, string> phaseNames,
        ICollection<ManagerOpeningSnapshot> openings, ICollection<ManagerLintelSnapshot> lintels,
        IGeometryReadContext analysis) {
        var elementNames = new Dictionary<ElementId, string>();
        var worksetNames = new Dictionary<WorksetId, string>();
        using var worksets = Document.GetWorksetTable();
        Func<ElementId, string> readElementName = id => Document.GetElement(id)?.Name;
        Func<WorksetId, string> readWorksetName = id => worksets.GetWorkset(id)?.Name;
        Func<ElementId, string> getElementName = id => id is null || id == ElementId.InvalidElementId
            ? null : GetCachedName(id, elementNames, readElementName);
        Func<WorksetId, string> getWorksetName = id => id is null
            ? null : GetCachedName(id, worksetNames, readWorksetName);
        int unreadableCount = 0;
        foreach(var (instance, type) in GetMatchingFamilies(collector, config, phaseId, () => unreadableCount++)) {
            try {
                var element = ReadFamilySnapshot(instance, type, phaseNames, config.Parameters,
                    getElementName, getWorksetName);
                if(type.IsOpening) {
                    openings.Add(ReadOpening(instance, type.Symbol, element, config.Parameters, analysis));
                } else {
                    lintels.Add(ReadLintel(instance, type.Symbol, element, config.Parameters, analysis));
                }
            } catch(Exception exception) when(IsElementReadException(exception)) {
                unreadableCount++;
            }
        }
        return unreadableCount;
    }

    // Применяет общий отбор экземпляров по категории, семейству и стадии внутри одного API-вызова.
    private IEnumerable<(FamilyInstance Instance, ProjectFamilyType Type)> GetMatchingFamilies(
        FilteredElementCollector collector, ManagerConfigSnapshot config, ElementId phaseId, Action unreadable = null) {
        using var categories = new ElementMulticategoryFilter(
            systemPluginConfig.DefaultOpeningCats.Union(systemPluginConfig.DefaultLintelCats).ToList());
        var openingFamilies = new HashSet<string>(config.OpeningFamilies.Select(family => family.UniqueId));
        var types = new Dictionary<ElementId, ProjectFamilyType>();
        collector.WherePasses(categories).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType();
        foreach(FamilyInstance instance in collector) {
            ProjectFamilyType type;
            try {
                var typeId = instance.GetTypeId();
                if(typeId is null || typeId == ElementId.InvalidElementId) {
                    unreadable?.Invoke();
                    continue;
                }
                if(!types.TryGetValue(typeId, out type)) {
                    type = ResolveProjectFamilyType(typeId, openingFamilies, config.LintelFamily.UniqueId);
                    types.Add(typeId, type);
                }
                if(type is null || !ExistsOnPhase(instance, phaseId)) {
                    continue;
                }
            } catch(Exception exception) when(IsElementReadException(exception)) {
                unreadable?.Invoke();
                continue;
            }
            yield return (instance, type);
        }
    }

    // Сопоставляет тип с настроенным семейством по UniqueId.
    private ProjectFamilyType ResolveProjectFamilyType(ElementId typeId, HashSet<string> openingFamilies,
        string lintelFamily) {
        if(Document.GetElement(typeId) is not FamilySymbol symbol || symbol.Family is not Family family
            || symbol.Category is not Category category) {
            return null;
        }
        string familyId = family.UniqueId;
        var categoryId = category.GetBuiltInCategory();
        bool isOpening = openingFamilies.Contains(familyId)
            && systemPluginConfig.DefaultOpeningCats.Contains(categoryId);
        bool isLintel = familyId == lintelFamily && systemPluginConfig.DefaultLintelCats.Contains(categoryId);
        return isOpening || isLintel ? new ProjectFamilyType(symbol, family, isOpening) : null;
    }

    // Материализует сведения элемента, оставляя недоступные поля пустыми.
    private ManagerFamilySnapshot ReadFamilySnapshot(FamilyInstance instance, ProjectFamilyType type,
        IReadOnlyDictionary<ElementId, string> phaseNames, IReadOnlyDictionary<string, string> parameters,
        Func<ElementId, string> getElementName, Func<WorksetId, string> getWorksetName) {
        var groupId = ReadOptional(() => instance.GroupId);
        bool? isInGroup = groupId is null ? null : groupId != ElementId.InvalidElementId;
        string groupName = isInGroup == true ? ReadOptional(() => getElementName(groupId)) : null;
        string fixationParameter = parameters[nameof(LintelManagerConfig.LintelFixParam)];
        var information = new ManagerElementInfo(
            type.FamilyName,
            ReadOptional(() => getWorksetName(instance.WorksetId)),
            ReadOptional(() => getElementName(instance.LevelId)),
            groupName,
            ReadOptional(() => paramService.ReadFixation(instance, fixationParameter)),
            instance.Id.GetIdValue(), phases: ReadPhases(instance, phaseNames), isInGroup: isInGroup);
        return new ManagerFamilySnapshot(instance.UniqueId, type.FamilyUniqueId, type.TypeUniqueId,
            type.TypeName, information);
    }

    // Сохраняет фактического хоста-стену и размеры из настроенных параметров проёма.
    private ManagerOpeningSnapshot ReadOpening(FamilyInstance instance, FamilySymbol type,
        ManagerFamilySnapshot element, IReadOnlyDictionary<string, string> parameters, IGeometryReadContext analysis) {
        string wall = ReadOptional(() => (instance.Host as Wall)?.UniqueId);
        double? width = ReadOptional(() => paramService.ReadLengthMm(instance, type,
            parameters[nameof(OpeningFamilyConfig.LintelOpeningWidth)]));
        double? height = ReadOptional(() => paramService.ReadLengthMm(instance, type,
            parameters[nameof(OpeningFamilyConfig.LintelOpeningHeight)]));
        var geometry = placements.ReadOpeningGeometry(analysis, instance, width, height);
        return new ManagerOpeningSnapshot(element, wall, width, height, geometry);
    }

    // Читает настроенные параметры перемычки без вычисления опирания и креплений.
    private ManagerLintelSnapshot ReadLintel(FamilyInstance instance, FamilySymbol type, ManagerFamilySnapshot element,
        IReadOnlyDictionary<string, string> parameters, IGeometryReadContext analysis) {
        double? Length(string key) => ReadOptional(() => paramService.ReadLengthMm(instance, type, parameters[key]));
        int? Integer(string key) => ReadOptional(() => paramService.ReadInteger(instance, type, parameters[key]));

        return new ManagerLintelSnapshot(element,
            Length(nameof(LintelFamilyConfig.LintelWidth)), Length(nameof(LintelFamilyConfig.LintelThickness)),
            Length(nameof(LintelFamilyConfig.LintelLeftOffset)), Length(nameof(LintelFamilyConfig.LintelRightOffset)),
            Integer(nameof(LintelFamilyConfig.LintelLeftCorner)), Integer(nameof(LintelFamilyConfig.LintelRightCorner)),
            Integer(nameof(LintelFamilyConfig.LintelLeftWelding)),
            Integer(nameof(LintelFamilyConfig.LintelRightWelding)), placements.ReadLintelGeometry(analysis, instance));
    }

    // Читает стены и экземпляры настроенных конструкций в одном проходе по допустимым категориям.
    private int ReadProjectStructures(ManagerConfigSnapshot config, ElementId phaseId,
        IReadOnlyDictionary<ElementId, string> phaseNames,
        ICollection<ManagerWallSnapshot> walls, ICollection<ManagerStructureSnapshot> structures, bool projectScope,
        StructureCatalog catalog) {
        var concrete = new HashSet<string>(config.ReinforcedConcreteTypes.Select(type => type.UniqueId));
        concrete.Remove(null);
        var steel = new HashSet<string>(config.SteelTypes.Select(type => type.UniqueId));
        steel.Remove(null);
        var concreteCategories = new HashSet<string>(
            config.ReinforcedConcreteTypes.Select(type => type.Settings.Category));
        var selectedCategories = new HashSet<string>(concreteCategories);
        selectedCategories.UnionWith(config.SteelTypes.Select(type => type.Settings.Category));
        var categories = systemPluginConfig.ReinforcedConcreteCategories.Union(systemPluginConfig.SteelCategories)
            .Where(category => category is BuiltInCategory.OST_Floors or BuiltInCategory.OST_StructuralFraming
                || (projectScope && category == BuiltInCategory.OST_Walls)
                || selectedCategories.Contains(category.ToString()));
        using var filter = new ElementMulticategoryFilter(categories.ToList());
        using var collector = new FilteredElementCollector(Document);
        var typeKeys = new Dictionary<ElementId, string>();
        int unreadableCount = 0;
        foreach(var element in collector.WherePasses(filter).WhereElementIsNotElementType()) {
            bool possibleUpper = true;
            bool possibleSide = selectedCategories.Count > 0;
            try {
                var category = element.Category.GetBuiltInCategory();
                bool upperCategory = category is BuiltInCategory.OST_Floors or BuiltInCategory.OST_StructuralFraming;
                possibleUpper = upperCategory || concreteCategories.Contains(category.ToString());
                possibleSide = selectedCategories.Contains(category.ToString());
                var typeId = element.GetTypeId();
                if(!typeKeys.TryGetValue(typeId, out string typeKey)) {
                    typeKey = typeId == ElementId.InvalidElementId ? null : Document.GetElement(typeId)?.UniqueId;
                    typeKeys.Add(typeId, typeKey);
                }
                bool isConcrete = concrete.Contains(typeKey);
                bool isSteel = steel.Contains(typeKey);
                possibleUpper = upperCategory || isConcrete || (possibleUpper && typeKey is null);
                possibleSide = isConcrete || isSteel || (possibleSide && typeKey is null);
                if(!possibleUpper && !possibleSide && !(projectScope && element is Wall)) {
                    continue;
                }
                if(!ExistsOnPhase(element, phaseId)) {
                    continue;
                }
                if(typeKey is null) {
                    catalog.UpperIncomplete |= possibleUpper;
                    catalog.SidesIncomplete |= possibleSide;
                } else if(possibleUpper || possibleSide) {
                    var candidate = new StructureCandidate(element, element.UniqueId, typeKey, category,
                        isConcrete, isSteel);
                    if(possibleUpper) {
                        catalog.Upper.Add(candidate);
                    }
                    if(possibleSide) {
                        catalog.Sides.Add(candidate);
                    }
                }
                possibleUpper = false;
                possibleSide = false;
                if(projectScope && (element is Wall || isConcrete || isSteel)) {
                    AddStructureSnapshot(element, typeKey, isConcrete, isSteel, phaseNames, walls, structures);
                }
            } catch(Exception exception) when(IsElementReadException(exception)) {
                catalog.UpperIncomplete |= possibleUpper;
                catalog.SidesIncomplete |= possibleSide;
                unreadableCount++;
            }
        }
        return unreadableCount;
    }

    // Хранит ЖБ-стену в снимках стен с признаком настройки, без дублирования в конструкциях.
    private static void AddStructureSnapshot(Element element, string typeKey, bool isConcrete, bool isSteel,
        IReadOnlyDictionary<ElementId, string> phaseNames,
        ICollection<ManagerWallSnapshot> walls, ICollection<ManagerStructureSnapshot> structures) {
        string uniqueId = element.UniqueId;
        long elementId = element.Id.GetIdValue();
        var phases = ReadPhases(element, phaseNames);
        if(element is Wall wall) {
            double? width = ReadOptional<double?>(() => UnitUtils.ConvertFromInternalUnits(wall.Width,
                UnitTypeId.Millimeters));
            walls.Add(new ManagerWallSnapshot(uniqueId, elementId, typeKey, width, isConcrete, phases));
        } else {
            string category = element.Category?.GetBuiltInCategory().ToString();
            structures.Add(new ManagerStructureSnapshot(uniqueId, elementId, typeKey, category, isSteel, phases));
        }
    }

    // Стадию New или Existing Revit определяет независимо от фильтра стадий активного вида.
    private static bool ExistsOnPhase(Element element, ElementId phaseId) {
        return element.GetPhaseStatus(phaseId) is ElementOnPhaseStatus.Existing or ElementOnPhaseStatus.New;
    }

    // Различает неназначенную стадию и недоступные сведения о ней.
    private static ManagerElementPhases ReadPhases(Element element, IReadOnlyDictionary<ElementId, string> names) {
        string Name(ElementId id) => id is null ? null : id == ElementId.InvalidElementId ? string.Empty
            : names.TryGetValue(id, out string name) ? name : null;

        var created = ReadOptional(() => element.CreatedPhaseId);
        var demolished = ReadOptional(() => element.DemolishedPhaseId);
        return new ManagerElementPhases(Name(created), Name(demolished),
            created?.GetIdValue(), demolished?.GetIdValue());
    }
}
