using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using static RevitLintelsManager.Models.RevitElementRead;
using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Models;

/// <summary>Читает центральные связи актуальных экземпляров в пределах исходного документа.</summary>
internal sealed class LintelLockReader(RevitRepository source) : ILintelLockReader {
    /// <inheritdoc />
    public IReadOnlyList<OpeningLintelMatch> ReadLocks(IGeometryReadContext geometry,
        IReadOnlyList<OpeningLintelMatch> matches) {
        if(!matches.Any(IsPair)) {
            return matches;
        }
        if(!source.IsSourceDocumentActive(source.UIApplication) || !source.Document.Equals(geometry.Document)) {
            var stale = new LintelLockFacts(LintelLockState.Stale, LintelLockReason.SourceDocumentUnavailable);
            return matches.Select(row => IsPair(row) ? new OpeningLintelMatch(row, stale) : row).ToList().AsReadOnly();
        }
        var index = new Lazy<LintelLockIndex>(() => ReadIndex(geometry.Document, matches.Where(IsPair)
            .SelectMany(row => new[] {
                row.Opening.Element.Information?.ElementId, row.Lintel.Element.Information?.ElementId
            })
            .OfType<long>().ToHashSet()));
        return matches.Select(row => IsPair(row) ? new OpeningLintelMatch(row, ReadPair(geometry, row, index)) : row)
            .ToList().AsReadOnly();
    }

    // Учитывает только строки с двумя идентифицированными экземплярами.
    private static bool IsPair(OpeningLintelMatch match) {
        return match.Opening is not null && match.Lintel is not null;
    }

    // Переопределяет идентичность экземпляров и центральных ссылок для каждого поколения чтения.
    private static LintelLockFacts ReadPair(IGeometryReadContext geometry, OpeningLintelMatch match,
        Lazy<LintelLockIndex> index) {
        try {
            var opening = ResolveInstance(geometry.Document, match.Opening.Element, match.Opening.Geometry);
            var lintel = ResolveInstance(geometry.Document, match.Lintel.Element, match.Lintel.Geometry);
            if(opening is null || lintel is null || opening.Id == lintel.Id
                || opening.Host?.UniqueId != match.Opening.HostWallUniqueId) {
                return new LintelLockFacts(LintelLockState.Stale, LintelLockReason.PairChanged);
            }
            var (openingRef, openingIssue) = ReadCenter(geometry, opening, match.Opening.Geometry?.Cut?.TargetPlanes,
                LintelLockReason.OpeningReferenceUnavailable);
            if(openingIssue is not null) {
                return openingIssue;
            }
            var lintelGeometry = match.Lintel.Geometry;
            var (lintelRef, lintelIssue) = ReadCenter(geometry, lintel,
                lintelGeometry?.Anchor ?? lintelGeometry?.Placement?.TransformAxes,
                LintelLockReason.LintelReferenceUnavailable);
            return lintelIssue ?? index.Value.Assess(openingRef, lintelRef);
        } catch(Exception exception) when(IsElementReadException(exception)) {
            return new LintelLockFacts(LintelLockState.Unreadable, LintelLockReason.DependencyUnreadable);
        }
    }

    // Проверяет тип, числовой идентификатор и изменение положения от момента съёмки пары.
    private static FamilyInstance ResolveInstance(Document document, ManagerFamilySnapshot snapshot,
        GeometryFacts geometry) {
        if(string.IsNullOrEmpty(snapshot.UniqueId) || snapshot.Information?.ElementId is not long id
            || document.GetElement(snapshot.UniqueId) is not FamilyInstance { IsValidObject: true } instance
            || instance.Symbol.UniqueId != snapshot.TypeUniqueId
            || instance.Symbol.Family.UniqueId != snapshot.FamilyUniqueId
            || instance.Id.GetIdValue() != id) {
            return null;
        }
        if(geometry?.Placement?.TransformAxes is not { } axes) {
            return instance;
        }
        using var pose = instance.GetTransform();
        return pose.Origin.DistanceTo(AsXyz(axes.Center)) <= LengthEpsilon
            && pose.BasisX.Normalize().IsAlmostEqualTo(AsXyz(axes.CenterNormal))
            && pose.BasisY.Normalize().IsAlmostEqualTo(AsXyz(axes.LongitudinalNormal))
            && pose.BasisZ.Normalize().IsAlmostEqualTo(AsXyz(axes.HeightNormal)) ? instance : null;
    }

    // Разрешает имя по определению семейства и проверяет, что плоскость проходит через фактический центр.
    private static (LintelLockReference Reference, LintelLockFacts Issue) ReadCenter(IGeometryReadContext geometry,
        FamilyInstance instance, AlignmentPlanes expected, LintelLockReason unavailable) {
        var matching = ReadOptional(() => ReadCenterIdentities(geometry, instance, expected));
        var issue = GetCenterIssue(matching, unavailable);
        return issue is null ? (new LintelLockReference(instance.Id.GetIdValue(), matching[0]), null) : (null, issue);
    }

    // После проверки семантики принимает одну идентичность. Конфликт возвращается без выбора ссылки.
    private static LintelLockFacts GetCenterIssue(string[] matching, LintelLockReason unavailable) {
        return matching?.Length switch {
            1 when !string.IsNullOrEmpty(matching[0]) => null,
            > 1 => new LintelLockFacts(LintelLockState.Ambiguous, LintelLockReason.MultipleCentralReferences),
            _ => new LintelLockFacts(LintelLockState.ReferencesUnavailable, unavailable)
        };
    }

