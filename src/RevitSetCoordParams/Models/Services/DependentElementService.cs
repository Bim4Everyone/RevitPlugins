using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Services;

internal class DependentElementService : IDependentElementService {
    private readonly Document _document;

    public DependentElementService(Document document) {
        _document = document;
        BuildDependencyMap(_document);
    }

    public Dictionary<ElementId, List<ElementId>> DependentMap { get; private set; }

    /// <summary>
    /// Проверяет, является ли элемент корневым.
    /// </summary>
    public bool IsRootElement(Element element) {
        return element is not InsulationLiningBase
            && (element is not FamilyInstance fi || fi.SuperComponent == null);
    }

    /// <summary>
    /// Возвращает корневой элемент для вложенного семейства
    /// или зависимого элемента (изоляции).
    /// </summary>
    public Element GetRootElement(Element element) {
        if(element == null) {
            return null;
        }

        var current = element;
        var visited = new HashSet<ElementId>();

        while(current != null && visited.Add(current.Id)) {
            switch(current) {
                case FamilyInstance fi when fi.SuperComponent != null:
                    current = fi.SuperComponent;
                    continue;

                case InsulationLiningBase insulation:
                    var host = _document.GetElement(insulation.HostElementId);

                    if(host != null) {
                        current = host;
                        continue;
                    }

                    break;
            }

            break;
        }

        return current;
    }

    /// <summary>
    /// Строит карту зависимостей:
    /// Родитель -> список непосредственных потомков.
    /// </summary>
    private void BuildDependencyMap(Document document) {
        DependentMap = [];

        var collector = new FilteredElementCollector(document)
            .WhereElementIsNotElementType();

        foreach(var element in collector) {
            var parentId = GetParentId(element);

            AddDependency(parentId, element.Id);
        }
    }

    /// <summary>
    /// Возвращает непосредственного родителя элемента.
    /// </summary>
    private static ElementId GetParentId(Element element) {
        return element switch {
            FamilyInstance { SuperComponent: not null } fi => fi.SuperComponent.Id,
            InsulationLiningBase insulation => insulation.HostElementId,
            _ => ElementId.InvalidElementId
        };
    }

    /// <summary>
    /// Добавляет связь родитель -> потомок.
    /// </summary>
    private void AddDependency(ElementId parentId, ElementId childId) {
        if(parentId == null || parentId == ElementId.InvalidElementId) {
            return;
        }

        if(!DependentMap.TryGetValue(parentId, out var children)) {
            children = [];
            DependentMap[parentId] = children;
        }

        children.Add(childId);
    }
}
