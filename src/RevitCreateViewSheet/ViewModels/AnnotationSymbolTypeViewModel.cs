using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCreateViewSheet.ViewModels {
    internal class AnnotationSymbolTypeViewModel : BaseViewModel, IEquatable<AnnotationSymbolTypeViewModel> {
        private readonly AnnotationSymbolType _annotationSymbolType;

        public AnnotationSymbolTypeViewModel(AnnotationSymbolType annotationSymbolType) {
            _annotationSymbolType = annotationSymbolType ?? throw new ArgumentNullException(nameof(annotationSymbolType));
        }

        public string Name => _annotationSymbolType.Name;

        public string FamilyName => _annotationSymbolType.FamilyName;

        public string RichName => $"{FamilyName}: {Name}";

        public AnnotationSymbolType AnnotationSymbolType => _annotationSymbolType;


        public bool Equals(AnnotationSymbolTypeViewModel other) {
            return other is not null
                && _annotationSymbolType.Id == other._annotationSymbolType.Id;
        }

        public override int GetHashCode() {
            return 2131698743 + EqualityComparer<ElementId>.Default.GetHashCode(_annotationSymbolType.Id);
        }

        public override bool Equals(object obj) {
            return Equals(obj as AnnotationSymbolTypeViewModel);
        }
    }
}
