using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

using RevitClashDetective.Models.Extensions;

namespace RevitClashDetective.Models.Clashes {
    internal class ClashModel {
        private RevitRepository _revitRepository;

        public ClashModel(RevitRepository revitRepository, Element mainElement, Element otherElement) {
            _revitRepository = revitRepository;

            MainElement = new ElementModel(_revitRepository, mainElement);
            OtherElement = new ElementModel(_revitRepository, otherElement);
        }

        public ClashModel() { }
        public bool IsSolved { get; set; }
        public ElementModel MainElement { get; set; }
        public ElementModel OtherElement { get; set; }

        public BoundingBoxXYZ GetClashBoundingBox() {
            var docInfos = _revitRepository.GetDocInfos();
            var mainDoc = docInfos.FirstOrDefault(item => item.Name.Equals(MainElement.DocumentName));
            var otherDoc = docInfos.FirstOrDefault(item => item.Name.Equals(OtherElement.DocumentName));

            BoundingBoxXYZ bb1 = null;
            BoundingBoxXYZ bb2 = null;

            if(mainDoc != null && otherDoc != null) {
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
    }
}