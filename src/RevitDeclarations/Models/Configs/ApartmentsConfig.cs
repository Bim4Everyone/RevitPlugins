using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models;
internal class ApartmentsConfig : ProjectConfig<ApartmnetsConfigSettings> {
    [JsonIgnore] 
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore] 
    public override IConfigSerializer Serializer { get; set; }

    public static ApartmentsConfig GetPluginConfig() {
        return new ProjectConfigBuilder()
            .SetSerializer(new ConfigSerializer())
            .SetPluginName(nameof(RevitDeclarations))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(ApartmentsConfig) + ".json")
            .Build<ApartmentsConfig>();
    }
}

internal class ApartmnetsConfigSettings : DeclarationConfigSettings {
    public string ApartmentFullNumberParam { get; set; }

    public string BuildingNumberParam { get; set; }
    public string ConstrWorksNumberParam { get; set; }
    public string ApartmentAreaCoefParam { get; set; }
    public string RoomsAmountParam { get; set; }
    public string ApartmentAreaLivingParam { get; set; }
    public string RoomsHeightParam { get; set; }
    public string ApartmentAreaNonSumParam { get; set; }
    public string RoomAreaCoefParam { get; set; }
    public string PrioritiesFilePath { get; set; }

    public ApartmnetsConfigSettings GetCompanyConfig() {
        return new ApartmnetsConfigSettings() {
            FilterRoomsParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_DEPARTMENT),
            FilterRoomsValues = new[] { "квартира" },
            GroupingBySectionParam = SharedParamsConfig.Instance.RoomSectionShortName.Name,
            GroupingByGroupParam = SharedParamsConfig.Instance.RoomGroupShortName.Name,
            MultiStoreyParam = SharedParamsConfig.Instance.RoomMultilevelGroup.Name,

            DepartmentParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_DEPARTMENT),
            LevelParam = SharedParamsConfig.Instance.Level.Name,
            SectionParam = SharedParamsConfig.Instance.RoomSectionShortName.Name,
            BuildingParam = SharedParamsConfig.Instance.RoomBuildingShortName.Name,
            BuildingNumberParam = SharedParamsConfig.Instance.BuildingNumber.Name,
            ConstrWorksNumberParam = SharedParamsConfig.Instance.ConstructionWorksNumber.Name,

            ApartmentNumberParam = SharedParamsConfig.Instance.RoomGroupShortName.Name,
            ApartmentFullNumberParam = SharedParamsConfig.Instance.ApartmentNumber.Name,
            ApartmentAreaParam = SharedParamsConfig.Instance.ApartmentArea.Name,
            ApartmentAreaCoefParam = SharedParamsConfig.Instance.ApartmentAreaRatio.Name,
            ApartmentAreaLivingParam = SharedParamsConfig.Instance.ApartmentLivingArea.Name,
            ApartmentAreaNonSumParam = SharedParamsConfig.Instance.ApartmentAreaNoBalcony.Name,
            RoomsAmountParam = SharedParamsConfig.Instance.RoomsCount.Name,
            RoomsHeightParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_HEIGHT),

            ProjectNameID = "",

            RoomAreaParam = SharedParamsConfig.Instance.RoomArea.Name,
            RoomAreaCoefParam = SharedParamsConfig.Instance.RoomAreaWithRatio.Name,
            RoomNameParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NAME),
            RoomNumberParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NUMBER)
        };
    }
}
