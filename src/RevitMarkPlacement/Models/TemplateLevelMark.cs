using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Models {
    internal class TemplateLevelMark {
        public TemplateLevelMark(SpotDimension spotDimension, AnnotationManager annotationManager, AnnotationSymbol annotation = null) {
            SpotDimension = spotDimension;
            Annotation = annotation;
            AnnotationManager = annotationManager;
        }
        public AnnotationSymbol Annotation { get; }
        public SpotDimension SpotDimension { get; }
        public AnnotationManager AnnotationManager { get; }

        public void CreateAnnotation(int levelCount, double levelHeight) {
            AnnotationManager.CreateAnnotation(SpotDimension, levelCount, levelHeight);
        }

        public void OverwriteAnnotation(int levelCount, double levelHeight) {
            AnnotationManager.OverwriteAnnotation(SpotDimension, Annotation, levelCount, levelHeight);
        }

        public void UpdateAnnotation() {
            AnnotationManager.UpdateAnnotation(SpotDimension, Annotation);
        }
    }
}
