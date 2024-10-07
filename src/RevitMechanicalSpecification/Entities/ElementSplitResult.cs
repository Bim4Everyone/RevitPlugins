using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitMechanicalSpecification.Entities {
    public class ElementSplitResult {
        public List<SpecificationElement> SingleElements { get; set; }
        public List<SpecificationElement> ManifoldElements { get; set; }

        public ElementSplitResult(List<SpecificationElement> singleElements, List<SpecificationElement> manifoldElements) {
            SingleElements = singleElements;
            ManifoldElements = manifoldElements;
        }
    }
}
