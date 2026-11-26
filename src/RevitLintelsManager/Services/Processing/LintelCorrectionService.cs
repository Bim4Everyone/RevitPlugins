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
    ILintelCalculationService calculations, ISideBearingAnalyzer sideBearings, ILintelLockReader lockReader,
    RevitRepository source, ILocalizationService localization)
    : ILintelCorrectionService {
    /// <inheritdoc />
    public void Apply(ProcessingTarget target, ManagerConfigSnapshot config, CancellationToken token) {
        source.PrepareLintelForChange(target.LockGeometry, target.Lintel, target.Opening, localization);
        target.BeforeCorrection = new CorrectionSnapshot(target);
        CorrectPosition(target);
        target.Document.Regenerate();
        var pair = ReadPair(target, config);
        var comparison = calculations.Compare(pair, target.Calculation, config);
        // Изменение пролёта смещает оба конца. Опирания и крепления определяются по новой геометрии.
        var width = comparison.Differences.FirstOrDefault(diff => diff.Code == LintelDifferenceCode.Width);
        if(width is not null) {
            WriteDifferences(target, pair, [width], token);
            target.Document.Regenerate();
            pair = ReadPair(target, config);
            comparison = calculations.Compare(pair, target.Calculation, config);
        }
        WriteDifferences(target, pair, comparison.Differences, token);
        token.ThrowIfCancellationRequested();
        target.CreatedLockId = source.CreateCentralLock(target.LockGeometry, target.Opening, target.Lintel,
            pair, localization);
    }

    /// <inheritdoc />
    public void Verify(ProcessingTarget target, ManagerConfigSnapshot config) {
        var pair = ReadPair(target, config);
        if(!lockReader.ConfirmsCreatedLock(target.LockGeometry, pair, target.CreatedLockId)) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.LockFailed"));
        }
        var comparison = calculations.Compare(pair, target.Calculation, config);
        if(comparison.RequiresCorrection || !comparison.PositionKnown
            || comparison.Differences.Any(diff => diff.Code is LintelDifferenceCode.ParameterUnavailable
                or LintelDifferenceCode.SideParametersUnavailable)) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.CorrectIncomplete"));
        }
        if(!target.BeforeCorrection.IsPreserved(target)) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.CorrectDependencies"));
        }
    }

    // Для замка совмещает центральные плоскости точно. Поперечное смещение и высоту исправляет по допускам.
    private static void CorrectPosition(ProcessingTarget target) {
        var instance = target.Lintel;
        var planes = target.Calculation.Target.Planes;
        var along = AsXyz(planes.CenterNormal);
        var across = AsXyz(planes.LongitudinalNormal);
        var height = AsXyz(planes.HeightNormal);
        using var pose = instance.GetTransform();
        var direction = pose.BasisX.DotProduct(along) < 0 ? -along : along;
        double angle = RevitRepository.RotateLintel(instance, pose, direction);
        target.BeforeCorrection.ExpectedPose = Transform.CreateRotationAtPoint(XYZ.BasisZ, angle, pose.Origin)
            .Multiply(pose);
        var center = RevitRepository.GetAlignedCenter(target.LockGeometry, target.Opening, instance.Symbol.Family,
            planes, direction);
        var delta = center - pose.Origin;
        var move = along * delta.DotProduct(along);
        if(target.Comparison.Differences.Any(diff => diff.Code == LintelDifferenceCode.Across)) {
            move += across * delta.DotProduct(across);
        }
        if(target.Comparison.Differences.Any(diff => diff.Code == LintelDifferenceCode.Height)) {
            move += height * delta.DotProduct(height);
        }
        if(move.GetLength() > SystemPluginConfig.LockCoincidenceToleranceFeet) {
            ElementTransformUtils.MoveElement(target.Document, instance.Id, move);
            target.BeforeCorrection.ExpectedPose = Transform.CreateTranslation(move).Multiply(target.BeforeCorrection.ExpectedPose);
        }
    }

    // Проверяет оба фактических конца по стенам той же стадии после регенерации.
    private OpeningLintelMatch ReadPair(ProcessingTarget target, ManagerConfigSnapshot config) {
        var lintel = snapshots.ReadInstalledLintel(target.Lintel, target.LockGeometry, config);
        var anchor = lintel.Geometry?.Anchor;
        if(anchor is null || target.Calculation.Target.GetPhysicalLeft(lintel.Geometry.Placement) is null) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.CorrectIncomplete"));
        }
        var deviation = new PlaneDeviation(target.Calculation.Target.Planes, anchor, config.MatchLimits);
        var contact = new MatchContact(target.Opening.UniqueId, target.Lintel.UniqueId, deviation);
        var pair = new OpeningLintelMatch(target.Match.Opening, lintel, target.Match.State, [contact]);
        return sideBearings.ReadRetainedEnds(target.LockGeometry, [pair], target.EndWalls,
            config.LintelFamily.UniqueId, target.Calculation.Target.FamilyLeftSign)[0];
    }

    // Пишет только конкретные параметры, для которых Compare установил расхождение.
    private void WriteDifferences(ProcessingTarget target, OpeningLintelMatch pair, IEnumerable<LintelDifference> differences,
        CancellationToken token) {
        var values = target.Calculation.GetComparableParameters(pair.Lintel.Geometry.Placement)
            .ToDictionary(value => value.Name, StringComparer.Ordinal);
        foreach(var diff in differences.Where(item => item.Correction != ManagerCorrection.None
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
