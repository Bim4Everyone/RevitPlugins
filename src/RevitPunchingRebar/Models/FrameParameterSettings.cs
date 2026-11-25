using System.Collections.Generic;

namespace RevitPunchingRebar.Models;

/// <summary>
/// Соответствия параметров текущего проекта. Отсутствующие роли получают значения по умолчанию.
/// </summary>
internal sealed class FrameParameterSettings {
    public Dictionary<string, FrameParameterBinding> Bindings { get; set; } = [];

    public FrameParameterBinding GetBinding(FrameParameterRole role) {
        return Bindings[role.ToString()];
    }

    public string GetName(FrameParameterRole role) {
        return GetBinding(role).TargetName;
    }

    public static FrameParameterSettings CopyWithDefaults(FrameParameterSettings source) {
        var result = new FrameParameterSettings();
        foreach(FrameParameterDefinition definition in FrameParameterCatalog.Definitions) {
            FrameParameterBinding binding = null;
            source?.Bindings?.TryGetValue(definition.Role.ToString(), out binding);
            result.Bindings[definition.Role.ToString()] = new FrameParameterBinding {
                TargetName = binding?.TargetName ?? definition.DefaultName,
                YesNoValue = binding?.YesNoValue ?? definition.DefaultYesNoValue
            };
        }

        return result;
    }
}
