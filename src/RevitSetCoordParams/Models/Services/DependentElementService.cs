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

    public bool IsRootElement(Element element) {
        return element is not InsulationLiningBase && (element is not FamilyInstance fi || fi.SuperComponent == null);
    }

    // Метод построения карты зависимостей вложенных/зависимых элементов
    private void BuildDependencyMap(Document document) {
        DependentMap = [];

        var collector = new FilteredElementCollector(document)
            .WhereElementIsNotElementType();

        foreach(var element in collector) {
            var parentId = ElementId.InvalidElementId;

            if(element is FamilyInstance fi && fi.SuperComponent != null) {
                parentId = fi.SuperComponent.Id;
            } else if(element is InsulationLiningBase insulation) {
                parentId = insulation.HostElementId;
            }

            AddDependency(parentId, element.Id);
        }
    }

    // Метод добавления вложенных/зависимых ID
    private void AddDependency(ElementId parentId, ElementId childId) {
        if(parentId == null || parentId == ElementId.InvalidElementId) {
            return;
        }
        if(!DependentMap.TryGetValue(parentId, out var list)) {
            list = [];
            DependentMap[parentId] = list;
        }
        list.Add(childId);
    }
}
