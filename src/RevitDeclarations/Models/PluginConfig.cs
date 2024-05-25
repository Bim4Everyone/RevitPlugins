using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
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

    internal class RevitSettings : ProjectSettings {
        public override string ProjectName { get; set; }
        public string DeclarationName { get; set; }
        public string DeclarationPath { get; set; }
        public string Phase { get; set; }
        public string Accuracy { get; set; }

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

        public RevitSettings GetCompanyConfig() {
            return new RevitSettings() {
                FilterRoomsParam = "Назначение",
                FilterRoomsValue = "Квартира",
                GroupingBySectionParam = "ФОП_ПМЩ_Секция",
                GroupingByGroupParam = "ФОП_ПМЩ_Группа",
                MultiStoreyParam = "Комментарии",

                ApartmentFullNumberParam = "ФОП_Номер квартиры",
                DepartmentParam = "Назначение",
                LevelParam = "ФОП_Этаж",
                SectionParam = "ФОП_ПМЩ_Секция",
                BuildingParam = "ФОП_ПМЩ_Секция",
                ApartmentNumberParam = "ФОП_ПМЩ_Группа",
                ApartmentAreaParam = "ФОП_КВР_Площадь без коэф.",
                ApartmentAreaCoefParam = "ФОП_КВР_Площадь с коэф.",
                ApartmentAreaLivingParam = "ФОП_КВР_Площадь жилая",
                RoomsAmountParam = "ФОП_Количество комнат",
                ProjectNameID = "Тест 1",
                ApartmentAreaNonSumParam = "ФОП_КВР_Площадь без ЛП",
                RoomsHeightParam = "Полная высота",

                RoomAreaParam = "ФОП_ПМЩ_Площадь",
                RoomAreaCoefParam = "ФОП_ПМЩ_Площадь с коэф."
            };
        }
    }
}
