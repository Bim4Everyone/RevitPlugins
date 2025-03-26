using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCreatingFiltersByValues.Models {
    public class ExportContext : IExportContext {
        private readonly Document _document;
        public List<Element> ExportedElements { get; } = new List<Element>();

        public ExportContext(Document document) {
            _document = document;
        }

        public bool Start() {
            return true;
        }

        public void Finish() {
        }

        public RenderNodeAction OnElementBegin(ElementId elementId) {
            // Получаем элемент по его ID
            Element element = _document.GetElement(elementId);
            if(element != null) {
                ExportedElements.Add(element);
            }
            return RenderNodeAction.Proceed;
        }

        // Остальные методы интерфейса IExportContext
        public RenderNodeAction OnViewBegin(ViewNode node) => RenderNodeAction.Proceed;
        public RenderNodeAction OnInstanceBegin(InstanceNode node) => RenderNodeAction.Proceed;
        public void OnElementEnd(ElementId elementId) { }
        public void OnInstanceEnd(InstanceNode node) { }
        public void OnViewEnd(ElementId elementId) { }
        public void OnPolymesh(PolymeshTopology node) { }
        public void OnMaterial(MaterialNode node) { }

        public bool IsCanceled() {
            return false;
        }

        public RenderNodeAction OnLinkBegin(LinkNode node) {
            return RenderNodeAction.Proceed;
        }

        public void OnLinkEnd(LinkNode node) { }

        public RenderNodeAction OnFaceBegin(FaceNode node) {
            return RenderNodeAction.Proceed;
        }

        public void OnFaceEnd(FaceNode node) { }

        public void OnRPC(RPCNode node) { }

        public void OnLight(LightNode node) { }
    }
}
