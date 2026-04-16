using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSuperfilter.Comparators;
using RevitSuperfilter.Extensions;
using RevitSuperfilter.Models;

namespace RevitSuperfilter.ViewModels;

internal sealed class CategoryViewModel : BaseViewModel, IElementIndexList {
    private readonly Dictionary<ElementId, Element> _elementsById = new();
    private readonly Dictionary<ElementId, HashSet<Definition>> _definitionKeys = new();
    private readonly Dictionary<Definition, DefinitionViewModel> _definitions = new(DefinitionNameComparer.Instance);

    private readonly Category _category;
    private bool _isExpanded;

    public CategoryViewModel(Category category) {
        _category = category;

        IsExpanded = true;
        LoadParamsCommand = RelayCommand.Create(LoadParams, CanLoadParams);
    }

    public ICommand LoadParamsCommand { get; }

    public bool IsLoaded { get; private set; }

    public int Count => _elementsById.Keys.Count;
    public string DisplayValue => _category?.Name;

    public bool IsExpanded {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public ObservableCollection<DefinitionViewModel> Definitions { get; } = [];

    #region IElementIndex

    public void Add(Element element) {
        if(_elementsById.ContainsKey(element.Id)) {
            Remove(element.Id);
        }

        if(!_elementsById.ContainsKey(element.Id)) {
            _elementsById.Add(element.Id, element);
        }

        if(!_definitionKeys.ContainsKey(element.Id)) {
            _definitionKeys.Add(element.Id, new HashSet<Definition>(DefinitionNameComparer.Instance));
        }

        if(IsLoaded) {
            var elementParams = element.Parameters
                .OfType<Parameter>()
                .OrderBy(x => x.Definition, DefinitionNameComparer.Instance);

            var definitions = _definitionKeys[element.Id];
            foreach(var elementParam in elementParams) {
                definitions.Add(elementParam.Definition);
                var param = GetOrAdd(elementParam);
                param.Add(element, elementParam.GetValueOrDefault());
            }
        }

        OnPropertyChanged(nameof(Count));
    }

    public void Remove(ElementId elementId) {
        if(!_definitionKeys.TryGetValue(elementId, out var definitions)) {
            return;
        }

        _elementsById.Remove(elementId);
        _definitionKeys.Remove(elementId);
        try {
            foreach(var definition in definitions) {
                if(_definitions.TryGetValue(definition, out var definitionViewModel)) {
                    definitionViewModel.Remove(elementId);
                    if(definitionViewModel.Count == 0) {
                        _definitions.Remove(definition);
                        Definitions.Remove(definitionViewModel);
                    }
                }
            }
        } catch(Exception ex) {
            string sss = ex.Message;
        }

        OnPropertyChanged(nameof(Count));
    }

    #endregion IElementIndex

    public void Build(IEnumerable<Element> elements) {
        foreach(var element in elements) {
            _elementsById.Add(element.Id, element);
            _definitionKeys.Add(element.Id, new HashSet<Definition>(DefinitionNameComparer.Instance));
        }

        OnPropertyChanged(nameof(Count));
    }

    private DefinitionViewModel GetOrAdd(Parameter elementParam) {
        if(!_definitions.TryGetValue(elementParam.Definition, out var paramViewModel)) {
            paramViewModel = new DefinitionViewModel(elementParam.Definition);

            Definitions.Add(paramViewModel);
            _definitions.Add(elementParam.Definition, paramViewModel);
        }

        return paramViewModel;
    }

    #region LoadParamsCommand

    private void LoadParams() {
        IsLoaded = true;
        foreach(var element in _elementsById.Values.ToArray()) {
            Add(element);
        }
    }

    private bool CanLoadParams() {
        return !IsLoaded;
    }

    #endregion LoadParamsCommand
}
