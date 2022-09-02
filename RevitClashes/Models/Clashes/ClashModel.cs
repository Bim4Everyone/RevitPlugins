using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using DevExpress.Mvvm.DataAnnotations;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Extensions;

namespace RevitClashDetective.Models.Clashes {
    internal class ClashModel : IEquatable<ClashModel> {
        private RevitRepository _revitRepository;

        public ClashModel(RevitRepository revitRepository, Element mainElement, Element otherElement) {
            _revitRepository = revitRepository;

            MainElement = new ElementModel(_revitRepository, mainElement);
            OtherElement = new ElementModel(_revitRepository, otherElement);
        }

        public ClashModel() { }
        public ClashStatus ClashStatus { get; set; }
        public ElementModel MainElement { get; set; }
        public ElementModel OtherElement { get; set; }


        public void SetRevitRepository(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ClashModel);
        }

        public override int GetHashCode() {
            int hashCode = 2096115351;
            hashCode *= EqualityComparer<ElementModel>.Default.GetHashCode(MainElement);
            hashCode *= EqualityComparer<ElementModel>.Default.GetHashCode(OtherElement);
            return hashCode;
        }

        public bool Equals(ClashModel other) {
            return other != null
                && ((EqualityComparer<ElementModel>.Default.Equals(MainElement, other.MainElement)
                && EqualityComparer<ElementModel>.Default.Equals(OtherElement, other.OtherElement))
                || (EqualityComparer<ElementModel>.Default.Equals(MainElement, other.OtherElement)
                && EqualityComparer<ElementModel>.Default.Equals(OtherElement, other.MainElement)));
        }
    }

    internal enum ClashStatus {
        [Display(Name = "Активно"), Image("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_High.png")]
        Active,
        [Display(Name = "Проанализировано"), Image("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_Low.png")]
        Analized,
        [Display(Name = "Исправлено"), Image("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_Normal.png")]
        Solved
    }
}