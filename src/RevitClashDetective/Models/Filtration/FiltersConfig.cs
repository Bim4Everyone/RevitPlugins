using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models.Filtration;
internal class FiltersConfig : ProjectConfig {
    public string RevitVersion { get; set; }

    /// <summary>
    /// Устаревшие поисковые наборы. Читаются только для миграции конфигов,
    /// сохраненных версиями плагина до перехода на Bim4Everyone.RevitFiltration.
    /// <para/>
    /// При сохранении сюда записывается заглушка <see cref="LegacyConfigPoison"/>,
    /// чтобы старые версии плагина не смогли открыть и перезаписать новый конфиг.
    /// </summary>
    [JsonConverter(typeof(LegacyFiltersJsonConverter))]
    public List<Filter> Filters { get; set; } = [];

    /// <summary>
    /// Поисковые наборы на основе Bim4Everyone.RevitFiltration
    /// </summary>
    public List<FilterSettings> FilterSettings { get; set; } = [];

    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public static FiltersConfig GetFiltersConfig(string revitFileName, Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : new ProjectConfigBuilder()
            .SetSerializer(new RevitClashConfigSerializer(new RevitClashesSerializationBinder(), document))
            .SetPluginName(nameof(RevitClashDetective))
            .SetRelativePath(revitFileName)
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(FiltersConfig) + ".json")
            .Build<FiltersConfig>();
    }
}
