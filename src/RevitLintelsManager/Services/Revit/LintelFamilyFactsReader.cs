using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitLintelsManager.Models.Configs;

using static RevitLintelsManager.Models.RevitElementRead;

namespace RevitLintelsManager.Models;

/// <summary>Читает факты семейства перемычек для расчёта размещения.</summary>
internal sealed class LintelFamilyFactsReader(RevitRepository source, IParamService parameters)
    : ILintelFamilyFactsReader {
    public ProjectReadCache SessionCache { private get; set; }

    /// <inheritdoc />
    public LintelFamilyFacts ReadLintelFamilyFacts(ManagerConfigSnapshot config, string sampleId) {
        var document = source.Document;
        long? workset = ReadOptional<long?>(() => {
            using var worksets = document.GetWorksetTable();
            var id = worksets.GetActiveWorksetId();
            return id is null || id == WorksetId.InvalidWorksetId ? null : id.IntegerValue;
        });
        if(config.LintelFamily is not { UniqueId: string familyId }
            || ReadOptional(() => document.GetElement(familyId) as Family) is not Family family) {
            return new LintelFamilyFacts(null, [], null, null, workset);
        }
        var typeIds = config.LintelTypes.Select(type => type.UniqueId).Where(id => ReadOptional(() =>
            document.GetElement(id) is FamilySymbol symbol && symbol.Family.Id == family.Id)).ToArray();
        var cache = SessionCache is { IsTracking: true } ? SessionCache : null;
        var facts = ReadOptional(() => cache is null ? ReadDefinition(document, family, config)
            : cache.ReadDefinition(family, config, () => ReadDefinition(document, family, config)));
        var sample = sampleId is null ? null : ReadOptional(() => document.GetElement(sampleId) as FamilyInstance);
        var parameterFacts = facts?.Parameters is null ? null : ReadOptional(() =>
            parameters.ReadLintelParameterFacts(document, facts.Parameters, family, sample, config.Parameters));
        return new LintelFamilyFacts(familyId, typeIds, facts?.Left, parameterFacts, workset) {
            Origin = facts?.Origin ?? default
        };
    }

    // Только сведения из определения остаются в кэше. Проектные привязки и образец проверяются при каждом чтении.
    private LintelDefinitionFacts ReadDefinition(Document document, Family family, ManagerConfigSnapshot config) {
        if(!family.IsEditable || family.IsInPlace) {
            return new LintelDefinitionFacts(null, null, true);
        }
        var definition = document.EditFamily(family);
        try {
            var parameterFacts = ReadOptional(() => parameters.ReadDefinitionParameters(definition, config.Parameters));
            var origin = ReadOptional<FamilyOriginFacts?>(() => GeometryReadContext.ReadFamilyOriginFacts(definition));
            try {
                int? left = origin.HasValue && family.FamilyPlacementType == FamilyPlacementType.OneLevelBased
                    ? LintelFamilyGeometry.ReadLeftSign(definition, config.Parameters, origin.Value) : null;
                return new LintelDefinitionFacts(left, parameterFacts, parameterFacts is not null && origin.HasValue,
                    origin.GetValueOrDefault());
            } catch(Exception exception) when(IsElementReadException(exception)) {
                return new LintelDefinitionFacts(null, parameterFacts, false, origin.GetValueOrDefault());
            }
        } finally {
            definition.Close(false);
        }
    }
}
