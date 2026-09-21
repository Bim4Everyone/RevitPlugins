using System;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitParamsChecker.Models.Results;

internal class ReportExportConfig : ProjectConfig {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    /// <summary>
    /// Путь к последней директории сохранения отчета
    /// </summary>
    public string DirPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

    public static ReportExportConfig GetConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitParamsChecker))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(ReportExportConfig) + ".json")
            .Build<ReportExportConfig>();
    }
}
