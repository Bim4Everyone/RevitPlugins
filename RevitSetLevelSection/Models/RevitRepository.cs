﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitSetLevelSection.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);
        }

        public Document Document => _document;
        public Application Application => _application;

        public ProjectInfo ProjectInfo => _document.ProjectInformation;

        public Element GetElements(ElementId elementId) {
            return _document.GetElement(elementId);
        }
        
        public IEnumerable<RevitLinkInstance> GetLinkInstances() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(RevitLinkInstance))
                .OfType<RevitLinkInstance>()
                .ToList();
        }
        
        public void UpdateElements(RevitParam revitParam, string paramValue) {
            using(Transaction transaction = _document.StartTransaction($"Установка уровня/секции \"{revitParam.Name} - По сведениям о проекте\"")) {
                ProjectInfo.SetParamValue(revitParam, paramValue);
                IEnumerable<Element> elements = GetElements(revitParam);

                foreach(Element element in elements) {
                    element.SetParamValue(revitParam, paramValue);
                }

                transaction.Commit();
            }
        }

        public void UpdateElements(RevitParam revitParam, IEnumerable<FamilyInstance> massElements) {
            List<Element> elements = GetElements(revitParam);
            using(Transaction transaction = _document.StartTransaction($"Установка уровня/секции \"{revitParam.Name} - по формообразующим\"")) {
                foreach(FamilyInstance massObject in massElements) {
                    Parameter massParameter = massObject.GetParam(revitParam);
                    foreach(Element element in elements) {
                        if(IsIntersectCenterElement(massObject, element)) {
                            Parameter parameter = element.GetParam(revitParam);
                            parameter.Set(massParameter);
                        } else {
                            element.RemoveParamValue(revitParam);
                        }
                    }
                }

                transaction.Commit();
            }
        }
        
        private List<Element> GetElements(RevitParam revitParam) {
            var catFilter = new ElementMulticategoryFilter(GetCategories(revitParam));
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .WherePasses(catFilter)
                .ToList();
        }

        private bool IsIntersectCenterElement(FamilyInstance massElement, Element element) {
            var outline = GetOutline(element);
            var point = (outline.MaximumPoint - outline.MinimumPoint) / 2 + outline.MinimumPoint;

            var massOutline = GetOutline(massElement);
            return massOutline.MinimumPoint.X < point.X
                   && massOutline.MinimumPoint.Y < point.Y
                   && massOutline.MinimumPoint.Z < point.Z
                   && massOutline.MaximumPoint.X > point.X
                   && massOutline.MaximumPoint.Y > point.Y
                   && massOutline.MaximumPoint.Z > point.Z;
        }

        private Outline GetOutline(Element element) {
            var boundingBox = element.get_BoundingBox(_document.ActiveView);
            if(boundingBox == null) {
                return new Outline(XYZ.Zero, XYZ.Zero);
            }

            return new Outline(boundingBox.Min, boundingBox.Max);
        }

        private ElementId[] GetCategories(RevitParam revitParam) {
            return _document.GetParameterBindings()
                .Where(item => item.Binding.IsInstanceBinding())
                .Where(item => revitParam.IsRevitParam(_document, item.Definition))
                .SelectMany(item => item.Binding.GetCategories())
                .Select(item => item.Id)
                .ToArray();
        }
    }
}
