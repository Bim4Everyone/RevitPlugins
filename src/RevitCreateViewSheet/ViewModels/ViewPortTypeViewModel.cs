using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewPortTypeViewModel : BaseViewModel, IEquatable<ViewPortTypeViewModel> {
        private readonly ElementType _viewPortType;

        public ViewPortTypeViewModel(ElementType viewPortType) {
            _viewPortType = viewPortType ?? throw new ArgumentNullException(nameof(viewPortType));
        }


        public string Name => _viewPortType.Name;

        public ElementType ViewType => _viewPortType;


        public bool Equals(ViewPortTypeViewModel other) {
            return other is not null
                && _viewPortType.Id == other._viewPortType.Id;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ViewPortTypeViewModel);
        }

        public override int GetHashCode() {
            return 2108858624 + EqualityComparer<ElementId>.Default.GetHashCode(_viewPortType.Id);
        }
    }
}
