using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;

using dosymep.Revit;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Хранит результаты чтения одного документа, пока сессия получает события Revit.</summary>
internal sealed class ProjectReadCache : IDisposable {
    private readonly Document _document;
    private readonly Application _application;
    private readonly Dictionary<long, FamilyEntry> _families = [];
    private readonly Dictionary<long, OpeningPlaneEntry> _openingPlanes = [];
    private readonly HashSet<long> _changedIds = [];
    private readonly HashSet<long> _transactionIds = [];
    private AnnotationCatalog _annotations;

    internal ProjectReadCache(Document document) {
        _document = document;
        _application = document.Application;
        _application.DocumentChanged += OnDocumentChanged;
        try {
            _application.FamilyLoadedIntoDocument += OnFamilyLoaded;
            IsTracking = true;
        } catch {
            _application.DocumentChanged -= OnDocumentChanged;
            throw;
        }
    }

    internal bool IsTracking { get; private set; }

    private bool _rollingBackPlaneMeasurement;

    // Revit синхронно уведомляет об откате даже без изменённых элементов.
    internal TransactionStatus RollBackPlaneMeasurement(Transaction transaction) {
        _rollingBackPlaneMeasurement = true;
        try {
            return transaction.RollBack();
        } finally {
            _rollingBackPlaneMeasurement = false;
        }
    }

    internal bool IsPlaneMeasurementRollback(DocumentChangedEventArgs args) =>
        _rollingBackPlaneMeasurement && args.Operation == UndoOperation.TransactionRolledBack
        && _document.Equals(args.GetDocument());

    /// <inheritdoc />
    public void Dispose() {
        if(!IsTracking) {
            return;
        }
        IsTracking = false;
        _application.FamilyLoadedIntoDocument -= OnFamilyLoaded;
        _application.DocumentChanged -= OnDocumentChanged;
        _families.Clear();
        _openingPlanes.Clear();
        _changedIds.Clear();
        _transactionIds.Clear();
        _annotations = null;
    }

    /// <summary>Возвращает измерение именно этого экземпляра после последней правки.</summary>
    internal (string Name, double Offset)? GetOpeningPlane(FamilyInstance instance) {
        return _openingPlanes.TryGetValue(instance.Id.GetIdValue(), out var entry) && entry.Key == instance.UniqueId
            ? entry.Plane : null;
    }

    /// <summary>Сохраняет числовой результат после отката измерительной транзакции.</summary>
    internal void StoreOpeningPlane(FamilyInstance instance, (string Name, double Offset) plane) {
        _openingPlanes[instance.Id.GetIdValue()] = new OpeningPlaneEntry(instance, plane);
    }

    private sealed class OpeningPlaneEntry(FamilyInstance instance, (string Name, double Offset) plane) {
        internal readonly string Key = instance.UniqueId;
        internal readonly long FamilyId = instance.Symbol.Family.Id.GetIdValue();
        internal readonly long TypeId = instance.GetTypeId().GetIdValue();
        internal readonly long? HostId = instance.Host?.Id.GetIdValue();
        internal readonly long? HostTypeId = instance.Host?.GetTypeId().GetIdValue();
        internal readonly (string Name, double Offset) Plane = plane;
    }

    /// <summary>Читает геометрию опираний и схему параметров настроенного семейства перемычек.</summary>
    internal LintelDefinitionFacts ReadDefinition(Family family, ManagerConfigSnapshot config,
        Func<LintelDefinitionFacts> read) {
        var entry = GetFamily(family);
        if(entry.Config == config && entry.Definition is not null) {
            return entry.Definition;
        }
        var facts = read();
        entry.Definition = facts.Complete ? facts : null;
        entry.Config = config;
        return facts;
    }

    private sealed class FamilyEntry(Family family) {
        internal readonly string Key = family.UniqueId;
        internal readonly HashSet<long> Types = family.GetFamilySymbolIds()
            .Select(id => (long) id.GetIdValue()).ToHashSet();
        internal ManagerConfigSnapshot Config;
        internal LintelDefinitionFacts Definition;
    }

    private FamilyEntry GetFamily(Family family) {
        long id = family.Id.GetIdValue();
        if(!_families.TryGetValue(id, out var entry) || entry.Key != family.UniqueId) {
            _families[id] = entry = new FamilyEntry(family);
        }
        return entry;
    }

    /// <summary>Перечитывает аннотации, затронутые с предыдущего анализа, и выделяет связи текущего диапазона.</summary>
    internal LintelAnnotationIndex ReadAnnotations(ISet<long> owners) {
        if(owners.Count == 0) {
            return new LintelAnnotationIndex();
        }
        if(_annotations is null) {
            _annotations = new AnnotationCatalog(LintelAnnotationIndex.Collect(_document));
        } else if(_changedIds.Count > 0) {
            ApplyAnnotationChanges();
        }
        _changedIds.Clear();
        return _annotations.ForOwners(owners);
    }

