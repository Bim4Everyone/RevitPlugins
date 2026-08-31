using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitLintelsManager.Models.Rules;
using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models.Configs;

internal class LintelManagerConfigResolver {
    private readonly RevitRepository _revitRepository;  
    private readonly LintelConfigRuleResolver _lintelConfigRuleResolver;
    
    private readonly ParamService _paramService;
    
    public LintelManagerConfigResolver(
        RevitRepository revitRepository, 
        LintelConfigRuleResolver lintelConfigRuleResolver,
        ParamService paramService) {
        _revitRepository = revitRepository;
        _lintelConfigRuleResolver = lintelConfigRuleResolver;
        _paramService = paramService;
    }
 
    public LintelManagerConfig BuildLintelManagerConfig(LintelManagerSettings lintelManagerSettings) {
        var lintelFamilyConfig = BuildLintelFamilyConfig(lintelManagerSettings.LintelFamilySettings);
        var openingFamilyConfig = BuildOpeningFamilyConfig(lintelManagerSettings.OpeningFamilySettings);
        var lintelConfigRule = _lintelConfigRuleResolver.GetLintelConfigRule(lintelManagerSettings.LintelConfigRuleName);
        
        var fixParam = _paramService.GetFixParam(
            _revitRepository.Document, 
            lintelFamilyConfig, 
            openingFamilyConfig, 
            lintelManagerSettings.LintelFixParamName, 
            StorageType.Integer);
        
        double? minimalHeightAboveOpeningMm = GetMinimalHeightAboveOpeningMm(lintelManagerSettings);
        var structureWalls = GetStructureWalls(lintelManagerSettings.StructureWallTypeNames);
        var phaseId = GetPhaseId(lintelManagerSettings);
        
        return new LintelManagerConfig {
            LintelFamilyConfig = lintelFamilyConfig,
            OpeningFamilyConfig = openingFamilyConfig,
            LintelConfigRule = lintelConfigRule,
            LintelFixParam = fixParam,
            MinimalHeightAboveOpeningMm = minimalHeightAboveOpeningMm,
            StructureWalls = structureWalls,
            PhaseId = phaseId
        };
    }

    private LintelFamilyConfig BuildLintelFamilyConfig(LintelFamilySettings lintelFamilySettings) {
        var revitLintelFamily = _revitRepository.LintelFamilies
            .FirstOrDefault(family => family.Family.Name.Equals(lintelFamilySettings.LintelFamily));

        if(revitLintelFamily is not null) {
            return new LintelFamilyConfig {
                RevitLintelFamily = revitLintelFamily,
                LintelWidth = _paramService.GetLintelParam(revitLintelFamily, lintelFamilySettings.LintelWidth, StorageType.Double),
                LintelThickness = _paramService.GetLintelParam(revitLintelFamily, lintelFamilySettings.LintelThickness, StorageType.Double),
                LintelRightOffset = _paramService.GetLintelParam(revitLintelFamily, lintelFamilySettings.LintelRightOffset, StorageType.Double),
                LintelLeftOffset = _paramService.GetLintelParam(revitLintelFamily, lintelFamilySettings.LintelLeftOffset, StorageType.Double),
                LintelRightCorner = _paramService.GetLintelParam(revitLintelFamily, lintelFamilySettings.LintelRightCorner, StorageType.Double),
                LintelLeftCorner = _paramService.GetLintelParam(revitLintelFamily, lintelFamilySettings.LintelLeftCorner, StorageType.Double),
                LintelRightWelding = _paramService.GetLintelParam(revitLintelFamily, lintelFamilySettings.LintelRightWelding, StorageType.Double),
                LintelLeftWelding = _paramService.GetLintelParam(revitLintelFamily, lintelFamilySettings.LintelLeftWelding, StorageType.Double)
            };
        }
        return null;
    }
    
    private OpeningFamilyConfig BuildOpeningFamilyConfig(OpeningFamilySettings openingFamilySettings) {
        var revitOpeningFamily = openingFamilySettings.FamilyNames.ToHashSet();
        
        if(revitOpeningFamily.Count == 0) {
            return null;
        }

        var revitOpeningFamilyInstances = _revitRepository.OpeningFamilies
            .Where(family => revitOpeningFamily.Contains(family.Family.Name))
            .ToArray();

        if(revitOpeningFamilyInstances.Length != 0) {
            return new OpeningFamilyConfig {
                RevitOpeningFamily = revitOpeningFamilyInstances,
                LintelOpeningHeight = _paramService.GetOpeningParam(revitOpeningFamilyInstances, openingFamilySettings.LintelOpeningHeight, StorageType.Double),
                LintelOpeningWidth = _paramService.GetOpeningParam(revitOpeningFamilyInstances, openingFamilySettings.LintelOpeningWidth, StorageType.Double)
            };
        }
        return null;
    }

    private static double? GetMinimalHeightAboveOpeningMm(LintelManagerSettings lintelManagerSettings) {
        double minimalHeightAboveOpeningMm = lintelManagerSettings.MinimalHeightAboveOpeningMm;
        if(minimalHeightAboveOpeningMm >= 0) {
            return minimalHeightAboveOpeningMm;
        }
        return null;
    }

    private ElementId GetPhaseId(LintelManagerSettings lintelManagerSettings) {
        var savedPhaseId = _revitRepository.GetPhaseIdByName(lintelManagerSettings.PhaseName);
        if(savedPhaseId == null) {
            return null;
        }
        return GetExistIds([savedPhaseId], _revitRepository.PhaseIds)
            .FirstOrDefault();
    }
    
    private static IEnumerable<ElementId> GetExistIds(IEnumerable<ElementId> savedIds, IEnumerable<ElementId> existIds) {
        var savedIdsArr = savedIds.ToArray();
        var existIdsHashSet = existIds.ToHashSet();

        if(savedIdsArr.Length != 0 || existIdsHashSet.Count != 0) {
            return savedIdsArr
                .Select(savedId => existIdsHashSet.FirstOrDefault(existId => existId == savedId))
                .Where(savedId => savedId != null);
        }
        return null;
    }
    
    private IEnumerable<RevitWall> GetStructureWalls(IEnumerable<string> wallTypeNames) {
        var names = wallTypeNames.ToHashSet();
        var revitWalls = _revitRepository.RevitWalls.ToArray();
        if(names.Count == 0 || revitWalls.Length == 0) {
            return null;
        }
        return _revitRepository.RevitWalls
            .Where(x => names.Contains(x.WallType.Name));
    }
}
