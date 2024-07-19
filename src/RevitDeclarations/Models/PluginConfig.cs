using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
    internal class PluginConfig : ProjectConfig<RevitSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static PluginConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitDeclarations))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(PluginConfig) + ".json")
                .Build<PluginConfig>();
        }
    }

    // Настройки Accuracy и LoadUtp не сохраняются в конфиг
    internal class RevitSettings : ProjectSettings {
        public override string ProjectName { get; set; }
        public string DeclarationName { get; set; }
        public string DeclarationPath { get; set; }
        public bool ExportToExcel { get; set; }
        public string Phase { get; set; }
        public List<string> RevitDocuments { get; set; } = new List<string>();

        public string FilterRoomsParam { get; set; }
        public string FilterRoomsValue { get; set; }
        public string GroupingBySectionParam { get; set; }
        public string GroupingByGroupParam { get; set; }
        public string MultiStoreyParam { get; set; }

        public string ApartmentFullNumberParam { get; set; }
        public string DepartmentParam { get; set; }
        public string LevelParam { get; set; }
        public string SectionParam { get; set; }
        public string BuildingParam { get; set; }
        public string ApartmentNumberParam { get; set; }
        public string ApartmentAreaParam { get; set; }
        public string ApartmentAreaCoefParam { get; set; }
        public string ApartmentAreaLivingParam { get; set; }
        public string RoomsAmountParam { get; set; }
        public string ProjectNameID { get; set; }
        public string ApartmentAreaNonSumParam { get; set; }
        public string RoomsHeightParam { get; set; }

        public string RoomAreaParam { get; set; }
        public string RoomAreaCoefParam { get; set; }

        public string PrioritiesFilePath { get; set; }


        public RevitSettings GetCompanyConfig() {
            return new RevitSettings() {
                FilterRoomsParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_DEPARTMENT),
                FilterRoomsValue = "квартира",
                GroupingBySectionParam = SharedParamsConfig.Instance.RoomSectionShortName.Name,
                GroupingByGroupParam = SharedParamsConfig.Instance.RoomGroupShortName.Name,                
                MultiStoreyParam = SharedParamsConfig.Instance.RoomMultilevelGroup.Name,

                ApartmentFullNumberParam = SharedParamsConfig.Instance.ApartmentNumber.Name,
                DepartmentParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_DEPARTMENT),
                LevelParam = SharedParamsConfig.Instance.Level.Name,
                SectionParam = SharedParamsConfig.Instance.RoomSectionShortName.Name,
                BuildingParam = SharedParamsConfig.Instance.RoomBuildingShortName.Name,
                ApartmentNumberParam = SharedParamsConfig.Instance.RoomGroupShortName.Name,
                ApartmentAreaParam = SharedParamsConfig.Instance.ApartmentArea.Name,
                ApartmentAreaCoefParam = SharedParamsConfig.Instance.ApartmentAreaRatio.Name,
                ApartmentAreaLivingParam = SharedParamsConfig.Instance.ApartmentLivingArea.Name,
                RoomsAmountParam = SharedParamsConfig.Instance.RoomsCount.Name,
                ProjectNameID = "",
                ApartmentAreaNonSumParam = SharedParamsConfig.Instance.ApartmentAreaNoBalcony.Name,
                RoomsHeightParam = LabelUtils.GetLabelFor(BuiltInParameter.ROOM_HEIGHT),

                RoomAreaParam = SharedParamsConfig.Instance.RoomArea.Name,
                RoomAreaCoefParam = SharedParamsConfig.Instance.RoomAreaWithRatio.Name
            };
        }
    }
}