    // Объединяет изменения нескольких транзакций, включая Undo, Redo и откат группы.
    private void OnDocumentChanged(object sender, DocumentChangedEventArgs args) {
        if(!_document.Equals(args.GetDocument()) || IsPlaneMeasurementRollback(args)) {
            return;
        }
        var changed = args.GetModifiedElementIds().Concat(args.GetDeletedElementIds()).Concat(args.GetAddedElementIds())
            .Select(id => (long) id.GetIdValue()).ToHashSet();
        // Revit может сообщить об откате группы без ID вложенных транзакций.
        if(args.Operation == UndoOperation.TransactionGroupRolledBack) {
            changed.UnionWith(_transactionIds);
        }
        _transactionIds.UnionWith(changed);
        if(_annotations is not null) {
            _changedIds.UnionWith(changed);
        }
        if(_families.Count == 0 && _openingPlanes.Count == 0) {
            return;
        }
        using var symbols = new ElementClassFilter(typeof(FamilySymbol));
        foreach(var added in args.GetAddedElementIds(symbols)) {
            if(_document.GetElement(added) is FamilySymbol symbol) {
                changed.Add(symbol.Family.Id.GetIdValue());
                _transactionIds.Add(symbol.Family.Id.GetIdValue());
            }
        }
        foreach(long id in _families.Where(pair => changed.Contains(pair.Key) || pair.Value.Types.Overlaps(changed))
            .Select(pair => pair.Key).ToArray()) {
            _families.Remove(id);
        }
        foreach(long id in _openingPlanes.Where(pair => changed.Contains(pair.Key)
                || changed.Contains(pair.Value.FamilyId) || changed.Contains(pair.Value.TypeId)
                || (pair.Value.HostId is long host && changed.Contains(host))
                || (pair.Value.HostTypeId is long type && changed.Contains(type)))
            .Select(pair => pair.Key).ToArray()) {
            _openingPlanes.Remove(id);
        }
    }

    // При перезагрузке идентификатор семейства может сохраниться или смениться.
    private void OnFamilyLoaded(object sender, FamilyLoadedIntoDocumentEventArgs args) {
        if(args.Status == RevitAPIEventStatus.Succeeded && _document.Equals(args.Document)) {
            foreach(var id in new[] { args.OriginalFamilyId, args.NewFamilyId }) {
                if(id is not null && id != ElementId.InvalidElementId) {
                    _families.Remove(id.GetIdValue());
                    foreach(long key in _openingPlanes.Where(pair => pair.Value.FamilyId == id.GetIdValue())
                        .Select(pair => pair.Key).ToArray()) {
                        _openingPlanes.Remove(key);
                    }
                    _transactionIds.Add(id.GetIdValue());
                }
            }
        }
    }

    // Удалённый ID исчезает из каталога, а у переназначенной ссылки полностью заменяется набор владельцев.
    private void ApplyAnnotationChanges() {
        var affected = _annotations.GetRelatedIds(_changedIds);
        affected.UnionWith(_changedIds);
        foreach(long id in affected) {
#if REVIT_2024_OR_GREATER
            var elementId = new ElementId(id);
#else
            var elementId = new ElementId(checked((int) id));
#endif
            var element = _document.GetElement(elementId);
            if(element is Dimension or IndependentTag || (element is not null and not ElementType
                && element.Category?.GetBuiltInCategory() == BuiltInCategory.OST_Constraints)) {
                _annotations.Replace(id, new AnnotationLinks(element));
            } else {
                _annotations.Replace(id, null);
            }
        }
    }

    // Обратный индекс позволяет выбирать аннотации владельца без обхода остальных связей документа.
    private sealed class AnnotationCatalog {
        private readonly Dictionary<long, AnnotationLinks> _byId = [];
        private readonly Dictionary<long, HashSet<long>> _byOwner = [];
        private readonly HashSet<long> _unknownLocks = [];

        internal AnnotationCatalog(IEnumerable<AnnotationLinks> annotations) {
            foreach(var item in annotations) {
                Replace(item.Id, item);
            }
        }

        internal LintelAnnotationIndex ForOwners(ISet<long> owners) =>
            LintelAnnotationIndex.ForOwners(GetRelatedIds(owners).OrderBy(id => id).Select(id => _byId[id]), owners);

        internal HashSet<long> GetRelatedIds(IEnumerable<long> owners) {
            var ids = new HashSet<long>(_unknownLocks);
            foreach(long owner in owners) {
                if(_byOwner.TryGetValue(owner, out var related)) {
                    ids.UnionWith(related);
                }
            }
            return ids;
        }

        internal void Replace(long id, AnnotationLinks current) {
            if(_byId.TryGetValue(id, out var previous)) {
                _byId.Remove(id);
                _unknownLocks.Remove(id);
                foreach(long owner in previous.Owners) {
                    var related = _byOwner[owner];
                    related.Remove(id);
                    if(related.Count == 0) {
                        _byOwner.Remove(owner);
                    }
                }
            }
            if(current is null) {
                return;
            }
            _byId.Add(id, current);
            if(current.UnknownLock) {
                _unknownLocks.Add(id);
            }
            foreach(long owner in current.Owners) {
                if(!_byOwner.TryGetValue(owner, out var related)) {
                    _byOwner.Add(owner, related = []);
                }
                related.Add(id);
            }
        }
    }
}
