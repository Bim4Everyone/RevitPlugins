using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels;

internal sealed class DefinitionViewModel : BaseViewModel {
    private readonly Dictionary<ElementId, Element> _elementsById = new();
    private readonly Dictionary<ElementId, string> _values = new();
    private readonly Dictionary<string, ParamValueViewModel> _paramValues = new(StringComparer.CurrentCulture);

    private bool _isExpanded;
    private bool _isSelected;
    private bool? _isChecked = false;
    private bool _isCascading;
    private readonly Definition _definition;

    public DefinitionViewModel(Definition definition, bool isType) {
        IsType = isType;
        _definition = definition;

        PropertyChanged += OnPropertyChanged;
    }
    
    public bool IsType { get; }

    public int Count => ParamValues.Count;
    public string DisplayValue => _definition.Name;

    public bool IsSelected {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public bool IsExpanded {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public bool? IsChecked {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }

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
                    paramValueViewModel.PropertyChanged -= OnParamValueChanged;
                    IsChecked = GetAggregateCheckState(ParamValues);
                }
            }
        }

        OnPropertyChanged(nameof(Count));
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName != nameof(IsChecked) || _isCascading) {
            return;
        }

        if(IsChecked == null) {
            return;
        }

        _isCascading = true;
        try {
            foreach(var paramValue in ParamValues) {
                paramValue.IsChecked = (bool) IsChecked;
            }
        } finally {
            _isCascading = false;
        }
    }

    private bool? GetAggregateCheckState(IEnumerable<ParamValueViewModel> paramValues) {
        bool? result = false;
        bool isFirst = true;

        foreach(var paramValue in paramValues) {
            bool? state = paramValue.IsChecked;
            if(isFirst) {
                result = state;
                isFirst = false;
                continue;
            }

            if(result != state) {
                return null;
            }
        }

        return result;
    }

    private void OnParamValueChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName != nameof(ParamValueViewModel.IsChecked) || _isCascading) {
            return;
        }

        IsChecked = GetAggregateCheckState(ParamValues);
    }

    private ParamValueViewModel GetOrAdd(Element element, string value) {
        if(!_paramValues.TryGetValue(GetKey(value), out var paramValueViewModel)) {
            paramValueViewModel = new ParamValueViewModel(value);
            if(IsChecked == true) {
                paramValueViewModel.IsChecked = true;
            }

            paramValueViewModel.PropertyChanged += OnParamValueChanged;

            ParamValues.Add(paramValueViewModel);
            _paramValues.Add(GetKey(value), paramValueViewModel);
        }

        return paramValueViewModel;
    }

    private static string GetKey(string value) {
        if(value is null) {
            return "<null>";
        }

        if(value.Equals(string.Empty)) {
            return "<empty>";
        }

        return value;
    }
}
