using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitLintelsManager.Models.Configs;

using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Models;

/// <summary>Приложение и исходный документ, зафиксированный при запуске команды.</summary>
internal sealed class RevitRepository(UIApplication uiApplication) {
    public UIApplication UIApplication { get; } = uiApplication;
    public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
    public Application Application => UIApplication.Application;
    public Document Document { get; } = uiApplication.ActiveUIDocument.Document;

    /// <summary>Проверяет идентичность открытого исходного проекта по экземпляру документа Revit.</summary>
    public bool IsSourceDocumentActive(UIApplication app) {
        return Document.IsValidObject && app.ActiveUIDocument?.Document is { IsValidObject: true } activeDocument
            && Document.Equals(activeDocument);
    }

    // Получает стадию из неизменяемой конфигурации текущей сессии.
    public ElementId GetReadPhaseId(ManagerConfigSnapshot config) {
        if(Document.GetElement(config.Phase.UniqueId) is not Phase phase) {
            throw new InvalidOperationException("Настроенная стадия удалена из исходного документа.");
        }
        return phase.Id;
    }

    // Привязывает одноуровневое семейство к уровню проёма и ориентирует по физической оси стены.
    internal FamilyInstance PlaceLintel(LintelCalculation calculation, ILocalizationService localization) {
        if(!calculation.IsApplicable || Document.GetElement(calculation.Type.UniqueId) is not FamilySymbol symbol
            || symbol.Family.UniqueId != calculation.Family.UniqueId
            || symbol.Family.FamilyPlacementType != FamilyPlacementType.OneLevelBased
            || Document.GetElement(calculation.Context.LevelId) is not Level level) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.InstallType"));
        }
        if(!symbol.IsActive) {
            symbol.Activate();
            Document.Regenerate();
        }
        var target = calculation.Target.Planes;
        var center = AsXyz(target.Center);
        var instance = Document.Create.NewFamilyInstance(center, symbol, level, StructuralType.NonStructural)
            ?? throw new InvalidOperationException(localization.GetLocalizedString("Processing.InstallType"));
        Document.Regenerate();
        using var pose = instance.GetTransform();
        RotateLintel(instance, pose, AsXyz(target.CenterNormal));
        var displacement = center - pose.Origin;
        if(displacement.GetLength() > LengthEpsilon) {
            ElementTransformUtils.MoveElement(Document, instance.Id, displacement);
        }
        return instance;
    }

    // Поворачивает вокруг точки вставки и возвращает фактически заданный угол для проверки базиса.
    internal static double RotateLintel(FamilyInstance instance, Transform pose, XYZ direction) {
        double angle = Math.Atan2(pose.BasisX.CrossProduct(direction).Z, pose.BasisX.DotProduct(direction));
        if(Math.Abs(angle) <= DirectionEpsilon) {
            return 0;
        }
        using var axis = Line.CreateUnbound(pose.Origin, XYZ.BasisZ);
        ElementTransformUtils.RotateElement(instance.Document, instance.Id, axis, angle);
        return angle;
    }

    // Сверяет полный результат Delete с владельцами геометрии и единственным центральным замком исходной пары.
    internal void DeleteLintel(IGeometryReadContext geometry, FamilyInstance lintel, FamilyInstance opening,
        LintelLockFacts lockFacts, ILocalizationService localization) {
        var oldId = lintel.Id;
        var allowed = geometry.GetGeometryOwners(lintel).OfType<FamilyInstance>().Select(owner => owner.Id).ToHashSet();
        if(opening is not null && lockFacts is { State: LintelLockState.Confirmed, DependencyIds.Count: 1 }
            && Document.GetElement(lockFacts.DependencyIds[0]) is Dimension { IsLocked: true } dimension
            && dimension.Category?.Id.GetIdValue() == (long) BuiltInCategory.OST_Constraints) {
            var references = dimension.References.Cast<Reference>().Select(reference => reference.ElementId).ToList();
            if(references.Count == 2 && references.Contains(oldId) && references.Contains(opening.Id)) {
                allowed.Add(dimension.Id);
            }
        }
        if(opening is not null) {
            allowed.Remove(opening.Id);
            allowed.Remove(opening.Host?.Id);
        }
        var deleted = Document.Delete(oldId);
        var unexpected = deleted.Except(allowed).Select(id => id.GetIdValue()).OrderBy(id => id).ToArray();
        if(!deleted.Contains(oldId) || unexpected.Length > 0) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.DeleteCascade",
                oldId.GetIdValue(), string.Join(", ", unexpected)));
        }
    }

    // Фиксирует исходные значения отдельно для обоих владельцев, включая незаполненные параметры.
    internal static IReadOnlyList<(long Owner, long Parameter, bool HasValue, object Value)> ReadLockParameters(
        FamilyInstance opening, FamilyInstance lintel) {
        return new[] { opening, lintel }.SelectMany(instance => instance.Parameters.Cast<Parameter>()
            .OrderBy(parameter => parameter.Id.GetIdValue()).Select(parameter =>
                ((long) instance.Id.GetIdValue(), (long) parameter.Id.GetIdValue(), parameter.HasValue,
                    parameter.HasValue ? parameter.AsObject() : null))).ToArray();
    }

    // Создаёт одну зависимость уже совмещённых центральных плоскостей в транзакции вызывающей операции.
    internal string CreateCentralLock(IGeometryReadContext geometry, FamilyInstance opening, FamilyInstance lintel,
        OpeningLintelMatch pair, ILocalizationService localization) {
        Document.Regenerate();
        var first = ResolveCenter(opening, pair.Opening.Geometry.Cut?.TargetPlanes);
        var second = ResolveCenter(lintel, pair.Lintel.Geometry.Anchor);
        if(first.Normal.CrossProduct(second.Normal).GetLength() > DirectionEpsilon
            || Math.Abs((first.Origin - second.Origin).DotProduct(first.Normal))
                > SystemPluginConfig.LockCoincidenceToleranceFeet) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.LockPlanes"));
        }
        using var views = new FilteredElementCollector(Document);
        var plan = views.OfClass(typeof(ViewPlan)).Cast<ViewPlan>().FirstOrDefault(view =>
            !view.IsTemplate && view.ViewType == ViewType.FloorPlan && view.GenLevel?.Id == opening.LevelId
            && Math.Abs(view.ViewDirection.DotProduct(first.Normal)) <= DirectionEpsilon)
            ?? throw new InvalidOperationException(localization.GetLocalizedString("Processing.LockView"));
        var dimension = Document.Create.NewAlignment(plan, first.Reference, second.Reference);
        return dimension?.UniqueId ?? throw new InvalidOperationException(localization.GetLocalizedString("Processing.LockFailed"));

        // Получает ссылки заново после регенерации, используя только факты определения семейства из кеша.
        (Reference Reference, XYZ Origin, XYZ Normal) ResolveCenter(FamilyInstance instance, AlignmentPlanes expected) {
            var ids = LintelLockReader.ReadCenterIdentities(geometry, instance, expected);
            if(ids.Length != 1 || geometry.GetWidthOriginOffset(instance.Symbol.Family) is not double offset) {
                throw new InvalidOperationException(localization.GetLocalizedString("Processing.LockReference", instance.UniqueId));
            }
            using var pose = instance.GetTransform();
            return (Reference.ParseFromStableRepresentation(Document, ids[0]),
                pose.OfPoint(new XYZ(offset, 0, 0)), pose.BasisX.Normalize());
        }
    }
}
