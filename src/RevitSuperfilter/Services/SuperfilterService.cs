using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;
using dosymep.WPF;

using RevitSuperfilter.Models;
using RevitSuperfilter.Models.Selections;

namespace RevitSuperfilter.Services;

internal sealed class SuperfilterService : ObservableObject, ISuperfilterService {
    private readonly UIApplication _uiApplication;
    private readonly ISelectionElements _selectionElements;
    private readonly ILocalizationService _localizationService;

    private Document _document;

    public SuperfilterService(UIApplication uiApplication, ISelectionElements selectionElements, ILocalizationService localizationService) {
        _uiApplication = uiApplication;
        _selectionElements = selectionElements;
        _localizationService = localizationService;
        _selectionElements.OnSelectionChanged += SelectionElementsOnOnSelectionChanged;
    }
   
    public Selection Selection => _selectionElements.Selection;
    public string DisplaySelection => _localizationService.GetLocalizedString($"{Selection.GetType().Name}.{_selectionElements.Selection}");


    public Superfilter Superfilter { get; } = new();
    public ElementsIndex ElementsIndex { get; } = new();

    public ISuperfilterService Build() {
        Clear();
        _document = GetActiveDocument();

        IReadOnlyCollection<Element> elements = _selectionElements
            .GetElements()
            .ToArray();

        Superfilter.Build(elements);
        //ElementsIndex.Build(elements);
        
        return this;
    }

    public void Clear() {
        Superfilter.Clear();
        ElementsIndex.Clear();
    }

    public void Add(Element element) {
        Superfilter.Add(element);
        ElementsIndex.Add(element);
    }

    public void Remove(ElementId elementId) {
        Superfilter.Remove(elementId);
        ElementsIndex.Remove(elementId);
    }

    private Document GetActiveDocument() {
        return _uiApplication.ActiveUIDocument.Document;
    }

    private void AddElements(SelectionChangeEventArgs e) {
        var addedElements = e.AddedElementIds
            .Select(item => _document.GetElement(item));

        foreach(var addedElement in addedElements) {
            Add(addedElement);
        }
    }

    private void RemoveElements(SelectionChangeEventArgs e) {
        foreach(var removedElement in e.RemovedElementIds) {
            Remove(removedElement);
        }
    }

    private void ReplaceElements(SelectionChangeEventArgs e) {
        var modifiedElements = e.ModifiedElementIds
            .Select(item => _document.GetElement(item));

        foreach(var modifiedElement in modifiedElements) {
            Add(modifiedElement);
        }
    }
    
    private void SelectionElementsOnOnSelectionChanged(object sender, SelectionChangeEventArgs e) {
        if(e.IsEmpty) {
            Build();
            return;
        }
        
        AddElements(e);
        RemoveElements(e);
        ReplaceElements(e);
    }
}
