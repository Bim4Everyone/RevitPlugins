﻿using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;
using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.ClashDetection {
    internal class FilterProvider : IProvider {

        private readonly Filter _filter;

        public FilterProvider(Document doc, Filter filterElement, Transform transform) {
            Doc = doc;
            _filter = filterElement;
            MainTransform = transform;
        }

        public Document Doc { get; }
        public Transform MainTransform { get; }

        public List<Element> GetElements() {
            var categories = _filter.CategoryIds.Select(item => (BuiltInCategory) item.GetIdValue()).ToList();

            var elements = new FilteredElementCollector(Doc)
                .WherePasses(new ElementMulticategoryFilter(categories))
                .WherePasses(_filter.GetRevitFilter(Doc, new StraightRevitFilterGenerator()))
                .WhereElementIsNotElementType()
                .Where(item => item.get_Geometry(new Options()) != null)
                .ToList();
            return elements;
        }

        public List<Solid> GetSolids(Element element) {
            return element.GetSolids();
        }
    }
}