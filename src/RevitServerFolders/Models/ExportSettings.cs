using pyRevitLabs.Json;

namespace RevitServerFolders.Models;

internal abstract class ExportSettings {
    [JsonIgnore]
    public int Index { get; set; }
    public string TargetFolder { get; set; }
    public string SourceFolder { get; set; }
    public bool ClearTargetFolder { get; set; } = false;
    public bool OpenTargetWhenFinish { get; set; } = true;
    public string[] SkippedObjects { get; set; }
    public bool IsSelected { get; set; } = true;
}
