using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitApartmentPlans.ViewModels {
    internal class ViewTemplateViewModel : BaseViewModel, IEquatable<ViewTemplateViewModel> {
        private readonly ViewPlan _template;

        public ViewTemplateViewModel(ViewPlan template) {
            _template = template ?? throw new ArgumentNullException(nameof(template));
        }


        public string Name => _template.Name;

        public ViewType ViewTemplateType => _template.ViewType;


        public bool Equals(ViewTemplateViewModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return _template.Name == other._template.Name
                && _template.ViewType == other._template.ViewType;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ViewTemplateViewModel);
        }

        public override int GetHashCode() {
            int hashCode = -51381976;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + ViewTemplateType.GetHashCode();
            return hashCode;
        }

        public ViewPlan GetTemplate() {
            return _template;
        }
    }
}
