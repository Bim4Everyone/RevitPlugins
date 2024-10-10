using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Entities {
    public class SpecificationElement {
        private Element _element;
        private Element _elementType;
        private BuiltInCategory _builtInCategory;
        private string _elementName;
        private string _elementMark;
        private FamilyInstance _manifoldInstance = null;
        private SpecificationElement _manifoldSpElement;
        private SpecificationElement _insulationSpHost;

        public BuiltInCategory BuiltInCategory {
            get => _builtInCategory;
            set => _builtInCategory = value;
        }

        public SpecificationElement ManifoldSpElement {
            get => _manifoldSpElement;
            set => _manifoldSpElement = value;
        }

        public SpecificationElement InsulationSpHost {
            get => _insulationSpHost;
            set => _insulationSpHost = value;
        }

        public Element Element {
            get => _element;
            set => _element = value;
        }

        public Element ElementType {
            get => _elementType;
            set => _elementType = value;
        }

        public string ElementName {
            get => _elementName;
            set => _elementName = value;
        }

        public string ElementMark {
            get => _elementMark;
            set => _elementMark = value;
        }

        public FamilyInstance ManifoldInstance {
                get => _manifoldInstance;
                set => _manifoldInstance = value;
        }
    }
}
