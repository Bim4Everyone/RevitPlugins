using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class AnnotationViewModel : BaseViewModel, IEquatable<AnnotationViewModel> {
        public AnnotationViewModel(AnnotationModel annotationModel) {
            AnnotationModel = annotationModel ?? throw new ArgumentNullException(nameof(annotationModel));
            IsPlaced = annotationModel.Exists;
        }


        public string FamilyName => AnnotationModel.FamilyName;

        public string SymbolName => AnnotationModel.SymbolName;

        public bool IsPlaced { get; }

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
