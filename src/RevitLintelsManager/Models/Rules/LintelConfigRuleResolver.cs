using System;
using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;

namespace RevitLintelsManager.Models.Rules;

public class LintelConfigRuleResolver {
    private readonly ILoggerService _loggerService;
    private readonly LintelConfigRuleStorage _lintelConfigRuleStorage;
    private readonly LintelConfigRuleSerializer _lintelConfigRuleSerializer;
    private readonly LintelConfigRuleValidator _lintelConfigRuleValidator;
    
    public LintelConfigRuleResolver(
        ILoggerService loggerService,
        LintelConfigRuleStorage lintelConfigRuleStorage, 
        LintelConfigRuleSerializer lintelConfigRuleSerializer,
        LintelConfigRuleValidator lintelConfigRuleValidator) {
        _loggerService = loggerService;
        _lintelConfigRuleStorage = lintelConfigRuleStorage;
        _lintelConfigRuleSerializer = lintelConfigRuleSerializer;
        _lintelConfigRuleValidator = lintelConfigRuleValidator;
    }
    
    public IList<LintelConfigRule> LintelConfigRules => GetLintelConfigRules();
    
    // Метод получения правила конфигурации по его имени
    public LintelConfigRule GetLintelConfigRule(string lintelConfigRuleName) {
        return LintelConfigRules
            .FirstOrDefault(x => x.RuleConfigName
                .Equals(lintelConfigRuleName, StringComparison.OrdinalIgnoreCase));
    }
    

    private IList<LintelConfigRule> GetLintelConfigRules() {
        var projectConfigRules = LoadProjectConfigRules();
        var templateConfigRule = LoadTemplateConfigRule();

        if(templateConfigRule is null) {
            return projectConfigRules;
        }

        if(projectConfigRules.Count == 0) {
            return [templateConfigRule];
        }

        bool templateFound = false;

        for(int i = 0; i < projectConfigRules.Count; i++) {
            var projectConfigRule = projectConfigRules[i];

            if(!projectConfigRule.RuleConfigName.Equals(templateConfigRule.RuleConfigName, StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            templateFound = true;

            if(_lintelConfigRuleValidator.Validate(templateConfigRule, projectConfigRule)) {
                continue;
            }

            projectConfigRules[i] = templateConfigRule;
            
            SaveConfigRule(templateConfigRule);
        }
            
        if(templateFound) {
            return projectConfigRules;
        }

        projectConfigRules.Add(templateConfigRule);
        
        SaveConfigRule(templateConfigRule);

        return projectConfigRules;
    }

    private void SaveConfigRule(LintelConfigRule lintelConfigRule) {
        string ruleName = lintelConfigRule.RuleConfigName;
        if(string.IsNullOrEmpty(ruleName)) {
            return;
        }
        string projectConfigRulePath = _lintelConfigRuleStorage.GetProjectConfigRulePath(ruleName);
        
        if(string.IsNullOrEmpty(projectConfigRulePath)) {
            return;
        }
        
        try {
            _lintelConfigRuleSerializer.SaveConfigRule(lintelConfigRule, projectConfigRulePath);
        } catch(Exception ex) {
            _loggerService.Error(ex.Message);
        }
    }


    private LintelConfigRule LoadTemplateConfigRule() {
        string templateConfigRulePath = _lintelConfigRuleStorage.TemplateConfigRulePath;
        if(string.IsNullOrEmpty(templateConfigRulePath)) {
            return null;
        }
        try {
            return _lintelConfigRuleSerializer.LoadConfigRule(templateConfigRulePath);
        } catch(Exception ex) {
            _loggerService.Error(ex.Message);
            return null;
        } 
    }

    private IList<LintelConfigRule> LoadProjectConfigRules() {
        var projectConfigRulePaths = _lintelConfigRuleStorage.ProjectConfigRulePaths;
        if(projectConfigRulePaths.Count == 0) {
            return [];
        }
        var configs = new List<LintelConfigRule>();
        foreach(string projectConfigRulePath in projectConfigRulePaths) {
            try {
                var lintelConfigRule = _lintelConfigRuleSerializer.LoadConfigRule(projectConfigRulePath);
                if(lintelConfigRule == null) {
                    continue;
                }
                configs.Add(lintelConfigRule);
            } catch(Exception ex) {
                _loggerService.Error(ex.Message);
            }
        }
        
        return configs;
    }
}
