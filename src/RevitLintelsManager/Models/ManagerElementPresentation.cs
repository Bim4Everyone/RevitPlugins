using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Независимые сведения элемента, необходимые для отображения строки.</summary>
internal sealed class ManagerElementPresentation(string typeName, ManagerElementStatus status,
    ManagerElementInfo information = null, IEnumerable<ManagerDiagnostic> diagnostics = null,
    double? widthMm = null, double? heightMm = null, double? thicknessMm = null) {
    public string TypeName { get; } = typeName;
    public ManagerElementStatus Status { get; } = status;
    public ManagerElementInfo Information { get; } = information;
    public double? WidthMm { get; } = widthMm;
    public double? HeightMm { get; } = heightMm;
    public double? ThicknessMm { get; } = thicknessMm;
    public IReadOnlyList<ManagerDiagnostic> Diagnostics { get; } =
        (diagnostics ?? Enumerable.Empty<ManagerDiagnostic>()).ToList().AsReadOnly();
}

/// <summary>Подготовленные сведения; DisplayId позволяет явно показать внешний ID без числового ID Revit.</summary>
/// <remarks>Принадлежность к группе может быть известна отдельно от её имени.</remarks>
internal sealed class ManagerElementInfo(string familyName, string worksetName, string levelName,
    string groupName, bool? isFixed, long? elementId, string displayId = null, ManagerElementPhases phases = null,
    bool? isInGroup = null) {
    public string FamilyName { get; } = familyName;
    public string WorksetName { get; } = worksetName;
    public string LevelName { get; } = levelName;
    public string GroupName { get; } = groupName;
    public bool? IsInGroup { get; } = isInGroup;
    public bool? IsFixed { get; } = isFixed;
    public long? ElementId { get; } = elementId;
    public string DisplayId { get; } = displayId;
    public ManagerElementPhases Phases { get; } = phases;
}
