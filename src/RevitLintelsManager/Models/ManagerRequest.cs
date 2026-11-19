using System;

namespace RevitLintelsManager.Models;

/// <summary>Неизменяемый запрос чтения UI-контекста исходного документа.</summary>
internal sealed class ManagerRequest(Guid documentToken, long generation) {
    public Guid DocumentToken { get; } = documentToken;
    public long Generation { get; } = generation;
}
