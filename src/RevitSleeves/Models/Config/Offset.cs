using pyRevitLabs.Json;

namespace RevitSleeves.Models.Config;
internal class Offset {
    [JsonConstructor]
    public Offset() { }


    public OffsetType OffsetType { get; set; }

    public double Value { get; set; }
}
