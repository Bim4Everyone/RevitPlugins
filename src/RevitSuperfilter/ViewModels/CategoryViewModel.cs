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

internal sealed class CategoryViewModel : BaseViewModel, IElementIndex {
    private readonly Dictionary<ElementId, Element> _elementsById = new();
    private readonly Dictionary<Definition, ParamViewModel> _params = new(DefinitionNameComparer.Instance);

    private readonly Category _category;

    public CategoryViewModel(Category category) {
        _category = category;
        LoadParamsCommand = RelayCommand.Create(LoadParams, CanLoadParams);
    }

    public ICommand LoadParamsCommand { get; }

    public bool IsLoaded { get; private set; }

    public int Count => _elementsById.Count;
    public string Name => _category?.Name;

    public ObservableCollection<ParamViewModel> Params { get; } = [];
    
    #region IElementIndex

    public void Add(Element element) {
        if(IsLoaded && _elementsById.ContainsKey(element.Id)) {
            Remove(element.Id);
        }

        if(IsLoaded) {
            _elementsById.Add(element.Id, element);
        }

        var elementParams = element.Parameters
            .OfType<Parameter>()
            .OrderBy(x => x.Definition, DefinitionNameComparer.Instance);

        foreach(var elementParam in elementParams) {
            var param = GetOrAdd(elementParam);
            param.Add(element, elementParam.GetValueOrDefault());
        }
    }

    public void Remove(ElementId elementId) {
        if(!_elementsById.TryGetValue(elementId, out var element)) {
            return;
        }

        _elementsById.Remove(elementId);

        var elementParams = element.Parameters
            .OfType<Parameter>()
            .OrderBy(x => x.Definition, DefinitionNameComparer.Instance);

        foreach(var elementParam in elementParams) {
            if(_params.TryGetValue(elementParam.Definition, out var paramViewModel)) {
                Params.Remove(paramViewModel);
                _params.Remove(elementParam.Definition);
            }
        }
    }

    #endregion IElementIndex

    public void Build(IEnumerable<Element> elements) {
        foreach(var element in elements) {
            _elementsById.Add(element.Id, element);
        }
    }
    
    private ParamViewModel GetOrAdd(Parameter elementParam) {
        if(!_params.TryGetValue(elementParam.Definition, out var paramViewModel)) {
            paramViewModel = new ParamViewModel(elementParam.Definition);

            Params.Add(paramViewModel);
            _params.Add(elementParam.Definition, paramViewModel);
        }

        return paramViewModel;
    }

    #region LoadParamsCommand

    private void LoadParams() {
        try {
            foreach(var element in _elementsById.Values) {
                Add(element);
            }
        } finally {
            IsLoaded = true;
        }
    }

    private bool CanLoadParams() {
        return !IsLoaded;
    }

    #endregion LoadParamsCommand
}
