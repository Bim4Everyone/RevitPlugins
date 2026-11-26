using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Services;

using static RevitLintelsManager.Models.RevitElementRead;
using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Models;

/// <summary>Собирает независимые снимки элементов и объединяет результаты анализа одного чтения.</summary>
internal sealed class ProjectSnapshotReader(RevitRepository source, SystemPluginConfig systemPluginConfig,
    IParamService paramService, Func<Document, IGeometryReadContext> openGeometry, IPlacementGeometryReader placements,
    IAboveOpeningAnalyzer aboveOpenings, ISideBearingAnalyzer sideBearings,
    ILintelFamilyFactsReader lintelFamilyFacts, ILintelCalculationService calculation, ILintelLockReader lockReader)
    : IProjectSnapshotReader {
    private sealed class StructureCatalog {
        public List<StructureCandidate> Upper { get; } = [];
        public List<StructureCandidate> Sides { get; } = [];
        public List<StructureCandidate> EndWalls { get; } = [];
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

    private Document Document => source.Document;

    /// <inheritdoc />
    public ManagerLintelSnapshot ReadInstalledLintel(FamilyInstance instance, IGeometryReadContext geometry,
        ManagerConfigSnapshot config) {
        using var worksets = Document.GetWorksetTable();
        var phases = Document.Phases.Cast<Phase>().ToDictionary(phase => phase.Id, phase => phase.Name);
        var type = new ProjectFamilyType(instance.Symbol, instance.Symbol.Family, false);
        var element = ReadFamilySnapshot(instance, type, phases, config.Parameters,
            id => Document.GetElement(id)?.Name, id => worksets.GetWorkset(id)?.Name);
        return ReadLintel(instance, element, config.Parameters, geometry, id => Document.GetElement(id)?.UniqueId);
    }

    /// <inheritdoc />
    public bool HasMatchingFamilies(FilteredElementCollector collector, ManagerConfigSnapshot config,
        ElementId phaseId) {
        return GetMatchingFamilies(collector, config, phaseId).Any();
    }

    /// <inheritdoc />
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
        using var analysis = openGeometry(Document);
        int unreadableCount = ReadFamilySnapshots(projectCollector ?? collector, config, phaseId, phaseNames,
            openings, lintels, analysis);
        string sampleLintelId = lintels.FirstOrDefault()?.Element.UniqueId;
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
        if(projectScope || rowOpenings.Any(o => o.Geometry?.TopCenter is not null)) {
            unreadableCount += ReadProjectStructures(config, phaseId, phaseNames, walls, structures,
                projectScope, catalog);
        }
        var above = aboveOpenings.ReadAboveOpenings(analysis, rowOpenings, catalog.Upper, catalog.UpperIncomplete,
            config.MinimalHeightAboveOpeningMm, phaseId);
        var sides = sideBearings.ReadSideBearings(analysis, rowOpenings, catalog.Sides,
            catalog.SidesIncomplete, config);
        var family = rowOpenings.Count == 0 ? null : lintelFamilyFacts.ReadLintelFamilyFacts(config, sampleLintelId);
        var checkedEnds = sideBearings.ReadRetainedEnds(analysis, matches, catalog.EndWalls, family);
        var calculations = ReadCalculations(matches, above, sides, walls, phaseNames, config, family);
        var rows = lockReader.ReadLocks(analysis, checkedEnds);
        return new ManagerProjectSnapshot(openings, lintels, walls, structures, unreadableCount, rows, above, sides,
            calculations);
    }

    // Собирает хосты видимых строк, включая партнёров за пределами выбранного диапазона.
    private IDictionary<string, LintelCalculation> ReadCalculations(IReadOnlyList<OpeningLintelMatch> matches,
        IReadOnlyList<AboveOpeningFacts> above, IReadOnlyList<OpeningBearingFacts> sides,
        IReadOnlyCollection<ManagerWallSnapshot> walls, IReadOnlyDictionary<ElementId, string> phaseNames,
        ManagerConfigSnapshot config, LintelFamilyFacts family) {
        var byOpening = new Dictionary<string, LintelCalculation>();
        if(!matches.Any(row => row.Opening is not null)) {
            return byOpening;
        }
        var byWall = walls.ToDictionary(wall => wall.UniqueId);
        var aboveById = above.ToDictionary(facts => facts.OpeningId);
        var sidesById = sides.ToDictionary(facts => facts.OpeningId);
        foreach(var match in matches.Where(row => row.Opening is not null)) {
            var opening = match.Opening;
            string id = opening.Element.UniqueId;
            string cutWallId = opening.CutWallUniqueId;
            ManagerWallSnapshot wall = null;
            if(cutWallId is not null && !byWall.TryGetValue(cutWallId, out wall)) {
                wall = ReadOptional(() => ReadCutWall(cutWallId, phaseNames));
                byWall.Add(cutWallId, wall);
            }
            aboveById.TryGetValue(id, out var upper);
            sidesById.TryGetValue(id, out var bearing);
            byOpening.Add(id, calculation.Calculate(opening, wall, upper, bearing, family, config,
                match.Lintel?.Element.UniqueId));
        }
        return byOpening;
    }

    // Читает стену выреза, которая не вошла в общий каталог текущего диапазона.
    private ManagerWallSnapshot ReadCutWall(string id, IReadOnlyDictionary<ElementId, string> phaseNames) {
        if(Document.GetElement(id) is not Wall wall) {
            return null;
        }
        return ReadWall(wall, wall.WallType?.UniqueId, false, phaseNames);
    }

    // Материализует экземпляры области с переиспользованием справочных данных за одно чтение.
    private int ReadFamilySnapshots(FilteredElementCollector collector, ManagerConfigSnapshot config, ElementId phaseId,
        IReadOnlyDictionary<ElementId, string> phaseNames,
        ICollection<ManagerOpeningSnapshot> openings, ICollection<ManagerLintelSnapshot> lintels,
        IGeometryReadContext analysis) {
        var elementNames = new Dictionary<ElementId, string>();
        var elementKeys = new Dictionary<ElementId, string>();
        var worksetNames = new Dictionary<WorksetId, string>();
        using var worksets = Document.GetWorksetTable();
        Func<ElementId, string> readElementName = id => Document.GetElement(id)?.Name;
        Func<ElementId, string> readElementKey = id => Document.GetElement(id)?.UniqueId;
        Func<WorksetId, string> readWorksetName = id => worksets.GetWorkset(id)?.Name;
        Func<ElementId, string> getElementName = id => id is null || id == ElementId.InvalidElementId
            ? null : GetCachedName(id, elementNames, readElementName);
        Func<ElementId, string> getElementKey = id => id is null || id == ElementId.InvalidElementId
            ? null : GetCachedName(id, elementKeys, readElementKey);
        Func<WorksetId, string> getWorksetName = id => id is null
            ? null : GetCachedName(id, worksetNames, readWorksetName);
        int unreadableCount = 0;
        foreach(var (instance, type, hostId, widthMm) in GetMatchingFamilies(
            collector, config, phaseId, () => unreadableCount++)) {
            try {
                var element = ReadFamilySnapshot(instance, type, phaseNames, config.Parameters,
                    getElementName, getWorksetName);
                if(type.IsOpening) {
                    var opening = ReadOpening(instance, type.Symbol, element, hostId, widthMm, config, phaseId,
                        analysis);
                    if(opening is not null) {
                        openings.Add(opening);
                    }
                } else {
                    lintels.Add(ReadLintel(instance, element, config.Parameters, analysis, getElementKey));
                }
            } catch(Exception exception) when(IsElementReadException(exception)) {
                unreadableCount++;
            }
        }
        return unreadableCount;
    }

    // Проверяет стены и диапазоны только для проёмов. Перемычки отбирает по семейству и стадии.
    private IEnumerable<(FamilyInstance Instance, ProjectFamilyType Type, string HostId, double? WidthMm)>
        GetMatchingFamilies(
            FilteredElementCollector collector, ManagerConfigSnapshot config, ElementId phaseId,
            Action unreadable = null) {
        using var categories = new ElementMulticategoryFilter(
            systemPluginConfig.DefaultOpeningCats.Union(systemPluginConfig.DefaultLintelCats).ToList());
        var openingFamilies = new HashSet<string>(config.OpeningFamilies.Select(family => family.UniqueId));
        var concreteWallTypeIds = GetConcreteWallTypeIds(config);
        var types = new Dictionary<ElementId, ProjectFamilyType>();
        var wallTypeKeys = new Dictionary<ElementId, string>();
        var placementRules = PlacementRuleLookup.For(config);
        config.Parameters.TryGetValue(nameof(OpeningFamilyConfig.LintelOpeningWidth), out string widthParameter);
        Func<ElementId, string> readWallTypeKey = id =>
            ReadOptional(() => (Document.GetElement(id) as WallType)?.UniqueId);
        collector.WherePasses(categories).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType();
        foreach(FamilyInstance instance in collector) {
            ProjectFamilyType type;
            string hostId = null;
            double? widthMm = null;
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
                if(type.IsOpening) {
                    var host = ReadOptional(() => instance.Host as Wall);
                    var hostTypeId = ReadOptional(() => host?.GetTypeId());
                    if(hostTypeId is not null && concreteWallTypeIds.Contains(hostTypeId)) {
                        continue;
                    }
                    string wallTypeKey = hostTypeId is null || hostTypeId == ElementId.InvalidElementId
                        ? null : GetCachedName(hostTypeId, wallTypeKeys, readWallTypeKey);
                    if(!placementRules.HasWallType(wallTypeKey)) {
                        continue;
                    }
                    widthMm = ReadOptional(() => paramService.ReadLengthMm(instance, type.Symbol, widthParameter));
                    if(placementRules.Resolve(wallTypeKey, widthMm).Issue == CalculationIssueCode.NoWidthRule) {
                        continue;
                    }
                    hostId = ReadOptional(() => host?.UniqueId);
                }
            } catch(Exception exception) when(IsElementReadException(exception)) {
                unreadable?.Invoke();
                continue;
            }
            yield return (instance, type, hostId, widthMm);
        }
    }

    // Разрешает только настроенные типы стен ЖБ в исходном документе.
    private HashSet<ElementId> GetConcreteWallTypeIds(ManagerConfigSnapshot config) {
        return config.ReinforcedConcreteTypes
            .Where(type => type.Resolution == StructureTypeResolution.Resolved && type.Settings?.IsWallType() == true)
            .Select(type => Document.GetElement(type.UniqueId))
            .OfType<WallType>()
            .Select(type => type.Id)
            .ToHashSet();
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

    // Сохраняет хост Revit и подтверждённую стену выреза отдельно.
    private ManagerOpeningSnapshot ReadOpening(FamilyInstance instance, FamilySymbol type,
        ManagerFamilySnapshot element, string hostId, double? widthMm,
        ManagerConfigSnapshot config, ElementId phaseId, IGeometryReadContext analysis) {
        double? height = ReadOptional(() => paramService.ReadLengthMm(instance, type,
            config.Parameters[nameof(OpeningFamilyConfig.LintelOpeningHeight)]));
        var geometry = placements.ReadOpeningGeometry(analysis, instance, widthMm, height);
        string cutWallId = hostId;
        if(geometry is { Cut: null, OpenTop: null }) {
            var (wall, confirmed) = placements.ReadAdjacentOpeningGeometry(analysis, instance,
                widthMm, height, ReadNearbyRuleWalls(analysis, instance, config, phaseId));
            if(wall is not null) {
                var rule = PlacementRuleLookup.For(config).Resolve(wall.WallType.UniqueId, widthMm);
                if(rule.Issue == CalculationIssueCode.NoWidthRule) {
                    return null;
                }
                geometry = confirmed;
                cutWallId = wall.UniqueId;
            }
        }
        string level = ReadOptional(() => (Document.GetElement(instance.LevelId) as Level)?.UniqueId);
        return new ManagerOpeningSnapshot(element, hostId, widthMm, height, geometry, level, cutWallId);
    }

    // Ограничивает поиск стенами исходной модели, пересекающими рамку проёма на выбранной стадии.
    private IEnumerable<Wall> ReadNearbyRuleWalls(IGeometryReadContext analysis, FamilyInstance instance,
        ManagerConfigSnapshot config, ElementId phaseId) {
        var bounds = analysis.GetBounds(instance);
        if(bounds is null) {
            yield break;
        }
        var rules = PlacementRuleLookup.For(config);
        var concrete = config.ReinforcedConcreteTypes.Select(type => type.UniqueId).ToHashSet();
        using var outline = new Outline(AsXyz(bounds.Min), AsXyz(bounds.Max));
        using var filter = new BoundingBoxIntersectsFilter(outline, LengthEpsilon);
        using var collector = new FilteredElementCollector(Document);
        foreach(Wall wall in collector.OfClass(typeof(Wall)).WherePasses(filter).WhereElementIsNotElementType()) {
            string typeId = ReadOptional(() => wall.WallType?.UniqueId);
            if(rules.HasWallType(typeId) && !concrete.Contains(typeId)
                && ReadOptional(() => ExistsOnPhase(wall, phaseId))) {
                yield return wall;
            }
        }
    }

    // Читает настроенные параметры перемычки без вычисления опирания и креплений.
    private ManagerLintelSnapshot ReadLintel(FamilyInstance instance, ManagerFamilySnapshot element,
        IReadOnlyDictionary<string, string> parameters, IGeometryReadContext analysis,
        Func<ElementId, string> getElementKey) {
        double? Length(string key) => ReadOptional(() => paramService.ReadLengthMm(instance, null, parameters[key]));
        int? Flag(string key) => ReadOptional(() => paramService.ReadFixation(instance, parameters[key])) switch {
            true => 1,
            false => 0,
            _ => null
        };

        return new ManagerLintelSnapshot(element,
            Length(nameof(LintelFamilyConfig.LintelWidth)), Length(nameof(LintelFamilyConfig.LintelThickness)),
            Length(nameof(LintelFamilyConfig.LintelLeftOffset)), Length(nameof(LintelFamilyConfig.LintelRightOffset)),
            Flag(nameof(LintelFamilyConfig.LintelLeftCorner)), Flag(nameof(LintelFamilyConfig.LintelRightCorner)),
            Flag(nameof(LintelFamilyConfig.LintelLeftWelding)), Flag(nameof(LintelFamilyConfig.LintelRightWelding)),
            placements.ReadLintelGeometry(analysis, instance), ReadOptional(() => getElementKey(instance.LevelId)),
            ReadOptional(() => getElementKey(instance.CreatedPhaseId)));
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
        var placementRules = PlacementRuleLookup.For(config);
        var categories = systemPluginConfig.ReinforcedConcreteCategories.Union(systemPluginConfig.SteelCategories)
            .Where(category => category is BuiltInCategory.OST_Floors or BuiltInCategory.OST_StructuralFraming
                or BuiltInCategory.OST_Walls
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
                possibleSide = category == BuiltInCategory.OST_Walls || selectedCategories.Contains(category.ToString());
                var typeId = element.GetTypeId();
                if(!typeKeys.TryGetValue(typeId, out string typeKey)) {
                    typeKey = typeId == ElementId.InvalidElementId ? null : Document.GetElement(typeId)?.UniqueId;
                    typeKeys.Add(typeId, typeKey);
                }
                bool isConcrete = concrete.Contains(typeKey);
                bool isSteel = steel.Contains(typeKey);
                bool isWall = element is Wall;
                bool isMasonry = isWall && !isConcrete && !isSteel && placementRules.HasWallType(typeKey);
                possibleUpper = upperCategory || isConcrete || (possibleUpper && typeKey is null);
                possibleSide = isConcrete || isSteel || isMasonry || (possibleSide && typeKey is null);
                if(!possibleUpper && !possibleSide && !isWall) {
                    continue;
                }
                if(!ExistsOnPhase(element, phaseId)) {
                    continue;
                }
                if(typeKey is null) {
                    catalog.UpperIncomplete |= possibleUpper;
                    catalog.SidesIncomplete |= possibleSide;
                } else {
                    var candidate = new StructureCandidate(element, element.UniqueId, typeKey, category,
                        isConcrete, isSteel);
                    if(possibleUpper) {
                        catalog.Upper.Add(candidate);
                    }
                    if(possibleSide) {
                        catalog.Sides.Add(candidate);
                    }
                    if(isWall) {
                        catalog.EndWalls.Add(candidate);
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
        if(element is Wall wall) {
            walls.Add(ReadWall(wall, typeKey, isConcrete, phaseNames));
        } else {
            string category = element.Category?.GetBuiltInCategory().ToString();
            structures.Add(new ManagerStructureSnapshot(element.UniqueId, element.Id.GetIdValue(), typeKey,
                category, isSteel, ReadPhases(element, phaseNames)));
        }
    }

    // Сохраняет реальный тип и толщину стены независимо от её имени.
    private static ManagerWallSnapshot ReadWall(Wall wall, string typeKey, bool isConcrete,
        IReadOnlyDictionary<ElementId, string> phaseNames) {
        double? width = ReadOptional<double?>(() => UnitUtils.ConvertFromInternalUnits(wall.Width,
            UnitTypeId.Millimeters));
        return new ManagerWallSnapshot(wall.UniqueId, wall.Id.GetIdValue(), typeKey, width, isConcrete,
            ReadPhases(wall, phaseNames));
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
