using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

internal class ParamService(IRevitParamFactory revitParamFactory) {
    public IEnumerable<RevitParam> CreateRevitParams(Document document, FamilySymbol symbol, FamilyInstance instance) {
        var symbolParams =  CreateRevitParams(document, symbol);
        if(instance is null) {
            return symbolParams;
        }
        var instanceParams = CreateRevitParams(document, instance);
        return symbolParams.Union(instanceParams);
    }

    private IEnumerable<RevitParam> CreateRevitParams(Document document, Element element) {
        return element.GetOrderedParameters()
            .Select(parameter => TryCreateRevitParam(document, parameter))
            .Where(revitParam => revitParam is not null);
    }
    
    private RevitParam TryCreateRevitParam(Document document, Parameter parameter) {
        if(!revitParamFactory.CanCreate(document, parameter.Id)) {
            return null;
        }

        try {
            return revitParamFactory.Create(document, parameter.Id);
        } catch(ArgumentOutOfRangeException) {
            return null;
        }
    }
    
    
    public RevitParam GetLintelParam(RevitFamily revitLintelFamily, string paramName, StorageType storageType) {
        return revitLintelFamily.OrderedParams
            .FirstOrDefault(param => param.Name.Equals(paramName) && param.StorageType == storageType);
    }
    
    public RevitParam GetOpeningParam(RevitFamily[] revitOpeningFamilies, string paramName, StorageType storageType) {
        if (revitOpeningFamilies == null || revitOpeningFamilies.Length == 0) {
            return null;
        }

        bool existsInAll = revitOpeningFamilies.All(f =>
            f.OrderedParams.Any(p => p.Name == paramName));

        return !existsInAll 
            ? null 
            : revitOpeningFamilies[0].OrderedParams
                .FirstOrDefault(param => param.Name.Equals(paramName) && param.StorageType == storageType);
    }
    
    
    public RevitParam GetFixParam(Document document, LintelFamilyConfig lintelFamilyConfig, OpeningFamilyConfig openingFamilyConfig, string paramName, StorageType storageType) {
        if(lintelFamilyConfig is null || openingFamilyConfig is null) {
            return null;
        }
        
        var allInstances = lintelFamilyConfig.RevitLintelFamily.FamilyInstances
            .Concat(openingFamilyConfig.RevitOpeningFamily.SelectMany(family => family.FamilyInstances))
            .GroupBy(instance => instance.Symbol.Family.Id)
            .ToArray();

        if(allInstances.Length == 0) {
            return null;
        }
        
        bool existsInAll = allInstances
            .All(group => {
                var instance = group.First();
                return instance.IsExistsParam(paramName);
            });
        
        if(!existsInAll) {
            return null;
        }
        
        var instance = allInstances[0].First();
        var param = CreateRevitParamByName(document, instance, paramName);

        if(param is not null && param.StorageType == storageType) {
            return param;
        }
        
        return null;
    }
    
    private RevitParam CreateRevitParamByName(Document document, Element element, string paramName) {
        if(TryGetParam(element, paramName, out var param) && revitParamFactory.CanCreate(document, param.Id)) {
            return revitParamFactory.Create(document, param.Id);
        }
        return null;
    }
    
    private static bool TryGetParam(Element element, string paramName, out Parameter param) {
        try {
            param = element.GetParam(paramName);
            return param != null;
        } catch {
            param = null;
            return false;
        }
    }
}
