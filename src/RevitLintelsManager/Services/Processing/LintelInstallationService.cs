using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitLintelsManager.Models;
using RevitLintelsManager.Models.Configs;

using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Services;

/// <summary>Применяет один расчёт к создаваемой перемычке в транзакции Install или Replace.</summary>
internal sealed class LintelInstallationService(RevitRepository source, IParamService parameters,
    IProjectSnapshotReader snapshots, IPlacementGeometryReader placements, ILintelLockReader lockReader,
    ILocalizationService localization)
    : ILintelInstallationService {
    /// <inheritdoc />
    public void Install(ProcessingTarget target, ManagerConfigSnapshot config, CancellationToken token) {
        var calculation = target.Calculation;
        var document = target.Document;
        var instance = source.PlaceLintel(calculation, target.Opening, target.LockGeometry, localization);
        target.InstalledLintel = instance;
        var context = calculation.Context;
        var phase = (Phase) document.GetElement(context.CreatedPhaseId);
        if(instance.CreatedPhaseId != phase.Id) {
            instance.CreatedPhaseId = phase.Id;
        }
        if(instance.DemolishedPhaseId != target.Opening.DemolishedPhaseId) {
            instance.DemolishedPhaseId = target.Opening.DemolishedPhaseId;
        }
        if(document.IsWorkshared && instance.WorksetId.IntegerValue != context.ActiveWorksetId) {
            instance.SetParamValue(BuiltInParameter.ELEM_PARTITION_PARAM, checked((int) context.ActiveWorksetId));
        }
        document.Regenerate();
        foreach(var value in GetValues(target)) {
            token.ThrowIfCancellationRequested();
            if(!parameters.WriteInstanceValue(instance, value)) {
                throw new InvalidOperationException(localization.GetLocalizedString("Processing.Parameter", value.Name));
            }
        }
        document.Regenerate();
        var lintel = snapshots.ReadInstalledLintel(instance, target.LockGeometry, config);
        target.InstalledPair = new OpeningLintelMatch(target.Match.Opening, lintel, MatchState.Confirmed, []);
        token.ThrowIfCancellationRequested();
        target.CreatedLockId = source.CreateCentralLock(target.LockGeometry, target.Opening, instance,
            target.InstalledPair, localization);
    }

    /// <inheritdoc />
    public void Verify(ProcessingTarget target) {
        var instance = target.InstalledLintel;
        var calculation = target.Calculation;
        var context = calculation.Context;
        if(instance.Symbol.UniqueId != calculation.Type.UniqueId || instance.Symbol.Family.UniqueId != calculation.Family.UniqueId
            || instance.LevelId != target.Opening.LevelId || instance.WorksetId.IntegerValue != context.ActiveWorksetId
            || target.Document.GetElement(instance.CreatedPhaseId)?.UniqueId != context.CreatedPhaseId
            || instance.DemolishedPhaseId != target.Opening.DemolishedPhaseId
            || instance.DemolishedPhaseId.GetIdValue() != context.DemolishedPhaseId) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.InstallAttributes"));
        }
        foreach(var value in GetValues(target)) {
            bool matches = value.Kind == LintelParameterKind.LengthMm
                ? parameters.ReadLengthMm(instance, null, value.Name) is double actual
                    && Math.Abs(actual - value.LengthMm.Value) <= SystemPluginConfig.MatchRoundoffMm
                : parameters.ReadYesNo(instance, value.Name) == value.YesNo;
            if(!matches) {
                throw new InvalidOperationException(localization.GetLocalizedString("Processing.InstallValue", value.Name));
            }
        }
        if(!lockReader.ConfirmsCreatedLock(target.LockGeometry, target.InstalledPair, target.CreatedLockId)) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.LockFailed"));
        }
    }

    // Проверяет положение нижней опорной плоскости и получает значения для физических сторон экземпляра.
    private IEnumerable<LintelParameterValue> GetValues(ProcessingTarget target) {
        var geometry = placements.ReadLintelGeometry(target.LockGeometry, target.InstalledLintel);
        var expected = target.Calculation.Target.Planes;
        if(geometry.Anchor is not { } anchor || AsXyz(anchor.Center).DistanceTo(AsXyz(expected.Center)) > LengthEpsilon
            || !AsXyz(anchor.CenterNormal).IsAlmostEqualTo(AsXyz(expected.CenterNormal))
            || !AsXyz(anchor.HeightNormal).IsAlmostEqualTo(AsXyz(expected.HeightNormal))) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.InstallPosition"));
        }
        var values = target.Calculation.GetParametersFor(geometry.Placement);
        if(values.Count + 1 != LintelParameterSpec.All.Count || target.Calculation.Context.Fixation is not { } fixation) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.InstallValues"));
        }
        return values.Append(fixation);
    }
}
