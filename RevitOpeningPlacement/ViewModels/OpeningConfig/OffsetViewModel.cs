using System;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class OffsetViewModel : BaseViewModel {
        private double _from;
        private double _to;
        private double _offset;
        private ObservableCollection<string> _typeNames;
        private string _selectedOpeningType;
        private ITypeNamesProvider _typeNamesProvider;

        public OffsetViewModel(Offset offset, ITypeNamesProvider typeNamesProvider) {
            To = offset.To;
            From = offset.From;
            Offset = offset.OffsetValue;
            _typeNamesProvider = typeNamesProvider;

            InitializeOpeningTypeNames();
            SelectedOpeningType = OpeningTypeNames.FirstOrDefault(item => item.Equals(offset.OpeningTypeName, StringComparison.CurrentCulture))
                ?? OpeningTypeNames.FirstOrDefault();
        }

        public OffsetViewModel(ITypeNamesProvider typeNamesProvider) {
            _typeNamesProvider = typeNamesProvider;

            InitializeOpeningTypeNames();
            SelectedOpeningType = OpeningTypeNames.FirstOrDefault();
        }

        public double From {
            get => _from;
            set => RaiseAndSetIfChanged(ref _from, value);
        }
        public double To {
            get => _to;
            set => RaiseAndSetIfChanged(ref _to, value);
        }
        public double Offset {
            get => _offset;
            set => RaiseAndSetIfChanged(ref _offset, value);
        }

        public ObservableCollection<string> OpeningTypeNames {
            get => _typeNames;
            set => RaiseAndSetIfChanged(ref _typeNames, value);
        }

        public string SelectedOpeningType {
            get => _selectedOpeningType;
            set => RaiseAndSetIfChanged(ref _selectedOpeningType, value);
        }

        public string GetErrorText() {
            if(From < 0) {
                return "значение параметра \"От\" должно быть неотрицательным.";
            }
            if(To < 0) {
                return "значение параметра \"До\" должно быть неотрицательным.";
            }
            if(From > To) {
                return "значение параметра \"От\" должно быть меньше значения параметра \"До\".";
            }
            if(Offset > To) {
                return "значение \"Зазора\" должно быть меньше значения параметра \"До\".";
            }
            return null;
        }

        public Offset GetOffset() {
            return new Offset() { From = From, To = To, OffsetValue = Offset, OpeningTypeName = SelectedOpeningType };
        }

        public string GetIntersectText(OffsetViewModel offset) {
            if((offset.From >= From || offset.To >= From) && (offset.To <= To || offset.From <= To)) {
                return $"пересекаются диапазоны значений параметров \"{From}\" - \"{To}\" с \"{offset.From}\" - \"{offset.To}\".";
            }
            return null;
        }

        public void Update(ITypeNamesProvider typeNamesProvider) {
            _typeNamesProvider = typeNamesProvider;
            InitializeOpeningTypeNames();
        }

        private void InitializeOpeningTypeNames() {
            OpeningTypeNames = new ObservableCollection<string>(_typeNamesProvider.GetTypeNames());
        }
    }
}
