using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models.Rules;

public class LintelConfigRuleResolver {
    private readonly ILoggerService _loggerService;
    private readonly LintelConfigRuleStorage _lintelConfigRuleStorage;
    private readonly LintelConfigRuleFactory _lintelConfigRuleFactory;
    private readonly LintelConfigRuleSerializer _lintelConfigRuleSerializer;
    private readonly LintelConfigRuleValidator _lintelConfigRuleValidator;
    
    internal LintelConfigRuleResolver(
        ILoggerService loggerService,
        LintelConfigRuleStorage lintelConfigRuleStorage, 
        LintelConfigRuleFactory lintelConfigRuleFactory, 
        LintelConfigRuleSerializer lintelConfigRuleSerializer,
        LintelConfigRuleValidator lintelConfigRuleValidator) {
        _loggerService = loggerService;
        _lintelConfigRuleStorage = lintelConfigRuleStorage;
        _lintelConfigRuleFactory = lintelConfigRuleFactory;
        _lintelConfigRuleSerializer = lintelConfigRuleSerializer;
        _lintelConfigRuleValidator = lintelConfigRuleValidator;
    }
    
    public bool HasError { get; set; }
    public IList<LintelConfigRule> LintelConfigRules => GetLintelConfigRules();
    
    // Метод получения правила конфигурации по его имени
    public LintelConfigRule GetLintelConfigRule(string lintelConfigRuleName) {
        return LintelConfigRules
            .FirstOrDefault(x => x.RuleConfigName
                .Equals(lintelConfigRuleName, StringComparison.OrdinalIgnoreCase));
    }
    

    private IList<LintelConfigRule> GetLintelConfigRules() {
        var projectConfigRules = GetProjectConfigRules();
        var templateConfigRule = GetTemplateConfigRule();
        
        if(projectConfigRules.Count == 0) {
            if(templateConfigRule is not null) {
                return [templateConfigRule];
            }
            return [];
        }

        foreach(var projectConfigRule in projectConfigRules) {
            if(!projectConfigRule.RuleConfigName.Equals(templateConfigRule.RuleConfigName, StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            if(!_lintelConfigRuleValidator.Validate(projectConfigRule)) {
                
            }
        }
        return projectConfigRules;
        
    }


    private LintelConfigRule GetTemplateConfigRule() {
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

    private IList<LintelConfigRule> GetProjectConfigRules() {
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
