using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

namespace RevitSetLevelSection.Models {
    internal class FillAdskParam : IFillParam {
        private readonly SharedParam _adskParam;
        private readonly SharedParam[] _copyFromParam;

        public FillAdskParam(SharedParam adskParam, SharedParam[] copyFromParam) {
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

            element.SetParamValue(_adskParam, copyFromParam);
        }
    }
}