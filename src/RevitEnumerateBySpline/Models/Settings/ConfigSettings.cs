using System.Collections.ObjectModel;

using dosymep.Bim4Everyone;

using RevitEnumerateBySpline.ViewModels;

namespace RevitEnumerateBySpline.Models.Settings;

public class ConfigSettings {
    public double? StartNumber { get; set; }
    public string? Prefix { get; set; }
    public string? Suffix { get; set; }
    public string? SearchKey { get; set; }
    public RevitParam? DependentSearchParam { get; set; }
    public RevitParam? DependentParam { get; set; }
}
