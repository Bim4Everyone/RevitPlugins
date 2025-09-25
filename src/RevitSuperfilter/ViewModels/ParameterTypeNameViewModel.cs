using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels;

internal class ParameterTypeNameViewModel : SelectableObjectViewModel<ElementType>, IParameterViewModel,
    IEquatable<ParameterTypeNameViewModel> {
    public ParameterTypeNameViewModel(ElementType objectData, IEnumerable<Element> elements)
        : base(objectData) {
        Elements = new ObservableCollection<Element>(elements);
    }

    public override string DisplayData => ObjectData.Name;

    public bool Equals(ParameterTypeNameViewModel other) {
        return other != null && DisplayData == other.DisplayData;
    }

    public int Count => Elements.Count;
    public ObservableCollection<Element> Elements { get; }

    public IEnumerable<Element> GetSelectedElements() {
        if(IsSelected == true) {
            return Elements;
        }

        return Enumerable.Empty<Element>();
    }

    public override bool Equals(object obj) {
        return Equals(obj as ParameterTypeNameViewModel);
    }

    public override int GetHashCode() {
        return -1258489443 + EqualityComparer<string>.Default.GetHashCode(DisplayData);
    }
}
