using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение параметра <see cref="RealOpeningArPlacer.RealOpeningManualBimModelPart"/>
    /// </summary>
    internal class ManualBimModelPartValueGetter : IValueGetter<StringParamValue> {
        private readonly HashSet<string> _bimModelPartNames = new HashSet<string>();


        /// <summary>
        /// Конструктор класса, предоставляющего значение параметра <see cref="RealOpeningArPlacer.RealOpeningManualBimModelPart"/>
        /// <para>
        /// После вызова конструктора необходимо задать разделы BIM модели через Set-свойства
        /// </para>
        /// </summary>
        public ManualBimModelPartValueGetter() { }


        public ManualBimModelPartValueGetter SetIsEom(IsEomValueGetter isEomValueGetter) {
            if(isEomValueGetter?.GetValue().TValue == 1) {
                _bimModelPartNames.Add(isEomValueGetter.ToString());
            }
            return this;
        }

        public ManualBimModelPartValueGetter SetIsSs(IsSsValueGetter isSsValueGetter) {
            if(isSsValueGetter?.GetValue().TValue == 1) {
                _bimModelPartNames.Add(isSsValueGetter.ToString());
            }
            return this;
        }

        public ManualBimModelPartValueGetter SetIsOv(IsOvValueGetter isOvValueGetter) {
            if(isOvValueGetter?.GetValue().TValue == 1) {
                _bimModelPartNames.Add(isOvValueGetter.ToString());
            }
            return this;
        }

        public ManualBimModelPartValueGetter SetIsDu(IsDuValueGetter isDuValueGetter) {
            if(isDuValueGetter?.GetValue().TValue == 1) {
                _bimModelPartNames.Add(isDuValueGetter.ToString());
            }
            return this;
        }

        public ManualBimModelPartValueGetter SetIsVk(IsVkValueGetter isVkValueGetter) {
            if(isVkValueGetter?.GetValue().TValue == 1) {
                _bimModelPartNames.Add(isVkValueGetter.ToString());
            }
            return this;
        }

        public ManualBimModelPartValueGetter SetIsTs(IsTsValueGetter isTsValueGetter) {
            if(isTsValueGetter?.GetValue().TValue == 1) {
                _bimModelPartNames.Add(isTsValueGetter.ToString());
            }
            return this;
        }


        public StringParamValue GetValue() {
            return new StringParamValue(string.Join("+", _bimModelPartNames.OrderBy(a => a)));
        }
    }
}
