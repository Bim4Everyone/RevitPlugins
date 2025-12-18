using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

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

        JObject settings = settingsUpdater.GetUnmodelingConfig();
        return GetConsumableItems(settings, resolveCategoryOption, out lastConfigIndex);
    }

    public static IReadOnlyList<ConsumableTypeItem> GetConsumableItems(
        JObject settings,
        Func<string, CategoryOption> resolveCategoryOption,
        out int lastConfigIndex) {

        lastConfigIndex = 0;
        var result = new List<ConsumableTypeItem>();
        if(settings == null) {
            return result;
        }

        if(settings.TryGetValue(UnmodelingConfigKey, out JToken configToken)
           && configToken is JObject configObj) {
            foreach(JProperty property in configObj.Properties()) {
                lastConfigIndex = UpdateConfigIndex(lastConfigIndex, property.Name);

                ConsumableTypeItem item = ConsumableTypeItem.FromConfig(property);
                if(resolveCategoryOption != null) {
                    item.SelectedCategory = resolveCategoryOption(item.CategoryId);
                }

                result.Add(item);
            }
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
