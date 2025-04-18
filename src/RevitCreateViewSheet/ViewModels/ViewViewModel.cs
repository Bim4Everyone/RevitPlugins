using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewViewModel : BaseViewModel, IEquatable<ViewViewModel> {
        private readonly View _view;

        public ViewViewModel(View view) {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public string Name => _view.Name;

        public View View => _view;

        public bool Equals(ViewViewModel other) {
            return other is not null
                && _view.Id == other._view.Id;
        }

        public override int GetHashCode() {
            return -513642073 + EqualityComparer<ElementId>.Default.GetHashCode(_view.Id);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ViewViewModel);
        }
    }
}
