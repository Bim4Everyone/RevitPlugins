using System;

using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewTypeViewModel : BaseViewModel, IEquatable<ViewTypeViewModel> {
        private readonly RevitViewType _viewType;

        public ViewTypeViewModel(RevitViewType viewType, string name) {
            if(string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException(nameof(name));
            }
            _viewType = viewType;
            Name = name;
        }

        public RevitViewType ViewType => _viewType;

        public string Name { get; }

        public bool Equals(ViewTypeViewModel other) {
            return other is not null
                && _viewType == other._viewType;
        }

        public override int GetHashCode() {
            return 1650804207 + _viewType.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as ViewTypeViewModel);
        }
    }
}
