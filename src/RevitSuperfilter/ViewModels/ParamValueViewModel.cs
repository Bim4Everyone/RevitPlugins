using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels;

internal sealed class ParamValueViewModel : BaseViewModel {
    private readonly Dictionary<ElementId, Element> _elementsById = new();
    
    public int Count => _elementsById.Count;
    public string DisplayValue { get; }

    public ParamValueViewModel(string displayValue) {
        DisplayValue = displayValue;
    }

    public void Add(Element element) {
        if(_elementsById.ContainsKey(element.Id)) {
            Remove(element.Id);
        }
        
        _elementsById.Add(element.Id, element);
        OnPropertyChanged(nameof(Count));
    }

    public void Remove(ElementId elementId) {
        _elementsById.Remove(elementId);
        OnPropertyChanged(nameof(Count));
    }
}
