using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitLintelPlacement.Models {
    internal class RulesTemplateInitializer {

        //TODO: уточнить все названия (материалов, классов материалов, типов стен)
        public List<RuleSettingsConfig> GetTemplateRules() {
            return new List<RuleSettingsConfig>() {
                InitializeRuleToOpeningInBrickWall200_1100(),
                InitializeRuleToOpeningInNotBrickWall400_1100(),
                InitializeRuleToOpening1100_2500()
            };
        }

        #region Common Conditions and LintelParameters

        private List<LintelParameterSettingConfig> InitializeCommonLintelParametrs() {

            var leftOffsetParameter = new LintelParameterSettingConfig() {
                LintelParameterType = LintelParameterType.NumberParameter,
                Name = "Смещение_слева",
                NumberValue = 250
            };
            var rightOffsetParameter = new LintelParameterSettingConfig() {
                LintelParameterType = LintelParameterType.NumberParameter,
                Name = "Смещение_справа",
                NumberValue = 250
            };
            var thicknessParameter = new LintelParameterSettingConfig() {
                LintelParameterType = LintelParameterType.RelativeWallParameter,
                Name = "Половина толщины стены",
                RelationValue = 0.5,
                WallParameterName = "Толщина"
            };
            var widthParameter = new LintelParameterSettingConfig() {
                LintelParameterType = LintelParameterType.ReletiveOpeneingParameter,
                Name = "ЭЛМТ_ширина проема",
                RelationValue = 1,
                OpeninigParameterName = "Ширина"
            };
            return new List<LintelParameterSettingConfig> {
                leftOffsetParameter,
                rightOffsetParameter,
                thicknessParameter,
                widthParameter
            };
        }

        private List<ConditionSettingConfig> InitializeCommonConditions() {

            var wallExclusionTypeCondition = new ConditionSettingConfig() {
                ConditionType = ConditionType.ExclusionWallTypes,
                ExclusionWallTypes = new List<string> { "Невозводимые" },
            };
            var materialClasses = new ConditionSettingConfig() {
                ConditionType = ConditionType.WallMaterialClasses,
                WallMaterialClasses = new List<string>() { "Кладка" },
            };
            return new List<ConditionSettingConfig>() { wallExclusionTypeCondition, materialClasses };
        }

        #endregion

        #region Conditions
        private List<ConditionSettingConfig> InitializeBrickWallMaterialCondition() {
            var brickCondition = new ConditionSettingConfig() {
                ConditionType = ConditionType.WallMaterialClasses,
                WallMaterialClasses = new List<string>() { "Кирпич" },
            };

            return new List<ConditionSettingConfig>() {
               brickCondition
            };
        }

        private List<ConditionSettingConfig> InitializeNotBrickWallMaterialsCondition() {
            var notBrickCondition = new ConditionSettingConfig() {
                ConditionType = ConditionType.WallMaterialClasses,
                WallMaterialClasses = new List<string>() { "Газобетон", "ПГП", "ПСП" },
            };

            return new List<ConditionSettingConfig>() {
               notBrickCondition
            };
        }

        private List<ConditionSettingConfig> InitializeConditionToOpening400_1100() {
            var openingWidthCondition = new ConditionSettingConfig() {
                ConditionType = ConditionType.OpeningWidth,
                OpeningWidthMin = 400,
                OpeningWidthMax = 1100,
            };

            return new List<ConditionSettingConfig>() { openingWidthCondition };
        }

        private List<ConditionSettingConfig> InitializeConditionToOpening200_1100() {
            var openingWidthCondition = new ConditionSettingConfig() {
                ConditionType = ConditionType.OpeningWidth,
                OpeningWidthMin = 200,
                OpeningWidthMax = 1100,
            };

            return new List<ConditionSettingConfig>() { openingWidthCondition };
        }

        private List<ConditionSettingConfig> InitializeConditionToOpening1100_2500() {
            var openingWidthCondition = new ConditionSettingConfig() {
                ConditionType = ConditionType.OpeningWidth,
                OpeningWidthMin = 1100,
                OpeningWidthMax = 2500,
            };

            return new List<ConditionSettingConfig>() { openingWidthCondition };
        }

        #endregion

        #region LintelParameters
        private List<LintelParameterSettingConfig> InitializeLintelParametersToOpeninig200_1100() {
            var yesNoLintelParameter = new LintelParameterSettingConfig() {
                LintelParameterType = LintelParameterType.YesNoLintelParameter,
                Name = "Уголок",
                IsChecked = false
            };

            return new List<LintelParameterSettingConfig> {
                yesNoLintelParameter
            };

        }

        private List<LintelParameterSettingConfig> InitializeLintelParametersToOpeninig1100_2500() {
            var yesNoLintelParameter = new LintelParameterSettingConfig() {
                LintelParameterType = LintelParameterType.YesNoLintelParameter,
                Name = "Уголок",
                IsChecked = true
            };

            return new List<LintelParameterSettingConfig> {
                yesNoLintelParameter
            };

        }

        #endregion

        #region Initializing Rules
        private RuleSettingsConfig InitializeRuleToOpeningInBrickWall200_1100() {
            var ruleconfig = new RuleSettingsConfig() { Name = "АТР_Проем_в_кирпичной_стене_200_1100" };
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeCommonConditions());
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeConditionToOpening200_1100());
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeBrickWallMaterialCondition());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeCommonLintelParametrs());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeLintelParametersToOpeninig200_1100());

            return ruleconfig;
        }

        private RuleSettingsConfig InitializeRuleToOpeningInNotBrickWall400_1100() {
            var ruleconfig = new RuleSettingsConfig() { Name = "АТР_Проем_в_некирпичной_стене_400_1100" };
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeCommonConditions());
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeConditionToOpening400_1100());
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeNotBrickWallMaterialsCondition());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeCommonLintelParametrs());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeLintelParametersToOpeninig200_1100());

            return ruleconfig;
        }

        private RuleSettingsConfig InitializeRuleToOpening1100_2500() {
            var ruleconfig = new RuleSettingsConfig() { Name = "АТР_Проем_в_стене_1100_2500" };
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeCommonConditions());
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeConditionToOpening1100_2500());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeCommonLintelParametrs());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeLintelParametersToOpeninig1100_2500());

            return ruleconfig;
        }

        #endregion

    }
}
