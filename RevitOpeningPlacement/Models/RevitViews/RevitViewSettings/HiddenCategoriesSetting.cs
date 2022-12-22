using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RevitViews.RevitViewSettings {
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
