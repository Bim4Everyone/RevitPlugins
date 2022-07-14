using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.ViewModels.Interfaces;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig.OffsetViewModels {
    internal class OffsetViewModel : BaseViewModel, IOffsetViewModel {
        private double _from;
        private double _to;
        private double _offset;

        public OffsetViewModel(Offset offset) {
            To = offset.To;
            From = offset.From;
            Offset = offset.OffsetValue;
        }

        public OffsetViewModel() {

        }

        public double From {
            get => _from;
            set => this.RaiseAndSetIfChanged(ref _from, value);
        }
        public double To {
            get => _to;
            set => this.RaiseAndSetIfChanged(ref _to, value);
        }
        public double Offset {
            get => _offset;
            set => this.RaiseAndSetIfChanged(ref _offset, value);
        }

        public string GetErrorText() {
            if(From < 0) {
                return "значение параметра \"От\" должно быть неотрицательным.";
            }
            if(To < 0) {
                return "значение параметра \"До\" должно быть неотрицательным.";
            }
            if(From > To) {
                return "значение парметра \"От\" должно быть меньше значения параметра \"До\".";
            }
            if(Offset > To) {
                return "значение \"Зазора\" должно быть меньше значения параметра \"До\".";
            }
            return null;
        }

        public Offset GetOffset() {
            return new Offset() { From = From, To = To, OffsetValue = Offset };
        }

        public string GetIntersectText(IOffsetViewModel offset) {
            if((offset.From >= From || offset.To >= From) && (offset.To <= To || offset.From <= To)) {
                return $"пересекаются диапазоны значений параметров \"{From}\" - \"{To}\" с \"{offset.From}\" - \"{offset.To}\".";
            }
            return null;
        }
    }
}
