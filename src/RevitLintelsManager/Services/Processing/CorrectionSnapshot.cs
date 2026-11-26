using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using dosymep.Revit;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.Services;

/// <summary>Исходные данные, которые Correct обязан сохранить до фиксации транзакции.</summary>
internal sealed class CorrectionSnapshot {
    private readonly Dictionary<ElementId, (bool HasValue, object Value)> _parameters;
    private readonly List<(string Id, bool Locked, string Reference)> _constraints;
    private readonly IReadOnlyList<string> _centralLocks;
    private readonly Transform _openingPose;
    private readonly Dictionary<ElementId, (bool HasValue, object Value)> _openingParameters;
    private readonly WorksetId _workset;
    private readonly bool _heightChanged;
    private readonly bool _mirrored;
    private readonly bool _handFlipped;
    private readonly bool _facingFlipped;

    internal CorrectionSnapshot(ProcessingTarget target) {
        var lintel = target.Lintel;
        _parameters = ReadUserValues(lintel);
        _centralLocks = target.Match.LockFacts is { State: LintelLockState.Confirmed } facts ? facts.DependencyIds : [];
        _constraints = ReadOtherConstraints(lintel);
        _openingPose = target.Opening.GetTransform();
        _openingParameters = ReadUserValues(target.Opening);
        ExpectedPose = lintel.GetTransform();
        _workset = lintel.WorksetId;
        _heightChanged = target.Comparison.Differences.Any(diff => diff.Code == LintelDifferenceCode.Height);
        _mirrored = lintel.Mirrored;
        _handFlipped = lintel.HandFlipped;
        _facingFlipped = lintel.FacingFlipped;
    }

    internal HashSet<ElementId> WrittenParameters { get; } = [];
    internal Transform ExpectedPose { get; set; }

    // Сверяет ключи пары, трансформацию проёма, зависимости и все остальные редактируемые значения.
    internal bool IsPreserved(ProcessingTarget target) {
        var lintel = target.Lintel;
        var initial = target.Match.Lintel;
        var current = ReadUserValues(lintel);
        using var pose = target.Opening.GetTransform();
        using var lintelPose = lintel.GetTransform();
        var openingParameters = ReadUserValues(target.Opening);
        return lintel.UniqueId == initial.Element.UniqueId && lintel.Id.GetIdValue() == initial.Element.Information.ElementId
            && lintel.Symbol.UniqueId == initial.Element.TypeUniqueId && lintel.Symbol.Family.UniqueId == initial.Element.FamilyUniqueId
            && target.Document.GetElement(lintel.LevelId)?.UniqueId == initial.LevelUniqueId
            && target.Document.GetElement(lintel.CreatedPhaseId)?.UniqueId == initial.CreatedPhaseUniqueId
            && lintel.DemolishedPhaseId.GetIdValue() == initial.Element.Information.Phases.DemolishedId
            && lintel.WorksetId.Equals(_workset) && lintel.Mirrored == _mirrored
            && lintel.HandFlipped == _handFlipped && lintel.FacingFlipped == _facingFlipped
            && target.Opening.Host?.UniqueId == target.Match.Opening.HostWallUniqueId && pose.AlmostEqual(_openingPose)
            && lintelPose.AlmostEqual(ExpectedPose) && _openingParameters.Count == openingParameters.Count
            && _openingParameters.All(entry => openingParameters.TryGetValue(entry.Key, out var value) && entry.Value.Equals(value))
            && _constraints.SequenceEqual(ReadOtherConstraints(lintel))
            && current.Count == _parameters.Count && _parameters.All(entry => current.TryGetValue(entry.Key, out var actual)
                && (WrittenParameters.Contains(entry.Key) || IsHeightOffset(entry.Key) || entry.Value.Equals(actual)));
    }

    // Перенос по Z изменяет собственное смещение экземпляра относительно сохранённого уровня.
    private bool IsHeightOffset(ElementId id) => _heightChanged && (id.GetIdValue() == (long) BuiltInParameter.INSTANCE_ELEVATION_PARAM
        || id.GetIdValue() == (long) BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM);

    // Вычисляемые Revit значения (например, объём тела) следуют за геометрией при регенерации.
    private static Dictionary<ElementId, (bool, object)> ReadUserValues(FamilyInstance lintel) =>
        lintel.Parameters.Cast<Parameter>().Where(parameter => !parameter.IsReadOnly).ToDictionary(
            parameter => parameter.Id, parameter => (parameter.HasValue, parameter.HasValue ? parameter.AsObject() : null));

    // Центральные замки отдельно перепроверяет LintelLockReader по актуальным плоскостям обоих семейств.
    // Revit может изменить внутренний ID поверхности именованной плоскости при регенерации семейства.
    private List<(string, bool, string)> ReadOtherConstraints(FamilyInstance lintel) {
        using var filter = new ElementClassFilter(typeof(Dimension));
        var document = lintel.Document;
        return lintel.GetDependentElements(filter).Select(id => (Dimension) document.GetElement(id))
            .Where(dim => !_centralLocks.Contains(dim.UniqueId)).OrderBy(dim => dim.UniqueId)
            .SelectMany(dim => dim.References.Cast<Reference>()
                .Select(reference => (dim.UniqueId, dim.IsLocked, reference.ConvertToStableRepresentation(document)))).ToList();
    }
}
