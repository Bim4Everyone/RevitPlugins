
using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCreateViewSheet.ViewModels {
    internal class TitleBlockViewModel : BaseViewModel, IEquatable<TitleBlockViewModel> {
        private readonly FamilySymbol _familySymbol;

        public TitleBlockViewModel(FamilySymbol familySymbol) {
            _familySymbol = familySymbol ?? throw new ArgumentNullException(nameof(familySymbol));
        }


        public ElementId TitleBlockSymbolId {
            get => _familySymbol.Id;
        }

        public string Name {
            get => $"{_familySymbol.FamilyName}: {_familySymbol.Name}";
        }

        public bool Equals(TitleBlockViewModel other) {
            return other is not null
                && _familySymbol.Id == other._familySymbol.Id;
        }

        public override int GetHashCode() {
            return -234318566 + EqualityComparer<ElementId>.Default.GetHashCode(_familySymbol.Id);
        }

        public override bool Equals(object obj) {
            return Equals(obj as TitleBlockViewModel);
        }
    }
}
