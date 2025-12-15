using System;
using System.Collections.Generic;

using dosymep.Bim4Everyone.ProjectConfigs;

namespace RevitDeclarations.Models;
internal abstract class DeclarationConfigSettings : ProjectSettings {
    public override string ProjectName { get; set; }

    public string DeclarationName { get; set; }
    public string DeclarationPath { get; set; }
    public Guid ExportFormat { get; set; }
    public string Phase { get; set; }
    public List<string> RevitDocuments { get; set; } = [];

    public string FilterRoomsParam { get; set; }
    public string[] FilterRoomsValues { get; set; }
    public string GroupingBySectionParam { get; set; }
    public string GroupingByGroupParam { get; set; }
    public string MultiStoreyParam { get; set; }

    public string DepartmentParam { get; set; }
    public string LevelParam { get; set; }
    public string SectionParam { get; set; }
    public string BuildingParam { get; set; }
    public string RoomNameParam { get; set; }
    public string RoomNumberParam { get; set; }
    public string RoomAreaParam { get; set; }
    public string ApartmentAreaParam { get; set; }
    public string ApartmentNumberParam { get; set; }
    public string ProjectNameID { get; set; }
}
