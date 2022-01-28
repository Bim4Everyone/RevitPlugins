using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

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
            var smth = _revitRepository.GetElementTest().GroupBy(e=>((FamilyInstance)e).Host.Name).ToList();
            var sm = _revitRepository.GetElementTest().ToList();
            var elementInWalls = _revitRepository.GetAllElementsInWall().ToList();
            Dictionary<ElementId, RuleViewModel> elementInWallIdRuleDict = new Dictionary<ElementId, RuleViewModel>();
            List<Element> elementInWallRuleDict = new List<Element>();
            Dictionary<ElementId, ElementId> lintelElementInWallDict = new Dictionary<ElementId, ElementId>();
            foreach(var elementInWall in elementInWalls) {
                var rule = Rules.GetRule(elementInWall);
                if(rule != null) {
                    elementInWallIdRuleDict.Add(elementInWall.Id, rule); //соотношение элементов и правил
                    elementInWallRuleDict.Add(elementInWall); //соотношение элементов и правил
                }
            }
            var smth2 = elementInWallRuleDict.GroupBy(e => ((FamilyInstance) e).Host.Name).ToList();
            var lintels = _revitRepository.GetLintels();
            //соотнести перемычки с проемами

            //1. логика с помощью схемы -> проверить корректность расстановки

            //2. геометрия
            foreach(var lintel in lintels) {
                var nearestElement = _revitRepository.GetNearestElement(lintel);
                    if(nearestElement == ElementId.InvalidElementId)
                    continue;
                if(elementInWallIdRuleDict.ContainsKey(nearestElement)) {
                    //lintelElementInWallDict.Add()
                    lintelElementInWallDict.Add(lintel.Id, nearestElement);
                    //проверить корректность расстановки
                    elementInWallIdRuleDict.Remove(nearestElement);
                }
            }


            //TODO: удалить потом
            foreach(var lintel in lintels) {
                if(lintel.SuperComponent != null && elementInWallIdRuleDict.ContainsKey(lintel.SuperComponent.Id)) {
                    elementInWallIdRuleDict.Remove(lintel.SuperComponent.Id);
                }
            }

            var lintelType = _revitRepository.GetLintelType();

            //у оставшихся elementInWallRuleDict расставить перемычки

            using(Transaction t = _revitRepository.StartTransaction("Расстановка перемычек")) {
                foreach(var elementInWallId in elementInWallIdRuleDict.Keys) {
                    var elementInWall = _revitRepository.GetElementById(elementInWallId) as FamilyInstance;
                    if(!_revitRepository.CheckUp(elementInWall))
                        continue;
                    var lintel = _revitRepository.PlaceLintel(lintelType, elementInWallId);
                    elementInWallIdRuleDict[elementInWallId].LintelParameters.SetTo(lintel, elementInWall);
                    var lintelViewModel = new LintelInfoViewModel() {
                        LintelId = lintel.Id,
                        ElementInWallId = elementInWall.Id,
                        WallTypeName = elementInWall.Host.Name,
                        ElementInWallName = elementInWall.Name
                    };
                    Lintels.LintelInfos.Add(lintelViewModel);

                    //elementInWall.
                }
                t.Commit();
            }
        }

    }
}
