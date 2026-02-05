using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using RevitUnmodelingMep.ViewModels;

namespace RevitUnmodelingMep.Models;

internal static class UnmodelingConfigReader {
    public const string UnmodelingConfigKey = "UNMODELING_CONFIG";

    public static IReadOnlyList<ConsumableTypeItem> LoadUnmodelingConfigs(
        VisSettingsStorage settingsUpdater,
        Func<string, CategoryOption> resolveCategoryOption,
        out int lastConfigIndex) {

        if(settingsUpdater is null) {
            throw new ArgumentNullException(nameof(settingsUpdater));
        }

        UnmodelingSettingsDocument settings = settingsUpdater.GetUnmodelingSettings();
        return GetConsumableItems(settings?.UnmodelingConfig, resolveCategoryOption, out lastConfigIndex);
    }

    public static IReadOnlyList<ConsumableTypeItem> GetConsumableItems(
        IReadOnlyDictionary<string, UnmodelingConfigItem> settings,
        Func<string, CategoryOption> resolveCategoryOption,
        out int lastConfigIndex) {

        lastConfigIndex = 0;
        var result = new List<ConsumableTypeItem>();
        if(settings == null) {
            return result;
        }

        foreach(var config in settings) {
            lastConfigIndex = UpdateConfigIndex(lastConfigIndex, config.Key);

            ConsumableTypeItem item = ConsumableTypeItem.FromConfig(config.Key, config.Value);
            if(resolveCategoryOption != null) {
                item.SelectedCategory = resolveCategoryOption(item.CategoryId);
            }

            result.Add(item);
        }

        return result;
    }

    private static int UpdateConfigIndex(int lastConfigIndex, string configKey) {
        Match match = Regex.Match(configKey ?? string.Empty, @"config_(\d+)", RegexOptions.IgnoreCase);
        if(match.Success && int.TryParse(match.Groups[1].Value, out int value) && value > lastConfigIndex) {
            return value;
        }

        return lastConfigIndex;
    }
}
