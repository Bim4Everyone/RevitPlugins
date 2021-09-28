using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitGenLookupTables.Models;

namespace RevitGenLookupTables.ViewModels {
    internal class FamilyParamValuesViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private int _startValue;
        private int _endValue;
        private int _stepValue;
        private string _paramValues;

        public FamilyParamValuesViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            GenerateCommand = new RelayCommand(Generate, CanGenerate);
        }

        public int MinValue {
            get => _startValue;
            set => this.RaiseAndSetIfChanged(ref _startValue, value);
        }

        public int MaxValue {
            get => _endValue;
            set => this.RaiseAndSetIfChanged(ref _endValue, value);
        }

        public int StepValue {
            get => _stepValue;
            set => this.RaiseAndSetIfChanged(ref _stepValue, value);
        }

        public string ParamValues {
            get => _paramValues;
            set => this.RaiseAndSetIfChanged(ref _paramValues, value);
        }

        public ICommand GenerateCommand { get; }

        public IEnumerable<string> GetParamValues() {
            return ParamValues?.Split(Environment.NewLine.ToArray(), StringSplitOptions.RemoveEmptyEntries);
        }

        private void Generate(object param) {
            ParamValues = string.Join(Environment.NewLine, RangedEnumeration(MinValue, MaxValue, StepValue));
        }

        private bool CanGenerate(object param) {
            return true;
        }

        public static IEnumerable<int> RangedEnumeration(int min, int max, int step) {
            return Enumerable.Range(min, max - min + 1).Where(i => (i - min) % step == 0);
        }
    }
}
