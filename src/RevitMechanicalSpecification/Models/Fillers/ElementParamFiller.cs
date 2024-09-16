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

        private string _elementName;
        private Parameter _targetParameter;
        private Parameter _originalParameter;
        private string _targetParameterName;
        private string _originalParameterName;
        private Element _elementType;
        private HashSet<ManifoldPart> _manifoldParts;
        private int _positionNumInManifold;

        public ElementParamFiller(
            string toParamName,
            string fromParamName,
            SpecConfiguration specConfiguration,
            Document document) {

            _targetParameterName = toParamName;
            _originalParameterName = fromParamName;
            _config = specConfiguration;
            _document = document;
        }

        protected string ElementName { 
            get => _elementName; 
            set => _elementName = value; 
        }

        protected Parameter TargetParameter {
            get => _targetParameter;
            set => _targetParameter = value;
        }

        protected Parameter OriginalParameter {
            get => _originalParameter;
            set => _originalParameter = value;
        }

        protected string TargetParamName {
            get => _targetParameterName;
            set => _targetParameterName = value;
        }

        protected string OriginalParamName {
            get => _originalParameterName;
            set => _originalParameterName = value;
        }

        protected Element ElemType {
            get => _elementType;
            set => _elementType = value;
        }

        protected HashSet<ManifoldPart> ManifoldParts {
            get => _manifoldParts;
            set => _manifoldParts = value;
        }

        protected int PositionNumInManifold {
            get => _positionNumInManifold;
            set => _positionNumInManifold = value;
        }

        protected SpecConfiguration Config => _config;

        protected Document Document => _document;

        public abstract void SetParamValue(SpecificationElement specificationElement);

        public void Fill(
            SpecificationElement specificationElement) {
            ElemType = specificationElement.ElementType;


            // Существует ли целевой параметр в экземпляре
            TargetParameter = specificationElement.Element.LookupParameter(TargetParamName);
            if(TargetParameter == null) {
                return;
            }

            // Проверка на нулл - для ситуаций где нет имени исходного(ФОП_ВИС_Число, Группирование), тогда исходный парам так и остается пустым 
            if(!(OriginalParamName is null)) {
                // Проверяем, если существует исходный параметр в типе или экземпляре
                OriginalParameter = specificationElement.GetTypeOrInstanceParam(OriginalParamName);
                if(OriginalParameter is null) {
                    return;
                }
            }

            // Если целевой параметр ридонли - можно сразу идти дальше
            if(TargetParameter.IsReadOnly) {
                return;
            }

            ManifoldParts = specificationElement.ManifoldParts;

            this.SetParamValue(specificationElement);
        }
    }
}
