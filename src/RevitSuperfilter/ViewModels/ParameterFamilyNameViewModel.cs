using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels {
    internal class ParameterFamilyNameViewModel : SelectableObjectViewModel<ElementType>, IParameterViewModel, IEquatable<ParameterFamilyNameViewModel> {
        public ParameterFamilyNameViewModel(ElementType objectData, IEnumerable<Element> elements)
            : base(objectData) {
            Elements = new ObservableCollection<Element>(elements);
        }

        public int Count => Elements.Count;
        public override string DisplayData => ObjectData.FamilyName;
        public ObservableCollection<Element> Elements { get; }

        public override bool Equals(object obj) {
            return Equals(obj as ParameterFamilyNameViewModel);
        }

        public bool Equals(ParameterFamilyNameViewModel other) {
            return other != null &&
                   DisplayData == other.DisplayData;
        }

        public override int GetHashCode() {
            return -1258489443 + EqualityComparer<string>.Default.GetHashCode(DisplayData);
        }

        public IEnumerable<Element> GetSelectedElements() {
            if(IsSelected == true) {
                return Elements;
            }

            return Enumerable.Empty<Element>();
        }
    }
}