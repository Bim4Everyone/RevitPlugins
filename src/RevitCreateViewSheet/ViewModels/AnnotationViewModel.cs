using System;
using System.Collections.Generic;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class AnnotationViewModel : BaseViewModel, IEquatable<AnnotationViewModel> {
        private readonly ILocalizationService _localizationService;

        public AnnotationViewModel(AnnotationModel annotationModel, ILocalizationService localizationService) {
            AnnotationModel = annotationModel ?? throw new ArgumentNullException(nameof(annotationModel));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            IsPlacedStatus = localizationService.GetLocalizedString(
                annotationModel.Exists ? "EntityState.Exist" : "EntityState.New");
        }


        public string FamilyName => AnnotationModel.FamilyName;

        public string SymbolName => AnnotationModel.SymbolName;

        public string IsPlacedStatus { get; }

        public AnnotationModel AnnotationModel { get; }


        public bool Equals(AnnotationViewModel other) {
            return other is not null
                && AnnotationModel.Equals(other.AnnotationModel);
        }

        public override bool Equals(object obj) {
            return Equals(obj as AnnotationViewModel);
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<AnnotationModel>.Default.GetHashCode(AnnotationModel);
        }
    }
}
