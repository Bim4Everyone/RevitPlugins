using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitViewSettings {
    internal class HiddenCategoriesSetting : IView3DSetting {
        private readonly ICollection<BuiltInCategory> _categories;

        public HiddenCategoriesSetting(ICollection<BuiltInCategory> categories) {
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        }

        public void Apply(View3D view3D) {
            foreach(var category in _categories) {
                if(view3D.CanCategoryBeHidden(new ElementId(category))) {
                    view3D.SetCategoryHidden(new ElementId(category), true);
                }
            }
        }
    }
}
