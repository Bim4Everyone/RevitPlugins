using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitScheduleImport.ViewModels {
    internal class CategoryViewModel : IEquatable<CategoryViewModel> {
        private readonly Category _category;

        public CategoryViewModel(Category category) {
            _category = category ?? throw new ArgumentNullException(nameof(category));
        }


        public string Name => _category?.Name ?? string.Empty;

        public BuiltInCategory BuiltInCategory => _category?.GetBuiltInCategory() ?? BuiltInCategory.INVALID;

        public override bool Equals(object obj) {
            return Equals(obj as CategoryViewModel);
        }
        public override int GetHashCode() {
            return -1288486024 + BuiltInCategory.GetHashCode();
        }

        public bool Equals(CategoryViewModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return BuiltInCategory == other.BuiltInCategory;
        }
    }
}
