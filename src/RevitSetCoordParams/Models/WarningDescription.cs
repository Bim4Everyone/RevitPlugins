using System;


namespace RevitSetCoordParams.Models;
internal class WarningDescription {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public static WarningDescription NotFound { get; } = new WarningDescription {
        Id = Guid.NewGuid(),
        Name = "Not Found",
        Description = "The requested item was not found."
    };

    public static WarningDescription NotFoundParam { get; } = new WarningDescription {
        Id = Guid.NewGuid(),
        Name = "Not Found Parameter",
        Description = "The specified parameter was not found."
    };

    public static WarningDescription Blocked { get; } = new WarningDescription {
        Id = Guid.NewGuid(),
        Name = "Blocked",
        Description = "The element was blocked"
    };
}
