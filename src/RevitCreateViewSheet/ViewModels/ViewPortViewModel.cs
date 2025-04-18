using System;
using System.Collections.Generic;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class ViewPortViewModel : BaseViewModel, IEquatable<ViewPortViewModel> {
        private readonly ILocalizationService _localizationService;
        private ViewPortTypeViewModel _viewPortType;

        public ViewPortViewModel(ViewPortModel viewPortModel, ILocalizationService localizationService) {
            ViewPortModel = viewPortModel ?? throw new ArgumentNullException(nameof(viewPortModel));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _viewPortType = new ViewPortTypeViewModel(ViewPortModel.ViewPortType);
            IsPlacedStatus = localizationService.GetLocalizedString(
                viewPortModel.Exists ? "EntityState.Exist" : "EntityState.New");
        }


        public string ViewName => ViewPortModel.Name;

        public string IsPlacedStatus { get; }

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
