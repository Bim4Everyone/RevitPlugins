using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitToMongoDB.Model;
using RevitToMongoDB.ViewModels.Interfaces;

namespace RevitToMongoDB {
    internal sealed class RevitExporter {
        private readonly Document _document;
        private readonly IElementFactory _elementFactory;
        private readonly IElementRepository _elementsRepository;

        public RevitExporter(Document document,
        IElementFactory elementFactory,
        IElementRepository elementsRepository) {
            _document = document;
            _elementFactory = elementFactory;
            _elementsRepository = elementsRepository;
        }

        public void Export() {
            View3D navisView = new FilteredElementCollector(_document)
                .OfClass(typeof(View3D))
                .OfType<View3D>()
                .FirstOrDefault(item => item.Name.Equals("Navisworks"));
            if(navisView is null) {
                throw new InvalidOperationException("Не был найден вид Navisworks");
            }
            List<ElementDto> elements = new FilteredElementCollector(_document, navisView.Id)
                .WhereElementIsNotElementType()
                .Select(item => _elementFactory.CreateElement(item))
                .ToList();
            foreach(ElementDto elementDto in elements) {
                _elementsRepository.Insert(elementDto);
            }
        }
    }
}
