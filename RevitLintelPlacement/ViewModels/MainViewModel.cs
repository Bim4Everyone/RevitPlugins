using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly IEnumerable<RulesSettigs> _rulesSettings;
        private RuleCollectionViewModel _rules;
        private LintelCollectionViewModel _lintels;

        public MainViewModel() {

        }

        public MainViewModel(RevitRepository revitRepository, IEnumerable<RulesSettigs> rulesSettings) {
            this._revitRepository = revitRepository;
            this._rulesSettings = rulesSettings;
            Lintels = new LintelCollectionViewModel();
            Rules = new RuleCollectionViewModel(_revitRepository, _rulesSettings);
            CheckRules();
        }
        public RuleCollectionViewModel Rules {
            get => _rules;
            set => this.RaiseAndSetIfChanged(ref _rules, value);
        }

        public LintelCollectionViewModel Lintels {
            get => _lintels;
            set => this.RaiseAndSetIfChanged(ref _lintels, value);
        }

        public void CheckRules() {
            //проверка проема на соответствие всем правилам (отфильтровываются ненужные проемы)
            var elementInWalls = _revitRepository.GetAllElementsInWall();
            foreach(var elementInWall in elementInWalls) {
                var smth = Rules.CheckConditions(elementInWall);
            }
        }

    }
}
