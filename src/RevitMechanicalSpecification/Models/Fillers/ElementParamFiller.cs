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
using RevitMechanicalSpecification.Service;

namespace RevitMechanicalSpecification.Models.Fillers {
    public abstract class ElementParamFiller : IElementParamFiller {
        private readonly SpecConfiguration _config;
        private readonly Document _document;


        public ElementParamFiller(
            string targetParamName,
            string originalParamName,
            SpecConfiguration specConfiguration,
            Document document) {

            _config = specConfiguration;
            _document = document;
            TargetParamName = targetParamName;
            OriginalParamName = originalParamName;
        }

        protected Parameter TargetParam { get; set; }

        protected string OriginalParamValue { get; set; }

        protected string TargetParamName { get; set; }

        protected string OriginalParamName { get; set; }

        protected SpecConfiguration Config => _config;

        protected Document Document => _document;

        public abstract void SetParamValue(SpecificationElement specificationElement);

        public void Fill(SpecificationElement specificationElement) {
            // Существует ли целевой параметр в экземпляре
            if(!specificationElement.Element.IsExistsSharedParam(TargetParamName)) {
                return;
            }
            TargetParam = specificationElement.Element.GetSharedParam(TargetParamName);

            // Проверка на нулл - для ситуаций где нет имени исходного(ФОП_ВИС_Число, Группирование), тогда исходный парам так и остается пустым 
            if(!(OriginalParamName is null)) {
                if(!specificationElement.IsTypeOrInstanceParamExist(OriginalParamName)) {
                    return;
                }
                // Получаем значение оригинального параметра
                OriginalParamValue = specificationElement.GetTypeOrInstanceParamStringValue(OriginalParamName);
            }

            // Если целевой параметр ридонли - можно сразу идти дальше
            if(TargetParam.IsReadOnly) {
                return;
            }

            this.SetParamValue(specificationElement);
        }
    }
}
