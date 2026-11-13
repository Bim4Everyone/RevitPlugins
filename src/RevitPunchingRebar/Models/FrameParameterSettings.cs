using System.Collections.Generic;

namespace RevitPunchingRebar.Models;

/// <summary>Сохраняемое имя параметра. Для копирования имя одинаково у плиты и каркаса.</summary>
internal sealed class FrameParameterBinding {
    public string TargetName { get; set; }
}

/// <summary>Соответствия параметров текущего проекта. Отсутствующие роли получают значения по умолчанию.</summary>
internal sealed class FrameParameterSettings {
    public Dictionary<string, FrameParameterBinding> Bindings { get; set; } = [];

    public FrameParameterBinding GetBinding(FrameParameterRole role) => Bindings[role.ToString()];
    public string GetName(FrameParameterRole role) => GetBinding(role).TargetName;

    public static FrameParameterSettings CopyWithDefaults(FrameParameterSettings source) {
        FrameParameterSettings result = new FrameParameterSettings();
        foreach(FrameParameterDefinition definition in FrameParameterCatalog.Definitions) {
            FrameParameterBinding binding = null;
            source?.Bindings?.TryGetValue(definition.Role.ToString(), out binding);
            result.Bindings[definition.Role.ToString()] = new FrameParameterBinding {
                TargetName = binding?.TargetName ?? definition.DefaultName
            };
        }

        return result;
    }
}
