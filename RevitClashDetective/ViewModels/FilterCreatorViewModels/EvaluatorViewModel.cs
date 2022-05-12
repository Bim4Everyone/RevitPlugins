using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using DevExpress.Mvvm;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Evaluators;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels {
    internal class EvaluatorViewModel : BaseViewModel {
        private SetEvaluator _setEvaluator;

        public SetEvaluator SetEvaluator {
            get => _setEvaluator;
            set => this.RaiseAndSetIfChanged(ref _setEvaluator, value);
        }
    }
}
