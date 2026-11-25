using System.Linq;

using Autodesk.Revit.DB;

using RevitLintelsManager.Models.Configs;

using static RevitLintelsManager.Models.RevitElementRead;

namespace RevitLintelsManager.Models;

/// <summary>Читает факты семейства перемычек для расчёта размещения.</summary>
internal sealed class LintelFamilyFactsReader(RevitRepository source, IParamService parameters)
    : ILintelFamilyFactsReader {
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
        var definition = ReadOptional(() => family.IsEditable && !family.IsInPlace
            ? document.EditFamily(family) : null);
        if(definition is null) {
            return new LintelFamilyFacts(familyId, typeIds, null, null, workset);
        }
        try {
            var sample = sampleId is null ? null : ReadOptional(() => document.GetElement(sampleId) as FamilyInstance);
            var parameterFacts = ReadOptional(() => parameters.ReadLintelParameterFacts(document, definition, family,
                sample, config.Parameters));
            int? left = ReadOptional(() => family.FamilyPlacementType == FamilyPlacementType.OneLevelBased
                ? LintelFamilyGeometry.ReadLeftSign(definition, config.Parameters) : null);
            return new LintelFamilyFacts(familyId, typeIds, left, parameterFacts, workset);
        } finally {
            definition.Close(false);
        }
    }
}
