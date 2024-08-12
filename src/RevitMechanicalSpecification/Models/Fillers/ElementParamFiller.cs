using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitMechanicalSpecification.Entities;

namespace RevitMechanicalSpecification.Models.Fillers {
    public abstract class ElementParamFiller : IElementParamFiller {

        protected SpecConfiguration Config => _config;

        protected Parameter ToParam;
        protected Parameter FromParam;
        protected FamilyInstance ManifoldInstance;
        protected HashSet<ManifoldPart> ManifoldParts;
        protected int Count;

        protected readonly string ToParamName;
        protected readonly string FromParamName;

        protected Element ElemType;

        protected readonly Document Document;
        private readonly SpecConfiguration _config;


        public ElementParamFiller(
            string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document) {
            ToParamName = toParamName;
            FromParamName = fromParamName;
            _config = specConfiguration;
            Document = document;
        }

        public abstract void SetParamValue(Element element);


        private Parameter GetTypeOrInstanceParam(Element element, string paramName) {
            if(paramName is null) {
                return null;
            }
            Parameter parameter = element.LookupParameter(paramName) ?? ElemType.LookupParameter(paramName);
            if(parameter == null) {
                return null;
            }
            return parameter;
        }

        public void Fill(
            Element manifoldElement, 
            FamilyInstance familyInstance = null, 
            int count = 0, 
            HashSet<ManifoldPart> manfifoldParts = null) {
            ElemType = Document.GetElement(manifoldElement.GetTypeId());


            //Существует ли целевой параметр в экземпляре
            //Если параметры существуют создаем их экземпляры чтоб не пересоздавать
            //все методы GetParam крашат, если null
            try {
                ToParam = manifoldElement.GetSharedParam(ToParamName);
            } catch(System.ArgumentException) {
                return;
            }

            //Проверка на нулл - для ситуаций где нет имени исходного(ФОП_ВИС_Число, Группирование), тогда исходный парам так и остается пустым 
            if(!(FromParamName is null)) {
                //Проверяем, если существует исходный параметр в типе или экземпляре
                FromParam = GetTypeOrInstanceParam(manifoldElement, FromParamName);
                if(FromParam is null) {
                    return;
                }
            }

            //Если целевой параметр ридонли - можно сразу идти дальше
            if(ToParam.IsReadOnly) {
                return;
            }


            ManifoldInstance = familyInstance;
            Count = count;
            ManifoldParts = manfifoldParts;

            this.SetParamValue(manifoldElement);
        }

    }
}