    // Сохраняет только разные устойчивые представления подтверждённой плоскости актуального экземпляра.
    private static string[] ReadCenterIdentities(IGeometryReadContext geometry, FamilyInstance instance,
        AlignmentPlanes expected) {
        if(expected is null || geometry.GetWidthOriginPlane(instance.Symbol.Family) is not string planeName) {
            return [];
        }
        using var pose = instance.GetTransform();
        var normal = pose.BasisX.Normalize();
        var center = AsXyz(expected.Center);
        var expectedNormal = AsXyz(expected.CenterNormal);
        if(!pose.IsConformal || !IsFinitePoint(center) || !IsFinitePoint(expectedNormal)
            || expectedNormal.IsZeroLength() || normal.CrossProduct(expectedNormal).GetLength() > DirectionEpsilon
            || Math.Abs((center - pose.Origin).DotProduct(normal)) > LengthEpsilon) {
            return [];
        }
        var candidates = instance.GetReferences(FamilyInstanceReferenceType.CenterLeftRight) ?? [];
        return candidates.Where(reference => reference is not null && reference.ElementId == instance.Id
            && reference.LinkedElementId == ElementId.InvalidElementId
            && instance.GetReferenceType(reference) == FamilyInstanceReferenceType.CenterLeftRight
            && instance.GetReferenceName(reference) == planeName)
            .Select(reference => reference.ConvertToStableRepresentation(geometry.Document))
            .Distinct(StringComparer.Ordinal).Take(2).ToArray();
    }

    // Индексирует размеры и категорию ограничений вне зависимости от видимости текущего вида.
    private static LintelLockIndex ReadIndex(Document document, ISet<long> ownerIds) {
        var items = new List<LintelLockDependency>();
        try {
            using var byType = new ElementClassFilter(typeof(Dimension));
            using var byCategory = new ElementCategoryFilter(BuiltInCategory.OST_Constraints);
            using var filter = new LogicalOrFilter(byType, byCategory);
            using var collector = new FilteredElementCollector(document);
            foreach(var element in collector.WherePasses(filter).WhereElementIsNotElementType()) {
                if(ReadOptional<bool?>(() => element.IsTransient) == true) {
                    continue;
                }
                var facts = ReadDependency(document, element, ownerIds);
                if(facts is not null) {
                    items.Add(facts);
                }
            }
        } catch(Exception exception) when(IsElementReadException(exception)) {
            items.Add(new LintelLockDependency(null, LintelLockDependencyKind.Direct, null, false, null));
        }
        return new LintelLockIndex(items);
    }

    // Читает признаки независимо, чтобы ошибка блокировки сохранила известные идентификаторы владельцев.
    private static LintelLockDependency ReadDependency(Document document, Element element, ISet<long> ownerIds) {
        string id = ReadOptional(() => element.UniqueId);
        if(element is not Dimension dimension) {
            return new LintelLockDependency(id, LintelLockDependencyKind.Unsupported, null, false, null);
        }
        var references = ReadOptional(() => dimension.References?.Cast<Reference>()
            .Select(reference => ReadReference(document, reference, ownerIds)).ToArray());
        if(references is { Length: > 0 }
            && references.All(reference => reference.OwnerId.HasValue && !ownerIds.Contains(reference.OwnerId.Value))) {
            return null;
        }
        int? segments = ReadOptional<int?>(() => dimension.NumberOfSegments);
        bool? locked = ReadOptional<bool?>(() => segments > 1
            ? dimension.AreSegmentsEqual || dimension.Segments.Cast<DimensionSegment>().Any(segment => segment.IsLocked)
            : dimension.IsLocked);
        if(references is null || references.Length == 0) {
            return locked == false ? null
                : new LintelLockDependency(id, LintelLockDependencyKind.Direct, locked, false, null);
        }
        var kind = ReadOptional<LintelLockDependencyKind?>(() => GetKind(dimension, segments, references.Length))
            ?? LintelLockDependencyKind.Unsupported;
        bool resolved = ReadOptional(() => dimension.AreReferencesAvailable);
        return new LintelLockDependency(id, kind, locked, resolved, references);
    }

    // У настоящего выравнивания Revit форма может быть Unknown. Линейная цепочка требует отдельной интерпретации.
    private static LintelLockDependencyKind GetKind(Dimension dimension, int? segments, int referencesCount) {
        var shape = dimension.DimensionShape;
        bool isConstraint = dimension.Category?.GetBuiltInCategory() == BuiltInCategory.OST_Constraints;
        if(shape != DimensionShape.Linear && !(shape == DimensionShape.Unknown && isConstraint)) {
            return LintelLockDependencyKind.Other;
        }
        return referencesCount == 2 && segments <= 1 ? LintelLockDependencyKind.Direct : LintelLockDependencyKind.Chain;
    }

    // Сохраняет числового владельца при недоступном stable representation.
    private static LintelLockReference ReadReference(Document document, Reference reference, ISet<long> ownerIds) {
        long? owner = ReadOptional<long?>(() => reference is null || reference.ElementId == ElementId.InvalidElementId
            ? null : reference.ElementId.GetIdValue());
        string representation = owner.HasValue && ownerIds.Contains(owner.Value)
            ? ReadOptional(() => reference.ConvertToStableRepresentation(document)) : null;
        return new LintelLockReference(owner, representation);
    }
}
