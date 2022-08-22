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

        public BoundingBoxXYZ GetClashBoundingBox() {
            var docInfos = _revitRepository.GetDocInfos();
            var mainDoc = docInfos.FirstOrDefault(item => item.Name.Equals(MainElement.DocumentName));
            var otherDoc = docInfos.FirstOrDefault(item => item.Name.Equals(OtherElement.DocumentName));

            BoundingBoxXYZ bb1 = null;
            BoundingBoxXYZ bb2 = null;

            if(mainDoc != null
                && otherDoc != null
                && _revitRepository.IsValidElement(mainDoc.Doc, new ElementId(MainElement.Id))
                && _revitRepository.IsValidElement(otherDoc.Doc, new ElementId(OtherElement.Id))) {
                bb1 = SolidUtils.CreateTransformed(mainDoc.Doc.GetElement(new ElementId(MainElement.Id)).GetSolid(), mainDoc.Transform).GetBoundingBox().GetTransformedBoundingBox();
                bb2 = SolidUtils.CreateTransformed(otherDoc.Doc.GetElement(new ElementId(OtherElement.Id)).GetSolid(), otherDoc.Transform).GetBoundingBox().GetTransformedBoundingBox();
                if(bb1 == null && bb2 == null) {
                    return null;
                }
                if(bb1 == null) {
                    return bb2;
                }
                if(bb2 == null) {
                    return bb1;
                }
                return bb1.GetIntersection(bb2);
            }
            return null;
        }

        public void SetRevitRepository(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public override bool Equals(object obj) {
            return Equals(obj as ClashModel);
        }

        public override int GetHashCode() {
            int hashCode = 2096115351;
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementModel>.Default.GetHashCode(MainElement);
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementModel>.Default.GetHashCode(OtherElement);
            return hashCode;
        }

        public bool Equals(ClashModel other) {
            return other != null
                && EqualityComparer<ElementModel>.Default.Equals(MainElement, other.MainElement)
                && EqualityComparer<ElementModel>.Default.Equals(OtherElement, other.OtherElement);
        }
    }

    internal enum ClashStatus {
        [Display(Name="Активно"), Image("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_High.png")]
        Active,
        [Display(Name = "Проанализировано"), Image("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_Low.png")]
        Analized,
        [Display(Name = "Исправлено"), Image("pack://application:,,,/DevExpress.Images.v21.2;component/Images/XAF/State_Priority_Normal.png")]
        Solved
    }
}