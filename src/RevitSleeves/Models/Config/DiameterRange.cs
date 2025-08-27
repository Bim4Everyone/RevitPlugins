using pyRevitLabs.Json;

namespace RevitSleeves.Models.Config;
internal class DiameterRange {
    [JsonConstructor]
    public DiameterRange() { }


    public double StartMepSize { get; set; }

    public double EndMepSize { get; set; }

    public double SleeveDiameter { get; set; }

    public double SleeveThickness { get; set; }
}
