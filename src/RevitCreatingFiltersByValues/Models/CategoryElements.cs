using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCreatingFiltersByValues.Models;

public class CategoryElements {
    public CategoryElements(Category cat, ElementId id, List<Element> elements) {
        CategoryInView = cat;
        CategoryIdInView = id;
        ElementsInView = elements;
    }

    public Category CategoryInView { get; set; }
    
    public ElementId CategoryIdInView { get; set; }
    
    public List<Element> ElementsInView { get; set; }
}
