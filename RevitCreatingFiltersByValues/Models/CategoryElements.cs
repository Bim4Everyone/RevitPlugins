using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCreatingFiltersByValues.Models {
    internal class CategoryElements {

        public CategoryElements(Category cat, ElementId id, bool check, List<Element> elements) {
            CategoryName = cat.Name;
            CategoryInView = cat;
            CategoryIdInView = id;
            IsCheck = check;
            ElementsInView = elements;
        }

        public string CategoryName { get; set; }
        public Category CategoryInView { get; set; }
        public ElementId CategoryIdInView { get; set; }

        public bool IsCheck { get; set; } = false;


        public List<Element> ElementsInView { get; set; } = new List<Element>();
    }
}
