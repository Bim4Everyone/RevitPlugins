using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMechanicalSpecification.Models.Fillers {
    public abstract class ElementParamFiller : IElementParamFiller {
        protected string ToParamName { get { return _toParamName; } }
        protected string FromParamName { get { return _fromParamName; } }

        private readonly string _toParamName;
        private readonly string _fromParamName;

        public ElementParamFiller(string toParamName, string fromParamName) {
            _toParamName = toParamName;
            _fromParamName = fromParamName;
        }

        public abstract void SetParamValue(Element element);
        protected string GetTypeOrInstanceParamValue(Element element) {
            if(element.IsExistsParam(_fromParamName)) { return element.GetSharedParamValueOrDefault(_fromParamName, ""); }
            if(element.GetElementType().IsExistsParam(_fromParamName)) { return element.GetElementType().GetSharedParamValueOrDefault(_fromParamName, ""); }
            return null;
        }
        private bool IsTypeOrInstanceExists(Element element, string paramName) {
            if(paramName == "Skip") { return true; }
            if(element.IsExistsParam(paramName)) { return true; }
            if(element.GetElementType().IsExistsParam(paramName)) { return true; }
            return false;
        }
        public void Fill(Element element) {
            if(!IsTypeOrInstanceExists(element, FromParamName) || !element.IsExistsParam(ToParamName)) { return; } 
            this.SetParamValue(element);
        }
    }
}
