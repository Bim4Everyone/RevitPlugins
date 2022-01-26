using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitLintelPlacement.Models {
    internal class RulesTemplateInitializer {

        //TODO: уточнить все названия (материалов, классов материалов, типов стен)
        public RulesSettigs GetTemplateRules() {

            var ruleSettingCollection = new RulesSettigs();
            ruleSettingCollection.IsSystem = true;
            ruleSettingCollection.RuleSettings = new List<RuleSetting>() { 
                InitializeRuleToOpeningInBrickWall200_1100(),
                InitializeRuleToOpeningInNotBrickWall400_1100(),
                InitializeRuleToOpening1100_2500()
            };
            return ruleSettingCollection;
        }

        #region Common Conditions and LintelParameters

        private List<LintelParameterSetting> InitializeCommonLintelParametrs() {

            var leftOffsetParameter = new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.NumberParameter,
                Name = "Смещение_слева",
                NumberValue = 250
            };
            var rightOffsetParameter = new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.NumberParameter,
                Name = "Смещение_справа",
                NumberValue = 250
            };
            var thicknessParameter = new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.RelativeWallParameter,
                Name = "Половина толщины стены",
                RelationValue = 0.5,
                WallParameterName = "Толщина"
            };
            var widthParameter = new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.ReletiveOpeneingParameter,
                Name = "ЭЛМТ_ширина проема",
                RelationValue = 1,
                OpeninigParameterName = "Ширина"
            };
            return new List<LintelParameterSetting> {
                leftOffsetParameter,
                rightOffsetParameter,
                thicknessParameter,
                widthParameter
            };
        }

        private List<ConditionSetting> InitializeCommonConditions() {

            var wallExclusionTypeCondition = new ConditionSetting() {
                ConditionType = ConditionType.ExclusionWallTypes,
                ExclusionWallTypes = new List<string> { "Невозводимые" },
            };
            var materialClasses = new ConditionSetting() {
                ConditionType = ConditionType.WallMaterialClasses,
                WallMaterialClasses = new List<string>() { "Кладка" },
            };
            return new List<ConditionSetting>() { wallExclusionTypeCondition, materialClasses };
        }

        #endregion

        #region Conditions
        private List<ConditionSetting> InitializeBrickWallMaterialCondition() {
            var brickCondition = new ConditionSetting() {
                ConditionType = ConditionType.WallMaterials,
                WallMaterials = new List<string>() { "Кирпич" },
            };

            return new List<ConditionSetting>() {
               brickCondition
            };
        }

        private List<ConditionSetting> InitializeNotBrickWallMaterialsCondition() {
            var notBrickCondition = new ConditionSetting() {
                ConditionType = ConditionType.WallMaterials,
                WallMaterials = new List<string>() { "Газобетон", "ПГП", "ПСП" },
            };

            return new List<ConditionSetting>() {
               notBrickCondition
            };
        }

        private List<ConditionSetting> InitializeConditionToOpening400_1100() {
            var openingWidthCondition = new ConditionSetting() {
                ConditionType = ConditionType.OpeningWidth,
                OpeningWidthMin = 400,
                OpeningWidthMax = 1100,
            };

            return new List<ConditionSetting>() { openingWidthCondition };
        }

        private List<ConditionSetting> InitializeConditionToOpening200_1100() {
            var openingWidthCondition = new ConditionSetting() {
                ConditionType = ConditionType.OpeningWidth,
                OpeningWidthMin = 200,
                OpeningWidthMax = 1100,
            };

            return new List<ConditionSetting>() { openingWidthCondition };
        }

        private List<ConditionSetting> InitializeConditionToOpening1100_2500() {
            var openingWidthCondition = new ConditionSetting() {
                ConditionType = ConditionType.OpeningWidth,
                OpeningWidthMin = 1100,
                OpeningWidthMax = 2500,
            };

            return new List<ConditionSetting>() { openingWidthCondition };
        }

        #endregion

        #region LintelParameters
        private List<LintelParameterSetting> InitializeLintelParametersToOpeninig200_1100() {
            var yesNoLintelParameter = new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.YesNoLintelParameter,
                Name = "Уголок",
                IsChecked = false
            };

            return new List<LintelParameterSetting> {
                yesNoLintelParameter
            };

        }

        private List<LintelParameterSetting> InitializeLintelParametersToOpeninig1100_2500() {
            var yesNoLintelParameter = new LintelParameterSetting() {
                LintelParameterType = LintelParameterType.YesNoLintelParameter,
                Name = "Уголок",
                IsChecked = true
            };

            return new List<LintelParameterSetting> {
                yesNoLintelParameter
            };

        }

        #endregion

        #region Initializing Rules
        private RuleSetting InitializeRuleToOpeningInBrickWall200_1100() {
            var ruleconfig = new RuleSetting() { Name = "АТР_Проем_в_кирпичной_стене_200_1100" };
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeCommonConditions());
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeConditionToOpening200_1100());
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeBrickWallMaterialCondition());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeCommonLintelParametrs());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeLintelParametersToOpeninig200_1100());

            return ruleconfig;
        }

        private RuleSetting InitializeRuleToOpeningInNotBrickWall400_1100() {
            var ruleconfig = new RuleSetting() { Name = "АТР_Проем_в_некирпичной_стене_400_1100" };
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeCommonConditions());
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeConditionToOpening400_1100());
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeNotBrickWallMaterialsCondition());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeCommonLintelParametrs());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeLintelParametersToOpeninig200_1100());

            return ruleconfig;
        }

        private RuleSetting InitializeRuleToOpening1100_2500() {
            var ruleconfig = new RuleSetting() { Name = "АТР_Проем_в_стене_1100_2500" };
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeCommonConditions());
            ruleconfig.ConditionSettingsConfig.AddRange(InitializeConditionToOpening1100_2500());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeCommonLintelParametrs());
            ruleconfig.LintelParameterSettingsConfig.AddRange(InitializeLintelParametersToOpeninig1100_2500());

            return ruleconfig;
        }

        #endregion

    }
}
