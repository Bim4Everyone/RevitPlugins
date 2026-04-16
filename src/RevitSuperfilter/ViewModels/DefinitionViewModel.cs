using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels;

internal sealed class DefinitionViewModel : BaseViewModel {
    private readonly Dictionary<ElementId, Element> _elementsById = new();
    private readonly Dictionary<ElementId, string> _values = new();
    private readonly Dictionary<string, ParamValueViewModel> _paramValues = new(StringComparer.CurrentCulture);

    private readonly Definition _definition;

    public DefinitionViewModel(Definition definition) {
        _definition = definition;
    }

    public int Count => _elementsById.Count;
    public string DisplayValue => _definition.Name;

    public ObservableCollection<ParamValueViewModel> ParamValues { get; } = [];

    public void Add(Element element, string paramValue) {
        if(_elementsById.ContainsKey(element.Id)) {
            Remove(element.Id);
        }

        _values.Add(element.Id, paramValue);
        _elementsById.Add(element.Id, element);
        
        var viewModel = GetOrAdd(element, paramValue);
        viewModel.Add(element);
        
        OnPropertyChanged(nameof(Count));
    }

    public void Remove(ElementId elementId) {
        if(!_elementsById.Remove(elementId)) {
            return;
        }

        if(_values.TryGetValue(elementId, out string value)) {
            _values.Remove(elementId);
            if(_paramValues.TryGetValue(GetKey(value), out var paramValueViewModel)) {
                paramValueViewModel.Remove(elementId);

                if(paramValueViewModel.Count == 0) {
                    _paramValues.Remove(GetKey(value));
                    ParamValues.Remove(paramValueViewModel);
                }
            }
        }

        OnPropertyChanged(nameof(Count));
    }

    private ParamValueViewModel GetOrAdd(Element element, string value) {
        if(!_paramValues.TryGetValue(GetKey(value), out var paramValueViewModel)) {
            paramValueViewModel = new ParamValueViewModel(value);

            ParamValues.Add(paramValueViewModel);
            _paramValues.Add(GetKey(value), paramValueViewModel);
        }

        return paramValueViewModel;
    }

    private string GetKey(string value) {
        if(value is null) {
            return "<null>";
        }

        if(value.Equals(string.Empty)) {
            return "<empty>";
        }

        return value;
    }
}
