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

    // Сохраняет идентификаторы выбранного диапазона на время одного Execute.
    public string[] CaptureSelectedElements() => ActiveUIDocument.Selection.GetElementIds()
        .Select(id => Document.GetElement(id)?.UniqueId).Where(id => id is not null).ToArray();

    // Revit может очистить выделение при откате. Возвращает уцелевшие элементы и новые экземпляры замен.
    public void RestoreBatchSelection(IEnumerable<string> original, ProcessingBatchResult batch) {
        if(!IsSourceDocumentActive(UIApplication)) {
            return;
        }
        var replacements = batch.Operations.Where(row => row.Status == ProcessingStatus.Succeeded
            && row.Operation.Action is ManagerAction.Replace or ManagerAction.Delete)
            .ToDictionary(row => row.Operation.Key.LintelUniqueId,
                row => row.InstalledLintelUniqueId ?? row.Operation.Key.OpeningUniqueId);
        var ids = original.Select(id => replacements.TryGetValue(id, out string replacement) ? replacement : id)
            .Where(id => id is not null).Select(id => Document.GetElement(id)?.Id)
            .Where(id => id is not null).ToHashSet();
        var selection = ActiveUIDocument.Selection;
        if(!ids.SetEquals(selection.GetElementIds())) {
            selection.SetElementIds(ids);
        }
    }

    // Получает стадию из неизменяемой конфигурации текущей сессии.
    public ElementId GetReadPhaseId(ManagerConfigSnapshot config) {
        if(Document.GetElement(config.Phase.UniqueId) is not Phase phase) {
            throw new InvalidOperationException("Настроенная стадия удалена из исходного документа.");
        }
        return phase.Id;
    }

    // Привязывает одноуровневое семейство к уровню проёма и ориентирует по физической оси стены.
    internal FamilyInstance PlaceLintel(LintelCalculation calculation, FamilyInstance opening, IGeometryReadContext geometry,
        ILocalizationService localization) {
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
        var direction = AsXyz(target.CenterNormal);
        var center = GetAlignedCenter(geometry, opening, symbol.Family, target, direction);
        var instance = Document.Create.NewFamilyInstance(center, symbol, level, StructuralType.NonStructural)
            ?? throw new InvalidOperationException(localization.GetLocalizedString("Processing.InstallType"));
        Document.Regenerate();
        using var pose = instance.GetTransform();
        RotateLintel(instance, pose, direction);
        var displacement = center - pose.Origin;
        if(displacement.GetLength() > LengthEpsilon) {
            ElementTransformUtils.MoveElement(Document, instance.Id, displacement);
        }
        return instance;
    }

    // В пределах точности измерения выреза берёт продольную координату из опорной плоскости семейства.
    internal static XYZ GetAlignedCenter(IGeometryReadContext geometry, FamilyInstance opening, Family lintelFamily,
        AlignmentPlanes planes, XYZ lintelDirection) {
        var center = AsXyz(planes.Center);
        if(geometry.GetWidthPlane(opening) is not { } openingPlaneFacts
            || geometry.GetWidthOriginOffset(lintelFamily) is not double lintelOffset) {
            return center;
        }
        using var pose = opening.GetTransform();
        var normal = pose.BasisX.Normalize();
        var openingPlane = pose.OfPoint(new XYZ(openingPlaneFacts.Offset, 0, 0));
        var lintelPlane = center + lintelDirection * lintelOffset;
        double gap = (openingPlane - lintelPlane).DotProduct(normal);
        return Math.Abs(gap) <= LengthEpsilon ? center + normal * gap : center;
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

    // Сначала удаляет марки и размеры, включая замки, затем старую перемычку в той же транзакции.
    internal void DeleteLintel(IGeometryReadContext geometry, FamilyInstance lintel, FamilyInstance opening,
        ILocalizationService localization) {
        var oldId = lintel.Id;
        var owners = PrepareLintelForChange(geometry, lintel, opening, localization);
        var deleted = Document.Delete(oldId);
        CheckCascade(deleted, owners, oldId, localization);
        if(!deleted.Contains(oldId)) {
            throw new InvalidOperationException(localization.GetLocalizedString("Processing.DeletePair"));
        }
    }

    // Удаляет относящиеся к перемычке размеры, геометрические замки и марки.
    internal HashSet<ElementId> PrepareLintelForChange(IGeometryReadContext geometry, FamilyInstance lintel,
        FamilyInstance opening, ILocalizationService localization) {
        var owners = geometry.GetGeometryOwners(lintel).OfType<FamilyInstance>().Select(owner => owner.Id).ToHashSet();
        if(opening is not null) {
            owners.Remove(opening.Id);
            owners.Remove(opening.Host?.Id);
        }
        var annotations = FindLintelAnnotations(owners, geometry.ProcessingData.Annotations);
        if(annotations.Count > 0) {
            var deleted = Document.Delete(annotations);
            CheckCascade(deleted, annotations, lintel.Id, localization);
            if(!annotations.SetEquals(deleted)) {
                throw new InvalidOperationException(localization.GetLocalizedString("Processing.AnnotationRemoval", lintel.Id.GetIdValue()));
            }
        }
        return owners;
    }

    // Учитывает зависимые аннотации и ссылки с любых видов, включая марки нескольких элементов и цепочки размеров.
    private HashSet<ElementId> FindLintelAnnotations(HashSet<ElementId> owners, LintelAnnotationIndex index) {
        using var dimensions = new ElementClassFilter(typeof(Dimension));
        using var tags = new ElementClassFilter(typeof(IndependentTag));
        using var filter = new LogicalOrFilter(dimensions, tags);
        var result = owners.SelectMany(id => Document.GetElement(id).GetDependentElements(filter)).ToHashSet();
        var ownerIds = owners.Select(id => (long) id.GetIdValue()).ToHashSet();
        index ??= LintelAnnotationIndex.Read(Document, ownerIds);
        foreach(var item in index.GetElements(Document, ownerIds, false)) {
            if(!result.Contains(item.Id)
                && LintelAnnotationIndex.ReadOwners(item)?.OfType<long>().Any(ownerIds.Contains) == true) {
                result.Add(item.Id);
            }
        }
        return result;
    }

    // Любой удалённый Revit элемент за пределами согласованного набора вызывает откат строки.
    private static void CheckCascade(ICollection<ElementId> deleted, HashSet<ElementId> allowed, ElementId oldId,
        ILocalizationService localization) {
        var unexpected = deleted.Except(allowed).Select(id => id.GetIdValue()).OrderBy(id => id).ToArray();
        if(unexpected.Length > 0) {
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
        var plans = geometry.ProcessingData.FloorPlans ?? ReadFloorPlans();
        var plan = plans[opening.LevelId.GetIdValue()].Select(id => Document.GetElement(id)).OfType<ViewPlan>()
            .FirstOrDefault(view => !view.IsTemplate && view.ViewType == ViewType.FloorPlan
                && view.GenLevel?.Id == opening.LevelId && Math.Abs(view.ViewDirection.DotProduct(first.Normal)) <= DirectionEpsilon)
            ?? throw new InvalidOperationException(localization.GetLocalizedString("Processing.LockView"));
        var dimension = Document.Create.NewAlignment(plan, first.Reference, second.Reference);
        return dimension?.UniqueId ?? throw new InvalidOperationException(localization.GetLocalizedString("Processing.LockFailed"));

        // Получает ссылки заново после регенерации и берёт локальное смещение из снимка.
        (Reference Reference, XYZ Origin, XYZ Normal) ResolveCenter(FamilyInstance instance, AlignmentPlanes expected) {
            var ids = LintelLockReader.ReadCenterIdentities(geometry, instance, expected);
            if(ids.Length != 1 || geometry.GetWidthPlane(instance) is not { } width) {
                throw new InvalidOperationException(localization.GetLocalizedString("Processing.LockReference", instance.UniqueId));
            }
            using var pose = instance.GetTransform();
            return (Reference.ParseFromStableRepresentation(Document, ids[0]),
                pose.OfPoint(new XYZ(width.Offset, 0, 0)), pose.BasisX.Normalize());
        }
    }

    /// <summary>Запоминает планы этажей по уровню для создания замков всего диапазона.</summary>
    internal ILookup<long, string> ReadFloorPlans() {
        using var views = new FilteredElementCollector(Document);
        return views.OfClass(typeof(ViewPlan)).Cast<ViewPlan>()
            .Where(view => !view.IsTemplate && view.ViewType == ViewType.FloorPlan && view.GenLevel is not null)
            .ToLookup(view => (long) view.GenLevel.Id.GetIdValue(), view => view.UniqueId);
    }
}
