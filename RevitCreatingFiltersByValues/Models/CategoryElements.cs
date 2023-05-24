using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCreatingFiltersByValues.Models {
    internal class CategoryElements {
        public CategoryElements(Category cat, ElementId id) {
            CategoryInView = cat;
            CategoryIdInView = id;
        }

        public CategoryElements(Category cat, ElementId id, List<Element> elements) {
            CategoryInView = cat;
            CategoryIdInView = id;
            ElementsInView = elements;
        }

        public Category CategoryInView { get; set; }
        public ElementId CategoryIdInView { get; set; }


        public List<Element> ElementsInView { get; set; } = new List<Element>();
    }
}
