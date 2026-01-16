using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models;
internal class PublicAreasConfig : ProjectConfig<PublicAreasConfigSettings> {
    [JsonIgnore] 
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore] 
    public override IConfigSerializer Serializer { get; set; }

    public static PublicAreasConfig GetPluginConfig() {
        return new ProjectConfigBuilder()
            .SetSerializer(new ConfigSerializer())
            .SetPluginName(nameof(RevitDeclarations))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PublicAreasConfig) + ".json")
            .Build<PublicAreasConfig>();
    }
}

internal class PublicAreasConfigSettings : DeclarationConfigSettings {
    public bool AddPrefixToNumber { get; set; }

    public PublicAreasConfigSettings GetCompanyConfig() {
        return new PublicAreasConfigSettings() {
            FilterRoomsParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_DEPARTMENT),
            FilterRoomsValues = new[] { "общественное", "техническое" },
            GroupingBySectionParam = SharedParamsConfig.Instance.RoomGroupShortName.Name,
            GroupingByGroupParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NUMBER),
            MultiStoreyParam = SharedParamsConfig.Instance.RoomMultilevelGroup.Name,

            DepartmentParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_DEPARTMENT),
            LevelParam = SharedParamsConfig.Instance.Level.Name,
            SectionParam = SharedParamsConfig.Instance.RoomSectionShortName.Name,
            BuildingParam = SharedParamsConfig.Instance.RoomBuildingShortName.Name,

            ApartmentAreaParam = SharedParamsConfig.Instance.ApartmentArea.Name,
            ApartmentNumberParam = SharedParamsConfig.Instance.RoomGroupShortName.Name,

            AddPrefixToNumber = true,
            ProjectNameID = "",

            RoomAreaParam = SharedParamsConfig.Instance.RoomArea.Name,
            RoomNameParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NAME),
            RoomNumberParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NUMBER),
        };
    }
}
