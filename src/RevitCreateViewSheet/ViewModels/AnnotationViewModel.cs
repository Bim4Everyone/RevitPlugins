using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class AnnotationViewModel : BaseViewModel, IEquatable<AnnotationViewModel>, IEntityViewModel {
        private readonly AnnotationModel _annotationModel;

        public AnnotationViewModel(AnnotationModel annotationModel) {
            _annotationModel = annotationModel ?? throw new ArgumentNullException(nameof(annotationModel));
            IsPlaced = annotationModel.State == EntityState.Unchanged;
        }


        public string FamilyName => _annotationModel.FamilyName;

        public string SymbolName => _annotationModel.SymbolName;

        public bool IsPlaced { get; }

        public IEntity Entity => AnnotationModel;

        public AnnotationModel AnnotationModel => _annotationModel;


        public bool Equals(AnnotationViewModel other) {
            return other is not null
                && _annotationModel.Equals(other._annotationModel);
        }

        public override bool Equals(object obj) {
            return Equals(obj as AnnotationViewModel);
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<AnnotationModel>.Default.GetHashCode(_annotationModel);
        }
    }
}
