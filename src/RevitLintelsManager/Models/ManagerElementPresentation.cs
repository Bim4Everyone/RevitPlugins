namespace RevitLintelsManager.Models;

/// <summary>Независимые сведения элемента, необходимые для отображения строки.</summary>
internal sealed class ManagerElementPresentation(string typeName, ManagerElementStatus status) {
    public string TypeName { get; } = typeName;
    public ManagerElementStatus Status { get; } = status;
}
