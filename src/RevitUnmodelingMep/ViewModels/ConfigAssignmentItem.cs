using System.Linq;

using Newtonsoft.Json.Linq;

using dosymep.WPF.ViewModels;

namespace RevitUnmodelingMep.ViewModels;

internal class ConfigAssignmentItem : BaseViewModel {
    private readonly ConsumableTypeItem _config;
    private readonly int _systemTypeId;
    private bool _isChecked;

    public ConfigAssignmentItem(ConsumableTypeItem config, int systemTypeId) {
        _config = config;
        _systemTypeId = systemTypeId;

        _isChecked = HasAssignment(config, systemTypeId);
    }

    public string Name => _config?.Name;

    public bool IsChecked {
        get => _isChecked;
        set {
            bool changed = _isChecked != value;
            RaiseAndSetIfChanged(ref _isChecked, value);

            if(!changed) {
                return;
            }

            if(value) {
                AddAssignment(_config, _systemTypeId);
            } else {
                RemoveAssignment(_config, _systemTypeId);
            }
        }
    }

    private static bool HasAssignment(ConsumableTypeItem config, int systemTypeId) {
        if(config?.AssignedElementIds == null) {
            return false;
        }

        return config.AssignedElementIds
            .Select(token => token.Type switch {
                JTokenType.Integer => (int?) token,
                JTokenType.String => int.TryParse(token.ToString(), out int val) ? val : (int?) null,
                _ => null
            })
            .Any(val => val == systemTypeId);
    }

    private static void AddAssignment(ConsumableTypeItem config, int systemTypeId) {
        if(config.AssignedElementIds == null) {
            config.AssignedElementIds = new JArray();
        }

        if(!HasAssignment(config, systemTypeId)) {
            config.AssignedElementIds.Add(systemTypeId);
        }
    }

    private static void RemoveAssignment(ConsumableTypeItem config, int systemTypeId) {
        if(config.AssignedElementIds == null) {
            return;
        }

        JToken tokenToRemove = config.AssignedElementIds
            .FirstOrDefault(token => {
                int? val = token.Type switch {
                    JTokenType.Integer => (int?) token,
                    JTokenType.String => int.TryParse(token.ToString(), out int parsed) ? parsed : (int?) null,
                    _ => null
                };
                return val == systemTypeId;
            });

        tokenToRemove?.Remove();
    }
}
