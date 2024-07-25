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

        protected SpecConfiguration Config { get { return _config; } }

        protected Parameter ToParam;
        protected Parameter FromParam;

        protected readonly string ToParamName;
        protected readonly string FromParamName;
        protected readonly Document Document;
        private readonly SpecConfiguration _config;

        public ElementParamFiller(string toParamName, string fromParamName, SpecConfiguration specConfiguration, Document document) {
            ToParamName = toParamName;
            FromParamName = fromParamName;
            _config = specConfiguration;
            Document = document;
        }

        public abstract void SetParamValue(Element element);

        protected string GetTypeOrInstanceParamValue(Element element) {
            if(element.IsExistsParam(FromParamName)) 
                { return element.GetSharedParamValue<string>(FromParamName); }
            Element elemType = element.GetElementType();
            if(elemType.IsExistsParam(FromParamName)) 
                { return elemType.GetSharedParamValue<string>(FromParamName); }
            return null;
        }

        private bool IsTypeOrInstanceExists(Element element, string paramName) {
            if(paramName is null) 
                { return true; }
            if(element.IsExistsParam(paramName)) 
                { return true; }
            if(element.GetElementType().IsExistsParam(paramName)) 
                { return true; }
            return false;
        }
        public void Fill(Element element) {

            if(!IsTypeOrInstanceExists(element, FromParamName) || !element.IsExistsParam(ToParamName)) 
                { return; }

            

            ToParam = element.GetParam(ToParamName);
            if(!(FromParamName is null)) {
                FromParam = element.GetParam(FromParamName);
            }

            if(ToParam.IsReadOnly) 
                { return; }

            this.SetParamValue(element);
        }
    }
}
