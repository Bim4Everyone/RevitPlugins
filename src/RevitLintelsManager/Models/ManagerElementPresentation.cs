using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Независимые сведения элемента, необходимые для отображения строки.</summary>
internal sealed class ManagerElementPresentation(string typeName, ManagerElementStatus status,
    ManagerElementInfo information = null, IEnumerable<ManagerDiagnostic> diagnostics = null) {
    public string TypeName { get; } = typeName;
    public ManagerElementStatus Status { get; } = status;
    public ManagerElementInfo Information { get; } = information;
    public IReadOnlyList<ManagerDiagnostic> Diagnostics { get; } =
        (diagnostics ?? Enumerable.Empty<ManagerDiagnostic>()).ToList().AsReadOnly();
}

/// <summary>Подготовленные сведения; DisplayId позволяет явно показать внешний ID без числового ID Revit.</summary>
/// <remarks>Null означает неизвестное, пустая группа — отсутствие группы.</remarks>
internal sealed class ManagerElementInfo(string familyName, string worksetName, string levelName,
    string groupName, bool? isFixed, long? elementId, string displayId = null) {
    public string FamilyName { get; } = familyName;
    public string WorksetName { get; } = worksetName;
    public string LevelName { get; } = levelName;
    public string GroupName { get; } = groupName;
    public bool? IsFixed { get; } = isFixed;
    public long? ElementId { get; } = elementId;
    public string DisplayId { get; } = displayId;
}
