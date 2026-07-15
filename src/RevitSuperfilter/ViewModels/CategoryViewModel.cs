using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSuperfilter.Comparators;
using RevitSuperfilter.Extensions;
using RevitSuperfilter.Models;

namespace RevitSuperfilter.ViewModels;

internal sealed class CategoryViewModel : BaseViewModel, IElementIndexList {
    private readonly Dictionary<ElementId, ElementIndex> _elementsById = new();
    private readonly Dictionary<ElementId, HashSet<Definition>> _definitions = new();
    private readonly Dictionary<Definition, DefinitionViewModel> _definitionVms = new(DefinitionNameComparer.Instance);

    private readonly Category _category;
    private bool _isExpanded;
    private bool _isSelected;
    private bool? _isChecked = false;
    private bool _isCascading;

    public CategoryViewModel(Category category) {
        _category = category;

        IsExpanded = true;
        LoadParamsCommand = RelayCommand.Create(LoadParams, CanLoadParams);

        PropertyChanged += OnPropertyChanged;
    }

    public ICommand LoadParamsCommand { get; }

    public bool IsLoaded { get; private set; }

    public int Count => _elementsById.Keys.Count;
    public string DisplayValue => _category?.Name;

    public bool IsSelected {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public bool? IsChecked {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }

    public bool IsExpanded {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public ObservableCollection<DefinitionViewModel> Definitions { get; } = [];

    #region IElementIndex

    public void Add(Element element) {
        if(!element.IsValidObject) {
            return;
        }
        
        if(_elementsById.ContainsKey(element.Id)) {
            Remove(element.Id);
        }

        if(!_elementsById.TryGetValue(element.Id, out var elementIndex)) {
            elementIndex = new ElementIndex(element);
            _elementsById.Add(element.Id, elementIndex);
        }



        AddType(element);
        FillDefinitions(element, null, false);
    }

    private void AddType(Element element) {
        if(!element.IsValidObject) {
            return;
        }

        if(!element.HasElementType()) {
            return;
        }

        var elementType = element.GetElementType();
        FillDefinitions(elementType, element, true);
    }

    public void Remove(ElementId elementId) {
        RemoveType(elementId);
        RemoveDefinitions(elementId);
    }
    
    private void RemoveType(ElementId elementId) {
        if(!_elementsById.TryGetValue(elementId, out var elementIndex)) {
            return;
        }

        _elementsById.Remove(elementIndex.Id);
        _elementsById.Remove(elementIndex.TypeId);

        RemoveDefinitions(elementIndex.TypeId);
    }

    #endregion IElementIndex

    public void Build(IEnumerable<Element> elements) {
        foreach(var element in elements) {
            _elementsById.Add(element.Id, new ElementIndex(element));
        }

        OnPropertyChanged(nameof(Count));
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(IsChecked)) {
            CascadeIsChecked();
        }
    }

    private void CascadeIsChecked() {
        if(_isCascading || IsChecked == null) {
            return;
        }

        _isCascading = true;
        try {
            if(!IsLoaded) {
                LoadParams();
            }

            foreach(var definition in Definitions) {
                definition.IsChecked = IsChecked!;
            }
        } finally {
            _isCascading = false;
        }
    }

    private void OnDefinitionChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName != nameof(DefinitionViewModel.IsChecked) || _isCascading) {
            return;
        }

        IsChecked = GetAggregateCheckState(Definitions);
    }

    private bool? GetAggregateCheckState(IEnumerable<DefinitionViewModel> definitions) {
        bool? result = false;
        bool isFirst = true;

        foreach(var definition in definitions) {
            bool? state = definition.IsChecked;
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

    private DefinitionViewModel GetOrAdd(Parameter elementParam, bool isType) {
        if(!_definitionVms.TryGetValue(elementParam.Definition, out var paramViewModel)) {
            paramViewModel = new DefinitionViewModel(elementParam.Definition, isType);
            if(IsChecked == true) {
                paramViewModel.IsChecked = IsChecked;
            }

            paramViewModel.PropertyChanged += OnDefinitionChanged;

            Definitions.Add(paramViewModel);
            _definitionVms.Add(elementParam.Definition, paramViewModel);
        }

        return paramViewModel;
    }
    
    private void FillDefinitions(Element element, Element innerElement, bool isType) {
        if(IsLoaded) {
            if(!_definitions.TryGetValue(element.Id, out var definitions)) {
                definitions = new HashSet<Definition>(DefinitionNameComparer.Instance);
                _definitions.Add(element.Id, definitions);
            }

            var elementParams = element.Parameters
                .OfType<Parameter>()
                .Where(x => x.Definition is not null)
                .OrderBy(x => x.Definition, DefinitionNameComparer.Instance);

            foreach(var elementParam in elementParams) {
                definitions.Add(elementParam.Definition);
                var param = GetOrAdd(elementParam, isType);
                param.Add(isType ? innerElement : element, elementParam.GetValueOrDefault());
            }
        }

        OnPropertyChanged(nameof(Count));
    }

    private void RemoveDefinitions(ElementId elementId) {
        if(!_elementsById.Remove(elementId)) {
            return;
        }

        if(!_definitions.TryGetValue(elementId, out var definitions)) {
            return;
        }

        if(IsLoaded) {
            foreach(var definition in definitions) {
                if(_definitionVms.TryGetValue(definition, out var definitionViewModel)) {
                    definitionViewModel.Remove(elementId);
                    if(definitionViewModel.Count == 0) {
                        _definitionVms.Remove(definition);
                        Definitions.Remove(definitionViewModel);
                        definitionViewModel.PropertyChanged -= OnDefinitionChanged;
                        IsChecked = GetAggregateCheckState(Definitions);
                    }
                }
            }
        }

        OnPropertyChanged(nameof(Count));
    }

    #region LoadParamsCommand

    private void LoadParams() {
        ApplySort();
        
        IsLoaded = true;
        foreach(var elementIndex in _elementsById.Values.ToArray()) {
            Add(elementIndex.Element);
        }
    }

    private bool CanLoadParams() {
        return !IsLoaded;
    }
    
    private void ApplySort() {
        var view = CollectionViewSource.GetDefaultView(Definitions);
        view.SortDescriptions.Clear();
        view.SortDescriptions.Add(new SortDescription(nameof(DefinitionViewModel.DisplayValue), ListSortDirection.Ascending));
    }

    #endregion LoadParamsCommand
}
