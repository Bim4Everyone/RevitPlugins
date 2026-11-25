using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Факты исходного документа; ключ документа и поколение хранятся в ответе на запрос.</summary>
internal sealed partial class ManagerProjectSnapshot(IEnumerable<ManagerOpeningSnapshot> openings,
    IEnumerable<ManagerLintelSnapshot> lintels, IEnumerable<ManagerWallSnapshot> walls,
    IEnumerable<ManagerStructureSnapshot> structures, int unreadableCount,
    IEnumerable<OpeningLintelMatch> matches = null) {
    public IReadOnlyList<ManagerOpeningSnapshot> Openings { get; } = openings.ToList().AsReadOnly();
    public IReadOnlyList<ManagerLintelSnapshot> Lintels { get; } = lintels.ToList().AsReadOnly();
    public IReadOnlyList<ManagerWallSnapshot> Walls { get; } = walls.ToList().AsReadOnly();
    public IReadOnlyList<ManagerStructureSnapshot> Structures { get; } = structures.ToList().AsReadOnly();
    public int UnreadableCount { get; } = unreadableCount;
    public IReadOnlyList<OpeningLintelMatch> Matches { get; } = matches?.ToList().AsReadOnly();
    public IReadOnlyDictionary<string, LintelCalculation> Calculations { get; } =
        new ReadOnlyDictionary<string, LintelCalculation>(new Dictionary<string, LintelCalculation>());

    /// <summary>Добавляет расчёты проёмов в то же поколение снимка, что и факты геометрии.</summary>
    public ManagerProjectSnapshot(IEnumerable<ManagerOpeningSnapshot> openings,
        IEnumerable<ManagerLintelSnapshot> lintels, IEnumerable<ManagerWallSnapshot> walls,
        IEnumerable<ManagerStructureSnapshot> structures, int unreadableCount,
        IEnumerable<OpeningLintelMatch> matches, IEnumerable<AboveOpeningFacts> above,
        IEnumerable<OpeningBearingFacts> sides, IDictionary<string, LintelCalculation> calculations)
        : this(openings, lintels, walls, structures, unreadableCount, matches, above, sides) {
        Calculations = new ReadOnlyDictionary<string, LintelCalculation>(
            new Dictionary<string, LintelCalculation>(calculations));
    }

    /// <summary>Для снимков без результатов поиска проецирует исходные одиночные строки.</summary>
    public IEnumerable<OpeningLintelMatch> GetRows() {
        return Matches ?? Openings.Select(o => new OpeningLintelMatch(o, null, MatchState.Incomplete, []))
            .Concat(Lintels.Select(l => new OpeningLintelMatch(null, l, MatchState.Incomplete, [])));
    }
}

/// <summary>Общие факты экземпляра семейства; числовой ID хранится в Information.</summary>
internal sealed class ManagerFamilySnapshot(string uniqueId, string familyUniqueId, string typeUniqueId,
    string typeName, ManagerElementInfo information) {
    public string UniqueId { get; } = uniqueId;
    public string FamilyUniqueId { get; } = familyUniqueId;
    public string TypeUniqueId { get; } = typeUniqueId;
    public string TypeName { get; } = typeName;
    public ManagerElementInfo Information { get; } = information;
}

/// <summary>Параметры проёма в мм; значение высоты сохраняет семантику настроенного параметра.</summary>
internal sealed class ManagerOpeningSnapshot(ManagerFamilySnapshot element, string hostWallUniqueId,
    double? widthMm, double? heightMm, GeometryFacts geometry = null) {
    /// <summary>Добавляет идентификатор уровня исходного проёма.</summary>
    public ManagerOpeningSnapshot(ManagerFamilySnapshot element, string hostWallUniqueId, double? widthMm,
        double? heightMm, GeometryFacts geometry, string levelUniqueId)
        : this(element, hostWallUniqueId, widthMm, heightMm, geometry) {
        LevelUniqueId = levelUniqueId;
    }

    /// <summary>Сохраняет отдельно стену, в которой подтверждён вырез, и хост экземпляра Revit.</summary>
    public ManagerOpeningSnapshot(ManagerFamilySnapshot element, string hostWallUniqueId, double? widthMm,
        double? heightMm, GeometryFacts geometry, string levelUniqueId, string cutWallUniqueId)
        : this(element, hostWallUniqueId, widthMm, heightMm, geometry, levelUniqueId) {
        CutWallUniqueId = cutWallUniqueId ?? hostWallUniqueId;
    }

    public string LevelUniqueId { get; }
    public GeometryFacts Geometry { get; } = geometry;
    public ManagerFamilySnapshot Element { get; } = element;
    public string HostWallUniqueId { get; } = hostWallUniqueId;
    public string CutWallUniqueId { get; } = hostWallUniqueId;
    public double? WidthMm { get; } = widthMm;
    public double? HeightMm { get; } = heightMm;
}

