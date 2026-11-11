using dosymep.Bim4Everyone;

namespace RevitEnumerateBySpline.Models.Settings;

public class ConfigSettings {
    public RevitParam? NumberParam { get; set; }
    public double? StartNumber { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public RevitParam? DependentSearchParam { get; set; }
    public string? SearchKey { get; set; }
    public RevitParam? DependentParam { get; set; }
}
