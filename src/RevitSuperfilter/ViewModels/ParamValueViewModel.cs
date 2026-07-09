using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels;

internal sealed class ParamValueViewModel : BaseViewModel {
    private readonly HashSet<ElementId> _elementsById = [];
    private bool _isSelected;

    public int Count => _elementsById.Count;
    public string DisplayValue { get; }

    public ParamValueViewModel(string displayValue) {
        DisplayValue = displayValue;
    }

    public bool IsSelected {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public ICollection<ElementId> GetElementIds() {
        return [.._elementsById];
    }

    public void Add(Element element) {
        if(_elementsById.Contains(element.Id)) {
            Remove(element.Id);
        }

        _elementsById.Add(element.Id);
        OnPropertyChanged(nameof(Count));
    }

    public void Remove(ElementId elementId) {
        _elementsById.Remove(elementId);
        OnPropertyChanged(nameof(Count));
    }
}