/// <summary>Значения настроенных параметров перемычки; опирание прочитано из параметров.</summary>
internal sealed class ManagerLintelSnapshot(ManagerFamilySnapshot element, double? openingWidthMm,
    double? wallThicknessMm, double? leftBearingMm, double? rightBearingMm,
    int? leftCorner, int? rightCorner, int? leftWelding, int? rightWelding, GeometryFacts geometry = null) {
    /// <summary>Сохраняет идентичность уровня и стадии создания для сравнения расчёта.</summary>
    public ManagerLintelSnapshot(ManagerFamilySnapshot element, double? openingWidthMm, double? wallThicknessMm,
        double? leftBearingMm, double? rightBearingMm, int? leftCorner, int? rightCorner, int? leftWelding,
        int? rightWelding, GeometryFacts geometry, string levelUniqueId, string createdPhaseUniqueId)
        : this(element, openingWidthMm, wallThicknessMm, leftBearingMm, rightBearingMm, leftCorner, rightCorner,
            leftWelding, rightWelding, geometry) {
        LevelUniqueId = levelUniqueId;
        CreatedPhaseUniqueId = createdPhaseUniqueId;
    }

    public string LevelUniqueId { get; }
    public string CreatedPhaseUniqueId { get; }
    public GeometryFacts Geometry { get; } = geometry;
    public ManagerFamilySnapshot Element { get; } = element;
    public double? OpeningWidthMm { get; } = openingWidthMm;
    public double? WallThicknessMm { get; } = wallThicknessMm;
    public double? LeftBearingMm { get; } = leftBearingMm;
    public double? RightBearingMm { get; } = rightBearingMm;
    public int? LeftCorner { get; } = leftCorner;
    public int? RightCorner { get; } = rightCorner;
    public int? LeftWelding { get; } = leftWelding;
    public int? RightWelding { get; } = rightWelding;
}

/// <summary>Ключи стены и толщина из свойств экземпляра.</summary>
internal sealed class ManagerWallSnapshot(string uniqueId, long elementId, string typeUniqueId, double? widthMm,
    bool isReinforcedConcrete, ManagerElementPhases phases) {
    public string UniqueId { get; } = uniqueId;
    public long ElementId { get; } = elementId;
    public string TypeUniqueId { get; } = typeUniqueId;
    public double? WidthMm { get; } = widthMm;
    public bool IsReinforcedConcrete { get; } = isReinforcedConcrete;
    public ManagerElementPhases Phases { get; } = phases;
}

/// <summary>Конструкция идентифицированного типа из одного из двух списков настроек.</summary>
internal sealed class ManagerStructureSnapshot(string uniqueId, long elementId, string typeUniqueId,
    string category, bool isSteel, ManagerElementPhases phases) {
    public string UniqueId { get; } = uniqueId;
    public long ElementId { get; } = elementId;
    public string TypeUniqueId { get; } = typeUniqueId;
    public string Category { get; } = category;
    public bool IsSteel { get; } = isSteel;
    public ManagerElementPhases Phases { get; } = phases;
}

/// <summary>Стадии элемента; пустая строка означает явно неназначенную стадию.</summary>
internal sealed class ManagerElementPhases(string createdName, string demolishedName,
    long? createdId = null, long? demolishedId = null) {
    public string CreatedName { get; } = createdName;
    public string DemolishedName { get; } = demolishedName;
    public long? CreatedId { get; } = createdId;
    public long? DemolishedId { get; } = demolishedId;

    /// <summary>Сравнивает идентичность и отображаемые имена стадий.</summary>
    public bool HasSameValues(ManagerElementPhases other) {
        return other is not null && CreatedId == other.CreatedId && DemolishedId == other.DemolishedId
            && CreatedName == other.CreatedName && DemolishedName == other.DemolishedName;
    }
}
