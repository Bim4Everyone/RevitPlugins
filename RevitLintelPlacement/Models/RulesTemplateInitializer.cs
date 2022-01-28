using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitLintelPlacement.Models {
    internal class RulesTemplateInitializer {

        //TODO: уточнить все названия (материалов, классов материалов, типов стен)
        public RulesSettigs GetTemplateRules() {
            return new RulesSettigs() {
                IsSystem = true,
                RuleSettings = new List<RuleSetting>() {
                    InitializeRuleToOpeningInBrickWall200_1100(),
                    InitializeRuleToOpeningInNotBrickWall400_1100(),
                    InitializeRuleToOpening1100_2500()
                }
            };
        }

        #region Common Conditions and LintelParameters

        private IEnumerable<LintelParameterSetting> InitializeCommonLintelParametrs() {

            yield return new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.NumberParameter,
                Name = "ОпираниеСлева",
                NumberValue = 250
            };
            yield return new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.NumberParameter,
                Name = "ОпираниеСправа",
                NumberValue = 250
            };
            yield return new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.RelativeWallParameter,
                Name = "Половина толщины стены",
                RelationValue = 0.5,
                WallParameterName = "Толщина"
            };
            yield return new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.ReletiveOpeneingParameter,
                Name = "ЭЛМТ_ширина проема",
                RelationValue = 1,
                OpeninigParameterName = "Ширина"
            };
        }

        private IEnumerable<ConditionSetting> InitializeCommonConditions() {

            yield return new ConditionSetting() {
                ConditionType = ConditionType.ExclusionWallTypes,
                ExclusionWallTypes = new List<string> { "Невозводим" },
            };
            yield return new ConditionSetting() {
                ConditionType = ConditionType.WallMaterialClasses,
                WallMaterialClasses = new List<string>() { "Кладка" },
            };
        }

        #endregion

        #region Conditions
        private IEnumerable<ConditionSetting> InitializeBrickWallMaterialCondition() {
            yield return new ConditionSetting() {
                ConditionType = ConditionType.WallMaterials,
                WallMaterials = new List<string>() { "Кирпич" },
            };
        }


        private IEnumerable<ConditionSetting> InitializeNotBrickWallMaterialsCondition() {
            yield return new ConditionSetting() {
                ConditionType = ConditionType.ExclusionWallTypes,
                ExclusionWallTypes = new List<string>() { "Кирпич", "Невозводим" },
            };
        }
        //private IEnumerable<ConditionSetting> InitializeNotBrickWallMaterialsCondition() {
        //    yield return new ConditionSetting() {
        //        ConditionType = ConditionType.WallMaterials,
        //        WallMaterials = new List<string>() { "Газобетон", "Пазогребнев", "СПП" },
        //    };
        //}

        private IEnumerable<ConditionSetting> InitializeConditionToOpening400_1100() {
            yield return new ConditionSetting() {
                ConditionType = ConditionType.OpeningWidth,
                OpeningWidthMin = 400,
                OpeningWidthMax = 1100,
            };
        }

        private IEnumerable<ConditionSetting> InitializeConditionToOpening200_1100() {
            yield return new ConditionSetting() {
                ConditionType = ConditionType.OpeningWidth,
                OpeningWidthMin = 200,
                OpeningWidthMax = 1100,
            };
        }

        private IEnumerable<ConditionSetting> InitializeConditionToOpening1100_2500() {
            yield return new ConditionSetting() {
                ConditionType = ConditionType.OpeningWidth,
                OpeningWidthMin = 1100,
                OpeningWidthMax = 2500,
            };
        }

        #endregion

        #region LintelParameters
        private IEnumerable<LintelParameterSetting> InitializeLintelParametersToOpeninig200_1100() {
            yield return new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.YesNoLintelParameter,
                Name = "Уголок",
                IsChecked = false
            };
        }

        private IEnumerable<LintelParameterSetting> InitializeLintelParametersToOpeninig1100_2500() {
            yield return new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.YesNoLintelParameter,
                Name = "Уголок",
                IsChecked = true
            };
        }

        #endregion

        #region Initializing Rules

        private RuleSetting InitializeRuleToOpeningInBrickWall200_1100() {
            return new RuleSetting() {
                Name = "АТР_Проем_в_кирпичной_стене_200_1100",
                ConditionSettingsConfig = new List<ConditionSetting>(InitializeCommonConditions()
                    .Union(InitializeConditionToOpening200_1100())),
                    //.Union(InitializeBrickWallMaterialCondition())),
                LintelParameterSettingsConfig = new List<LintelParameterSetting>(InitializeCommonLintelParametrs()
                    .Union(InitializeLintelParametersToOpeninig200_1100()))
            };

        }

        private RuleSetting InitializeRuleToOpeningInNotBrickWall400_1100() {
            return new RuleSetting() { 
                Name = "АТР_Проем_в_некирпичной_стене_400_1100",
                ConditionSettingsConfig = new List<ConditionSetting>(InitializeCommonConditions()
                    .Union(InitializeConditionToOpening400_1100())
                    .Union(InitializeNotBrickWallMaterialsCondition())),
                LintelParameterSettingsConfig = new List<LintelParameterSetting>(InitializeCommonLintelParametrs()
                    .Union(InitializeLintelParametersToOpeninig200_1100()))
            };
        }

        private RuleSetting InitializeRuleToOpening1100_2500() {
            return new RuleSetting() {
                Name = "АТР_Проем_в_стене_1100_2500",
                ConditionSettingsConfig = new List<ConditionSetting>(InitializeCommonConditions()
                   .Union(InitializeConditionToOpening1100_2500())),
                LintelParameterSettingsConfig = new List<LintelParameterSetting>(InitializeCommonLintelParametrs()
                   .Union(InitializeLintelParametersToOpeninig1100_2500()))
            };
        }

        #endregion

    }
}
