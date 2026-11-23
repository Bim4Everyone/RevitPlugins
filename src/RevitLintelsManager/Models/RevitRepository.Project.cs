using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitLintelsManager.Models.Configs;

using RevitExceptions = Autodesk.Revit.Exceptions;

namespace RevitLintelsManager.Models;

internal partial class RevitRepository {
    private sealed class ProjectFamilyType(FamilySymbol symbol, Family family, bool isOpening) {
        public FamilySymbol Symbol { get; } = symbol;
        public string FamilyUniqueId { get; } = family.UniqueId;
        public string FamilyName { get; } = family.Name;
        public string TypeUniqueId { get; } = symbol.UniqueId;
        public string TypeName { get; } = symbol.Name;
        public bool IsOpening { get; } = isOpening;
    }

    // Собирает данные документа целиком, не читая геометрию и видимость на виде.
    private ManagerProjectSnapshot ReadProject(ManagerConfigSnapshot config) {
        if(Document.GetElement(config.Phase.UniqueId) is not Phase phase) {
            throw new InvalidOperationException("Настроенная стадия удалена из исходного документа.");
        }
        var openings = new List<ManagerOpeningSnapshot>();
        var lintels = new List<ManagerLintelSnapshot>();
        var walls = new List<ManagerWallSnapshot>();
        var structures = new List<ManagerStructureSnapshot>();
        var phaseNames = Document.Phases.Cast<Phase>().ToDictionary(item => item.Id, item => item.Name);
        var phaseId = phase.Id;
        int unreadableCount = ReadProjectFamilies(config, phaseId, phaseNames, openings, lintels)
            + ReadProjectStructures(config, phaseId, phaseNames, walls, structures);
        return new ManagerProjectSnapshot(openings, lintels, walls, structures, unreadableCount);
    }

    // Разрешает семейство по ключу один раз на тип; экземпляры отбирает без фильтра по активному виду.
    private int ReadProjectFamilies(ManagerConfigSnapshot config, ElementId phaseId,
        IReadOnlyDictionary<ElementId, string> phaseNames,
        ICollection<ManagerOpeningSnapshot> openings, ICollection<ManagerLintelSnapshot> lintels) {
        using var categories = new ElementMulticategoryFilter(
            systemPluginConfig.DefaultOpeningCats.Union(systemPluginConfig.DefaultLintelCats).ToList());
        var openingFamilies = new HashSet<string>(config.OpeningFamilies.Select(family => family.UniqueId));
        var types = new Dictionary<ElementId, ProjectFamilyType>();
        var elementNames = new Dictionary<ElementId, string>();
        var worksetNames = new Dictionary<WorksetId, string>();
        using var worksets = Document.GetWorksetTable();
        Func<ElementId, string> readElementName = id => Document.GetElement(id)?.Name;
        Func<WorksetId, string> readWorksetName = id => worksets.GetWorkset(id)?.Name;
        Func<ElementId, string> getElementName = id => id is null || id == ElementId.InvalidElementId
            ? null : GetCachedName(id, elementNames, readElementName);
        Func<WorksetId, string> getWorksetName = id => id is null
            ? null : GetCachedName(id, worksetNames, readWorksetName);
        using var instances = new FilteredElementCollector(Document);
        instances.WherePasses(categories).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType();
        int unreadableCount = 0;
        foreach(FamilyInstance instance in instances) {
            try {
                var typeId = instance.GetTypeId();
                if(typeId is null || typeId == ElementId.InvalidElementId) {
                    unreadableCount++;
                    continue;
                }
                if(!types.TryGetValue(typeId, out var type)) {
                    type = ResolveProjectFamilyType(typeId, openingFamilies, config.LintelFamily.UniqueId);
                    types.Add(typeId, type);
                }
                if(type is null || !ExistsOnPhase(instance, phaseId)) {
                    continue;
                }
                var element = ReadFamilySnapshot(instance, type, phaseNames, config.Parameters,
                    getElementName, getWorksetName);
                if(type.IsOpening) {
                    openings.Add(ReadOpening(instance, type.Symbol, element, config.Parameters));
                } else {
                    lintels.Add(ReadLintel(instance, type.Symbol, element, config.Parameters));
                }
            } catch(Exception exception) when(IsElementReadException(exception)) {
                unreadableCount++;
            }
        }
        return unreadableCount;
    }

