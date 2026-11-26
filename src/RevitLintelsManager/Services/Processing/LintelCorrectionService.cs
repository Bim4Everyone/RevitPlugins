using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;
using dosymep.SimpleServices;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Services;

/// <summary>Исправляет только различия экземпляра, подтверждённые сравнением существующей пары.</summary>
internal sealed class LintelCorrectionService(IProjectSnapshotReader snapshots, IParamService parameters,
    ILintelCalculationService calculations, ILintelLockReader locks, ILocalizationService localization)
    : ILintelCorrectionService {
    /// <inheritdoc />
    public void Apply(ProcessingTarget target, ManagerConfigSnapshot config, CancellationToken token) {
        target.BeforeCorrection = new CorrectionSnapshot(target);
        var differences = target.Comparison.Differences.Select(diff => diff.Code).ToHashSet();
        if(target.Match.LockFacts?.IsLocked is null) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.CorrectDependencies"));
        }
        if(target.Comparison.RequiresPositionCorrection) {
            CorrectPosition(target, differences);
            target.Document.Regenerate();
        }
        var pair = ReadPair(target, config);
        var comparison = calculations.Compare(pair, target.Calculation, config);
        WriteDifferences(target, pair, comparison, token);
        // При несовпадении ширины Compare откладывает оценку опираний до восстановления пролёта.
        if(comparison.Differences.Any(diff => diff.Code == LintelDifferenceCode.Width)) {
            target.Document.Regenerate();
            pair = ReadPair(target, config);
            WriteDifferences(target, pair, calculations.Compare(pair, target.Calculation, config), token);
        }
    }

    /// <inheritdoc />
    public void Verify(ProcessingTarget target, ManagerConfigSnapshot config) {
        var pair = ReadPair(target, config);
        var comparison = calculations.Compare(pair, target.Calculation, config);
        if(comparison.RequiresCorrection || !comparison.PositionKnown
            || comparison.Differences.Any(diff => diff.Code is LintelDifferenceCode.ParameterUnavailable
                or LintelDifferenceCode.SideParametersUnavailable)) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.CorrectIncomplete"));
        }
        var current = locks.ReadLocks(target.LockGeometry, [pair]).Single().LockFacts;
        var original = target.Match.LockFacts;
        if(current?.State != original.State || !current.DependencyIds.SequenceEqual(original.DependencyIds)
            || !target.BeforeCorrection.IsPreserved(target)) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.CorrectDependencies"));
        }
    }

    // Поворачивает к ближайшей соосной ориентации и переносит лишь вышедшие за допуск проекции.
    private static void CorrectPosition(ProcessingTarget target, ISet<LintelDifferenceCode> differences) {
        var instance = target.Lintel;
        var planes = target.Calculation.Target.Planes;
        var along = AsXyz(planes.CenterNormal);
        var across = AsXyz(planes.LongitudinalNormal);
        var height = AsXyz(planes.HeightNormal);
        using var pose = instance.GetTransform();
        if(differences.Contains(LintelDifferenceCode.Orientation)) {
            var direction = pose.BasisX.DotProduct(along) < 0 ? -along : along;
            double angle = RevitRepository.RotateLintel(instance, pose, direction);
            target.BeforeCorrection.ExpectedPose = Transform.CreateRotationAtPoint(XYZ.BasisZ, angle, pose.Origin)
                .Multiply(pose);
        }
        var delta = AsXyz(planes.Center) - pose.Origin;
        var move = XYZ.Zero;
        foreach(var (code, axis) in new[] { (LintelDifferenceCode.Along, along),
                (LintelDifferenceCode.Across, across), (LintelDifferenceCode.Height, height) }) {
            if(differences.Contains(code)) {
                move += axis * delta.DotProduct(axis);
            }
        }
        if(move.GetLength() > LengthEpsilon) {
            ElementTransformUtils.MoveElement(target.Document, instance.Id, move);
            target.BeforeCorrection.ExpectedPose = Transform.CreateTranslation(move).Multiply(target.BeforeCorrection.ExpectedPose);
        }
    }

    // Сохраняет признаки защищённых концов исходной пары и использует фактический базис после движения.
    private OpeningLintelMatch ReadPair(ProcessingTarget target, ManagerConfigSnapshot config) {
        var lintel = snapshots.ReadInstalledLintel(target.Lintel, target.LockGeometry, config);
        var anchor = lintel.Geometry?.Anchor;
        if(anchor is null || target.Calculation.Target.GetPhysicalLeft(lintel.Geometry.Placement) is null) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.CorrectIncomplete"));
        }
        var deviation = new PlaneDeviation(target.Calculation.Target.Planes, anchor, config.MatchLimits);
        var contact = new MatchContact(target.Opening.UniqueId, target.Lintel.UniqueId, deviation);
        var pair = new OpeningLintelMatch(target.Match.Opening, lintel, target.Match.State, [contact]);
        return new OpeningLintelMatch(pair, target.Match.RetainedEnds);
    }

    // Пишет только конкретные параметры, для которых Compare установил расхождение.
    private void WriteDifferences(ProcessingTarget target, OpeningLintelMatch pair, LintelComparison comparison,
        CancellationToken token) {
        var values = target.Calculation.GetComparableParameters(pair.Lintel.Geometry.Placement)
            .ToDictionary(value => value.Name, StringComparer.Ordinal);
        foreach(var diff in comparison.Differences.Where(item => item.Correction != ManagerCorrection.None
                && item.Parameter is not null)) {
            token.ThrowIfCancellationRequested();
            var mapped = target.Lintel.GetParameters(diff.Parameter);
            if(mapped.Count != 1 || !values.TryGetValue(diff.Parameter, out var value)
                || !parameters.WriteInstanceValue(target.Lintel, value)) {
                throw new InvalidOperationException(localization.GetLocalizedString("Processing.Parameter", diff.Parameter));
            }
            target.BeforeCorrection.WrittenParameters.Add(mapped[0].Id);
        }
    }
}
