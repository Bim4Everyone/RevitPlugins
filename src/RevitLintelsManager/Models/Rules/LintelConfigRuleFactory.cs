using System;
using System.Linq;

using dosymep.SimpleServices;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models.Rules;

public class LintelConfigRuleFactory {
    // Дефолтные значения для правил
    private readonly ILocalizationService _localizationService;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;

    internal LintelConfigRuleFactory(ILocalizationService localizationService, SystemPluginConfig systemPluginConfig, RevitRepository revitRepository) {
        _localizationService = localizationService;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
    }

    public LintelConfigRule GetDefaultLintelConfigRule() {
        var defaultLintelConfigRule = new LintelConfigRule();
        
        var defaultLintelRule = new LintelRule {
            MinWidth = _systemPluginConfig.DefaultMinWidthMm,
            MaxWidth = _systemPluginConfig.DefaultMaxWidthMm,
            WallSupport = _systemPluginConfig.DefaultWallSupportMm,
            LintelType = _systemPluginConfig.DefaultLintelType
        };
        
        var defaultLintelPlaceRule = new LintelPlaceRule {
            LintelPlaceRuleName = _localizationService.GetLocalizedString("LintelConfigRuleResolver.DefaultRuleWallName"),
            LintelRules = [defaultLintelRule],
            WallTypeIds = [String.Empty]
        };
        
        defaultLintelConfigRule.RuleConfigName = GetDefaultRuleName();
        defaultLintelConfigRule.LintelPlaceRules = [defaultLintelPlaceRule];
        
        return defaultLintelConfigRule;
    }
    
    private string GetDefaultRuleName() {
        string documentName = string.IsNullOrEmpty(_revitRepository.Document.Title)
            ? _localizationService.GetLocalizedString("LintelConfigRuleResolver.DefaultRuleName")
            : _revitRepository.Document.Title.Split('_')
                .FirstOrDefault();

        return !string.IsNullOrEmpty(documentName) 
            ? documentName 
            : _localizationService.GetLocalizedString("LintelConfigRuleResolver.DefaultRuleName");
    }
}