    // Сопоставляет тип с настроенным семейством по UniqueId.
    private ProjectFamilyType ResolveProjectFamilyType(ElementId typeId, HashSet<string> openingFamilies,
        string lintelFamily) {
        if(Document.GetElement(typeId) is not FamilySymbol symbol || symbol.Family is not Family family) {
            return null;
        }
        string familyId = family.UniqueId;
        bool isOpening = openingFamilies.Contains(familyId);
        return isOpening || familyId == lintelFamily ? new ProjectFamilyType(symbol, family, isOpening) : null;
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
        ManagerFamilySnapshot element, IReadOnlyDictionary<string, string> parameters) {
        string wall = ReadOptional(() => (instance.Host as Wall)?.UniqueId);
        return new ManagerOpeningSnapshot(element, wall,
            ReadOptional(() => paramService.ReadLengthMm(instance, type,
                parameters[nameof(OpeningFamilyConfig.LintelOpeningWidth)])),
            ReadOptional(() => paramService.ReadLengthMm(instance, type,
                parameters[nameof(OpeningFamilyConfig.LintelOpeningHeight)])));
    }

    // Читает настроенные параметры перемычки без вычисления опирания и креплений.
    private ManagerLintelSnapshot ReadLintel(FamilyInstance instance, FamilySymbol type, ManagerFamilySnapshot element,
        IReadOnlyDictionary<string, string> parameters) {
        double? Length(string key) => ReadOptional(() => paramService.ReadLengthMm(instance, type, parameters[key]));
        int? Integer(string key) => ReadOptional(() => paramService.ReadInteger(instance, type, parameters[key]));

        return new ManagerLintelSnapshot(element,
            Length(nameof(LintelFamilyConfig.LintelWidth)), Length(nameof(LintelFamilyConfig.LintelThickness)),
            Length(nameof(LintelFamilyConfig.LintelLeftOffset)), Length(nameof(LintelFamilyConfig.LintelRightOffset)),
            Integer(nameof(LintelFamilyConfig.LintelLeftCorner)), Integer(nameof(LintelFamilyConfig.LintelRightCorner)),
            Integer(nameof(LintelFamilyConfig.LintelLeftWelding)),
            Integer(nameof(LintelFamilyConfig.LintelRightWelding)));
    }

    // Читает стены и экземпляры настроенных конструкций в одном проходе по допустимым категориям.
    private int ReadProjectStructures(ManagerConfigSnapshot config, ElementId phaseId,
        IReadOnlyDictionary<ElementId, string> phaseNames,
        ICollection<ManagerWallSnapshot> walls, ICollection<ManagerStructureSnapshot> structures) {
        var concrete = new HashSet<string>(config.ReinforcedConcreteTypes.Select(type => type.UniqueId));
        var steel = new HashSet<string>(config.SteelTypes.Select(type => type.UniqueId));
        var selectedCategories = new HashSet<string>(config.ReinforcedConcreteTypes.Concat(config.SteelTypes)
            .Select(type => type.Settings.Category));
        var categories = systemPluginConfig.ReinforcedConcreteCategories.Union(systemPluginConfig.SteelCategories)
            .Where(category => category == BuiltInCategory.OST_Walls
                || selectedCategories.Contains(category.ToString()));
        using var filter = new ElementMulticategoryFilter(categories.ToList());
        using var collector = new FilteredElementCollector(Document);
        var typeKeys = new Dictionary<ElementId, string>();
        int unreadableCount = 0;
        foreach(var element in collector.WherePasses(filter).WhereElementIsNotElementType()) {
            try {
                var typeId = element.GetTypeId();
                if(!typeKeys.TryGetValue(typeId, out string typeKey)) {
                    typeKey = typeId == ElementId.InvalidElementId ? null : Document.GetElement(typeId)?.UniqueId;
                    typeKeys.Add(typeId, typeKey);
                }
                bool isConcrete = concrete.Contains(typeKey);
                bool isSteel = steel.Contains(typeKey);
                if((element is Wall || isConcrete || isSteel) && ExistsOnPhase(element, phaseId)) {
                    AddStructureSnapshot(element, typeKey, isConcrete, isSteel, phaseNames, walls, structures);
                }
            } catch(Exception exception) when(IsElementReadException(exception)) {
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

    // Разрешает часто повторяющиеся справочные имена один раз за чтение.
    private static string GetCachedName<TKey>(TKey id, IDictionary<TKey, string> names, Func<TKey, string> readName) {
        if(!names.TryGetValue(id, out string name)) {
            name = readName(id);
            names.Add(id, name);
        }
        return name;
    }

    // Ошибка отдельного поля не должна скрывать остальную информацию об экземпляре.
    private static T ReadOptional<T>(Func<T> read) {
        try {
            return read();
        } catch(Exception exception) when(IsElementReadException(exception)) {
            return default;
        }
    }

    // Отделяет ошибки доступа к отдельному объекту от ошибок всего запроса.
    private static bool IsElementReadException(Exception exception) {
        return exception is RevitExceptions.InvalidObjectException or RevitExceptions.InvalidOperationException
            or RevitExceptions.ArgumentException or RevitExceptions.InternalException;
    }
}
