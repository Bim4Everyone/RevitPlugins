using System.Collections.Generic;
using System.Linq;
using System;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

using RevitSetLevelSection.Models.Repositories;

namespace RevitSetLevelSection.Models {
    internal class FillMassParam : IFillParam {
        private readonly ParamOption _paramOption;
        private readonly IDesignOption _designOption;
        private readonly IMassRepository _massRepository;
        
        private List<FamilyInstance> _massElements;
        private readonly IntersectImpl _intersectImpl;

        public FillMassParam(ParamOption paramOption, 
            IDesignOption designOption, 
            IMassRepository massRepository,
            Application application) {
            _paramOption = paramOption;
            _designOption = designOption;
            _massRepository = massRepository;

            _intersectImpl =
                new IntersectImpl() {LinkedTransform = massRepository.Transform, Application = application};

            // Кешируем нужные объекты
            _massElements = massRepository.GetElements(_designOption);
        }
        
        public RevitParam RevitParam => _paramOption.RevitParam;

        public void UpdateValue(Element element) {
            if(!element.IsExistsParam(_paramOption.RevitParam)) {
                return;
            }

            if(element.GetParamValueOrDefault(SharedParamsConfig.Instance.FixBuildingWorks, 0) == 1) {
                return;
            }

            var massObject = _massElements
                .FirstOrDefault(item => _intersectImpl.IsIntersect(item, element));

            try {
                string paramValue = massObject?.GetParamValue<string>(_paramOption);
                element.SetParamValue(_paramOption.RevitParam, paramValue);
            } catch(InvalidOperationException) {
                // решили что существует много вариантов,
                // когда параметр не может заполнится из-за настроек в ревите
                // Например: базовая стена внутри составной
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                // решили что существует много вариантов,
                // когда параметр не может заполнится из-за настроек в ревите
                // Например: базовая стена внутри составной
            }
        }
    }
}