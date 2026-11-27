using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using static RevitLintelsManager.Models.RevitElementRead;

namespace RevitLintelsManager.Models;

/// <summary>Связи размеров, замков и марок с владельцами выбранного диапазона на всех видах.</summary>
internal sealed class LintelAnnotationIndex {
    private readonly Dictionary<long, List<string>> _byOwner = [];
    private readonly List<string> _unknownLocks = [];

    /// <summary>Читает ссылки аннотаций один раз, включая ограничения с неизвестными владельцами.</summary>
    internal static LintelAnnotationIndex Read(Document document, ISet<long> owners) =>
        owners.Count == 0 ? new LintelAnnotationIndex() : ForOwners(Collect(document), owners);

    /// <summary>Собирает аннотации документа для первого чтения, включая аннотации за пределами текущего диапазона.</summary>
    internal static IEnumerable<AnnotationLinks> Collect(Document document) {
        using var dimensions = new ElementClassFilter(typeof(Dimension));
        using var tags = new ElementClassFilter(typeof(IndependentTag));
        using var constraints = new ElementCategoryFilter(BuiltInCategory.OST_Constraints);
        using var filter = new LogicalOrFilter(new List<ElementFilter> { dimensions, tags, constraints });
        using var collector = new FilteredElementCollector(document);
        foreach(var annotation in collector.WherePasses(filter).WhereElementIsNotElementType()) {
            yield return new AnnotationLinks(annotation);
        }
    }

    /// <summary>Формирует независимый снимок связей только для запрошенных владельцев.</summary>
    internal static LintelAnnotationIndex ForOwners(IEnumerable<AnnotationLinks> annotations, ISet<long> owners) {
        var index = new LintelAnnotationIndex();
        foreach(var annotation in annotations) {
            if(annotation.UnknownLock) {
                index._unknownLocks.Add(annotation.UniqueId);
            }
            foreach(long owner in annotation.Owners) {
                if(!owners.Contains(owner)) {
                    continue;
                }
                if(!index._byOwner.TryGetValue(owner, out var related)) {
                    index._byOwner.Add(owner, related = []);
                }
                related.Add(annotation.UniqueId);
            }
        }
        return index;
    }

    /// <summary>Возвращает существующие аннотации по устойчивым идентификаторам снимка.</summary>
    internal IEnumerable<Element> GetElements(Document document, IEnumerable<long> owners, bool includeUnknownLocks) {
        var ids = owners.Where(_byOwner.ContainsKey).SelectMany(id => _byOwner[id]);
        if(includeUnknownLocks) {
            ids = ids.Concat(_unknownLocks);
        }
        return ids.Distinct().Select(id => document.GetElement(id)).Where(element => element is not null);
    }

    /// <summary>Читает локальных владельцев, оставляя неопределённую ссылку для диагностики замка.</summary>
    internal static long?[] ReadOwners(Element element) => ReadOptional(() => element switch {
        IndependentTag tag => tag.GetTaggedLocalElementIds().Select(id => (long?) id.GetIdValue()).ToArray(),
        Dimension dimension => dimension.References?.Cast<Reference>().Select(reference =>
            reference?.ElementId is { } id && id != ElementId.InvalidElementId ? (long?) id.GetIdValue() : null)
            .ToArray(),
        _ => null
    });
}

/// <summary>Локальные владельцы марки или размера, включая неполные ссылки ограничений.</summary>
internal sealed class AnnotationLinks {
    internal AnnotationLinks(Element annotation) {
        Id = annotation.Id.GetIdValue();
        UniqueId = annotation.UniqueId;
        var ids = LintelAnnotationIndex.ReadOwners(annotation);
        Owners = (ids ?? []).OfType<long>().Distinct().ToArray();
        UnknownLock = annotation is not IndependentTag && (ids is null || ids.Length == 0 || ids.Contains(null));
    }

    internal long Id { get; }
    internal string UniqueId { get; }
    internal long[] Owners { get; }
    internal bool UnknownLock { get; }
}
