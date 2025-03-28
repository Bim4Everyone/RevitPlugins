using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewPortViewModel : BaseViewModel, IEquatable<ViewPortViewModel> {
        private readonly ViewPortModel _viewPortModel;
        private ViewPortTypeViewModel _viewPortType;

        public ViewPortViewModel(ViewPortModel viewPortModel) {
            _viewPortModel = viewPortModel ?? throw new ArgumentNullException(nameof(viewPortModel));
            _viewPortType = new ViewPortTypeViewModel(_viewPortModel.ViewPortType);
            IsPlaced = _viewPortModel.State == EntityState.Unchanged;
        }


        public string ViewName => _viewPortModel.Name;

        public bool IsPlaced { get; }

        public ViewPortModel ViewPortModel => _viewPortModel;

        public ViewPortTypeViewModel ViewPortType {
            get => _viewPortType;
            set {
                RaiseAndSetIfChanged(ref _viewPortType, value);
                _viewPortModel.ViewPortType = value?.ViewType;
            }
        }


        public bool Equals(ViewPortViewModel other) {
            return other is not null
                && ViewName == other.ViewName;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ViewPortViewModel);
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(ViewName);
        }
    }
}
