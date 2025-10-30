namespace RevitSetCoordParams.Models;
internal class WarningDescription {
    public string Name { get; set; }
    public string Description { get; set; }

    public static WarningDescription NotFound { get; } = new WarningDescription {
        Name = "Not Found",
        Description = "The requested item was not found."
    };

    public static WarningDescription NotFoundParam { get; } = new WarningDescription {
        Name = "Not Found Parameter",
        Description = "The specified parameter was not found."
    };

    public static WarningDescription Skip { get; } = new WarningDescription {
        Name = "Blocked",
        Description = "The element was blocked"
    };
}
