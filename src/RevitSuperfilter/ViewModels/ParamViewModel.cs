using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels;

internal sealed class ParamViewModel : BaseViewModel {
    private readonly Dictionary<ElementId, Element> _elementsById = new();
    private readonly Dictionary<string, ParamValueViewModel> _paramValues = new(StringComparer.CurrentCulture);

    private readonly Definition _definition;

    public ParamViewModel(Definition definition) {
        _definition = definition;
    }

    public int Count => _elementsById.Count;
    public string DisplayValue => _definition.Name;
    
    public ObservableCollection<ParamValueViewModel> ParamValues { get; } = [];

    public void Add(Element element, string paramValue) {
        if(_elementsById.ContainsKey(element.Id)) {
            Remove(element.Id, paramValue);
        }

        _elementsById.Add(element.Id, element);
        var viewModel = GetOrAdd(element, paramValue);
        viewModel.Add(element);
        OnPropertyChanged(nameof(Count));
    }

    public void Remove(ElementId elementId, string paramValue) {
        if(!_elementsById.Remove(elementId)) {
            return;
        }

        if(_paramValues.TryGetValue(paramValue, out var paramValueViewModel)) {
            _paramValues.Remove(paramValue);
            ParamValues.Remove(paramValueViewModel);
        }
        OnPropertyChanged(nameof(Count));
    }

    private ParamValueViewModel GetOrAdd(Element element, string value) {
        if(!_paramValues.TryGetValue(value ?? "<null>", out var paramValueViewModel)) {
            paramValueViewModel = new ParamValueViewModel(value);

            ParamValues.Add(paramValueViewModel);
            _paramValues.Add(value ?? "<null>", paramValueViewModel);
        }

        return paramValueViewModel;
    }
}
