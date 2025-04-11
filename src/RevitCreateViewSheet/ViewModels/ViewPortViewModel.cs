using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewPortViewModel : BaseViewModel, IEquatable<ViewPortViewModel> {
        private ViewPortTypeViewModel _viewPortType;

        public ViewPortViewModel(ViewPortModel viewPortModel) {
            ViewPortModel = viewPortModel ?? throw new ArgumentNullException(nameof(viewPortModel));
            _viewPortType = new ViewPortTypeViewModel(ViewPortModel.ViewPortType);
            IsPlaced = ViewPortModel.Exists;
        }


        public string ViewName => ViewPortModel.Name;

        public bool IsPlaced { get; }

        public ViewPortModel ViewPortModel { get; }

        public ViewPortTypeViewModel ViewPortType {
            get => _viewPortType;
            set {
                RaiseAndSetIfChanged(ref _viewPortType, value);
                ViewPortModel.ViewPortType = value?.ViewType;
            }
        }


        public bool Equals(ViewPortViewModel other) {
            return other is not null
                && ViewPortModel.Equals(other.ViewPortModel);
        }

        public override bool Equals(object obj) {
            return Equals(obj as ViewPortViewModel);
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<ViewPortModel>.Default.GetHashCode(ViewPortModel);
        }
    }
}
