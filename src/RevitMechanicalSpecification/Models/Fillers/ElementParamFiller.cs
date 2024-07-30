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

        protected string GetTypeOrInstanceParamValue(Element element, string paramName) {
            if(element.IsExistsParam(paramName)) 
                { return element.GetSharedParamValue<string>(paramName); }
            Element elemType = element.GetElementType();
            if(elemType.IsExistsParam(paramName)) 
                { return elemType.GetSharedParamValue<string>(paramName); }
            return null;
        }

        private Parameter GetTypeOrInstanceParam(Element element, Element elemType, string paramName) 
            {
            if(element.IsExistsParam(paramName)) 
                { return element.GetParam(paramName); }
            if(elemType.IsExistsParam(paramName)) 
                { return elemType.GetParam(paramName); }
            return null;
        }

        private bool IsTypeOrInstanceExists(Element element, Element elemType, string paramName) {
            if(paramName is null) 
                { return true; }
            if(element.IsExistsParam(paramName)) 
                { return true; }
            if(elemType.IsExistsParam(paramName)) 
                { return true; }
            return false;
        }
        public void Fill(Element element) {
            Element elemType = element.GetElementType();

            //Проверяем, если существует исходный параметр в типе или экземпляре, или целевой ТОЛЬКО в экземпляре
            if(!IsTypeOrInstanceExists(element, elemType, FromParamName) || !element.IsExistsParam(ToParamName)) 
                { return; }
            
            //Если параметры существуют создаем их экземпляры чтоб не пересоздавать
            ToParam = element.GetParam(ToParamName);
            //Проверка на нулл - для ситуаций где нет имени исходного(ФОП_ВИС_Число), тогда исходный парам так и остается пустым 
            if(!(FromParamName is null)) {
                FromParam = GetTypeOrInstanceParam(element, elemType, FromParamName);
            }

            //Если целевой параметр ридонли - можно сразу идти дальше
            if(ToParam.IsReadOnly) 
                { return; }

            this.SetParamValue(element);
        }
    }
}
