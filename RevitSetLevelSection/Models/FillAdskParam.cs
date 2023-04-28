using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

namespace RevitSetLevelSection.Models {
    internal class FillAdskParam : IFillParam {
        private readonly RevitParam _adskParam;
        private readonly RevitParam[] _copyFromParam;

        public FillAdskParam(RevitParam adskParam, params RevitParam[] copyFromParam) {
            _adskParam = adskParam;
            _copyFromParam = copyFromParam;
        }

        public RevitParam RevitParam => _adskParam;

        public void UpdateValue(Element element) {
            if(!element.IsExistsParam(_adskParam)) {
                return;
            }

            var copyFromParam = _copyFromParam
                .Where(item => element.IsExistsParamValue(item))
                .Select(item => element.GetParam(item))
                .FirstOrDefault();

            if(copyFromParam == null) {
                element.RemoveParamValue(_adskParam);
                return;
            }

            element.SetParamValue(_adskParam, copyFromParam);
        }
    }
}