using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Факты исходного документа; ключ документа и поколение хранятся в ответе на запрос.</summary>
internal sealed class ManagerProjectSnapshot(IEnumerable<ManagerOpeningSnapshot> openings,
    IEnumerable<ManagerLintelSnapshot> lintels, IEnumerable<ManagerWallSnapshot> walls,
    IEnumerable<ManagerStructureSnapshot> structures, int unreadableCount) {
    public IReadOnlyList<ManagerOpeningSnapshot> Openings { get; } = openings.ToList().AsReadOnly();
    public IReadOnlyList<ManagerLintelSnapshot> Lintels { get; } = lintels.ToList().AsReadOnly();
    public IReadOnlyList<ManagerWallSnapshot> Walls { get; } = walls.ToList().AsReadOnly();
    public IReadOnlyList<ManagerStructureSnapshot> Structures { get; } = structures.ToList().AsReadOnly();
    public int UnreadableCount { get; } = unreadableCount;
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
    double? widthMm, double? heightMm) {
    public ManagerFamilySnapshot Element { get; } = element;
    public string HostWallUniqueId { get; } = hostWallUniqueId;
    public double? WidthMm { get; } = widthMm;
    public double? HeightMm { get; } = heightMm;
}

/// <summary>Значения настроенных параметров перемычки; опирание прочитано из параметров.</summary>
internal sealed class ManagerLintelSnapshot(ManagerFamilySnapshot element, double? openingWidthMm,
    double? wallThicknessMm, double? leftBearingMm, double? rightBearingMm,
    int? leftCorner, int? rightCorner, int? leftWelding, int? rightWelding) {
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
}
