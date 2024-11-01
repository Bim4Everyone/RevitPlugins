using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
    internal class CommercialConfig : ProjectConfig<CommercialConfigSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static CommercialConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitDeclarations))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(CommercialConfig) + ".json")
                .Build<CommercialConfig>();
        }
    }

    internal class CommercialConfigSettings : DeclarationConfigSettings {
        public string BuildingNumberParam { get; set; }
        public string ConstrWorksNumberParam { get; set; }
        public bool AddPostfixToNumber { get; set; }
        public string RoomsHeightParam { get; set; }
        public string GroupNameParam { get; set; }

        public CommercialConfigSettings GetCompanyConfig() {
            return new CommercialConfigSettings() {
                FilterRoomsParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_DEPARTMENT),
                FilterRoomsValue = "Нежилое помещение,Машино-место,Кладовая",
                GroupingBySectionParam = SharedParamsConfig.Instance.RoomGroupShortName.Name,
                GroupingByGroupParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_NUMBER),
                MultiStoreyParam = SharedParamsConfig.Instance.RoomMultilevelGroup.Name,

                DepartmentParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_DEPARTMENT),
                LevelParam = SharedParamsConfig.Instance.Level.Name,
                SectionParam = SharedParamsConfig.Instance.RoomSectionShortName.Name,
                BuildingParam = SharedParamsConfig.Instance.RoomBuildingShortName.Name,
                ApartmentNumberParam = SharedParamsConfig.Instance.RoomGroupShortName.Name,
                ApartmentAreaParam = SharedParamsConfig.Instance.ApartmentArea.Name,
                RoomAreaParam = SharedParamsConfig.Instance.RoomArea.Name,
                ProjectNameID = "",

                RoomsHeightParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_HEIGHT),
                BuildingNumberParam = SharedParamsConfig.Instance.BuildingNumber.Name,
                ConstrWorksNumberParam = SharedParamsConfig.Instance.ConstructionWorksNumber.Name,
                AddPostfixToNumber = true,
                GroupNameParam = SharedParamsConfig.Instance.ApartmentGroupName.Name,
            };
        }
    }
}
