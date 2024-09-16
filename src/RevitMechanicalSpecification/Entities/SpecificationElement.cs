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
        private Element _manifoldElement;
        private Element _manifoldElementType;
        private string _elementName;
        private string _manifoldName = null;
        private FamilyInstance _manifoldInstance = null;
        private HashSet<ManifoldPart> _manifoldParts = null;
        private SpecificationElement _manifoldSpElement;

        public SpecificationElement ManifoldSpElement {
            get => _manifoldSpElement;
            set => _manifoldSpElement = value;
        }

        public Element Element {
            get => _element;
            set => _element = value;
        }

        public Element ElementType {
            get => _elementType;
            set => _elementType = value;
        }

        public Element ManifoldElement {
            get => _manifoldElement;
            set => _manifoldElement = value;
        }

        public Element ManifoldElementType {
            get => _manifoldElementType;
            set => _manifoldElementType = value;
        }

        public string ElementName {
            get => _elementName;
            set => _elementName = value;
        }

        public string ManifoldName {
            get => _manifoldName;
            set => _manifoldName = value;
        }

        public FamilyInstance ManifoldInstance {
                get => _manifoldInstance;
                set => _manifoldInstance = value;
        }

        public HashSet<ManifoldPart> ManifoldParts {
            get => _manifoldParts;
            set => _manifoldParts = value;
        }
    }
}
