using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSuperfilter.Comparators;
using RevitSuperfilter.Extensions;
using RevitSuperfilter.Services;

namespace RevitSuperfilter.Models;

internal sealed class ElementsIndex : IElementIndex {
    private readonly Dictionary<ElementId, Element> _elementsById = new();
    private readonly Dictionary<ElementIndex, HashSet<Element>> _indexElements = new();

    public void Build(IEnumerable<Element> elements) {
        foreach(var element in elements) {
            Add(element);
        }
    }

    public void Clear() {
        _elementsById.Clear();
        _indexElements.Clear();
    }
    
    public IEnumerable<Element> GetElements(Category category, Definition definition, string paramValue) {
        var key = new ElementIndex(category.Name, definition.Name, paramValue);
        return _indexElements.TryGetValue(key, out var elements) ? elements : Enumerable.Empty<Element>();
    }

    public void Add(Element element) {
        if(_elementsById.ContainsKey(element.Id)) {
            Remove(element.Id);
        }

        _elementsById.Add(element.Id, element);
        foreach(var param in element.Parameters.OfType<Parameter>()) {
            var index = new ElementIndex(element.Category?.Name, param.Definition.Name, param.GetValueOrDefault());

            if(!_indexElements.TryGetValue(index, out var elements)) {
                elements = new HashSet<Element>(ElementIdComparer.Instance);
                _indexElements.Add(index, elements);
            }

            elements.Add(element);
        }
    }

    public void Remove(ElementId elementId) {
        if(!_elementsById.TryGetValue(elementId, out Element element)) {
            return;
        }

        _elementsById.Remove(elementId);

        foreach(var param in element.Parameters.OfType<Parameter>()) {
            var index = new ElementIndex(element.Category?.Name, param.Definition.Name, param.GetValueOrDefault());

            if(_indexElements.TryGetValue(index, out var elements)) {
                elements.Remove(element);
                if(elements.Count == 0) {
                    _indexElements.Remove(index);
                }
            }
        }
    }
}
