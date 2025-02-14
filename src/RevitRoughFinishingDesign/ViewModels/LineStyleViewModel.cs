using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitRoughFinishingDesign.ViewModels {
    internal class LineStyleViewModel : IEquatable<LineStyleViewModel> {
        private readonly GraphicsStyle _graphicStyle;

        public LineStyleViewModel(GraphicsStyle graphicsStyle) {
            _graphicStyle = graphicsStyle ?? throw new ArgumentNullException(nameof(graphicsStyle));
        }

        public string Name => _graphicStyle.Name;
        public ElementId GraphicStyleId => _graphicStyle.Id;
        public override bool Equals(object obj) {
            return Equals(obj as LineStyleViewModel);
        }
        public bool Equals(LineStyleViewModel other) {
            if(ReferenceEquals(null, other)) { return false; };
            if(ReferenceEquals(this, other)) { return true; };
            return _graphicStyle.Id == other._graphicStyle.Id;
        }

        public override int GetHashCode() {
            return -853382692 + EqualityComparer<ElementId>.Default.GetHashCode(GraphicStyleId);
        }

        public override string ToString() {
            return Name;
        }
    }
}
