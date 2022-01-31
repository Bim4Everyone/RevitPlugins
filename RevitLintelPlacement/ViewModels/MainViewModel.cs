using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
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
            Lintels = new LintelCollectionViewModel(_revitRepository);
            Rules = new RuleCollectionViewModel(_revitRepository, _rulesSettings);
            InitializeLintels();
            PlaceLintelCommand = new RelayCommand(PlaceLintels, p => true);
        }
        public RuleCollectionViewModel Rules {
            get => _rules;
            set => this.RaiseAndSetIfChanged(ref _rules, value);
        }

        public LintelCollectionViewModel Lintels {
            get => _lintels;
            set => this.RaiseAndSetIfChanged(ref _lintels, value);
        }

        public ICommand PlaceLintelCommand { get; set; }

        public void PlaceLintels(object p) {
            
            Dictionary<ElementId, RuleViewModel> elementInWallIdRuleDict = new Dictionary<ElementId, RuleViewModel>();
            foreach(var elementInWall in _revitRepository.GetAllElementsInWall().ToList()) {
                var rule = Rules.GetRule(elementInWall);
                if(rule != null) {
                    elementInWallIdRuleDict.Add(elementInWall.Id, rule); //соотношение элементов и правил
                }
            }
            var lintels = _revitRepository.GetLintels();
            //TODO: соотнести перемычки с проемами

            //1. логика с помощью схемы -> проверить корректность расстановки



            //2. геометрия
            foreach(var lintel in lintels) {
                var nearestElement = _revitRepository.GetNearestElement(lintel);
                    if(nearestElement == ElementId.InvalidElementId)
                    continue;
                if(elementInWallIdRuleDict.ContainsKey(nearestElement)) {
 
                    //TODO: проверить корректность расстановки

                    elementInWallIdRuleDict.Remove(nearestElement);
                }
            }


            //TODO: удалить потом
           

            var lintelType = _revitRepository.GetLintelType();

            //у оставшихся elementInWallRuleDict расставить перемычки
            using(Transaction t = _revitRepository.StartTransaction("Расстановка перемычек")) {
                foreach(var elementInWallId in elementInWallIdRuleDict.Keys) {
                    var elementInWall = _revitRepository.GetElementById(elementInWallId) as FamilyInstance;
                    if(!_revitRepository.CheckUp(elementInWall))
                        continue;
                    var lintel = _revitRepository.PlaceLintel(lintelType, elementInWallId);
                    elementInWallIdRuleDict[elementInWallId].LintelParameters.SetTo(lintel, elementInWall);
                    _revitRepository.LockLintel(lintel, elementInWall);
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


        //сопоставляются перемычки в группе + перемычки, закрепленные с элементом
        private void InitializeLintels() {
            var lintels = _revitRepository.GetLintels();
            foreach(var lintel in lintels) {
                if(lintel.SuperComponent != null) {
                    var elementInWall = (FamilyInstance) _revitRepository.GetElementById(lintel.SuperComponent.Id);
                    var lintelViewModel = new LintelInfoViewModel() {
                        LintelId = lintel.Id,
                        ElementInWallId = elementInWall.Id,
                        WallTypeName = elementInWall.Host.Name,
                        ElementInWallName = elementInWall.Name
                    };
                    Lintels.LintelInfos.Add(lintelViewModel);
                }
                var allignElement = _revitRepository.GetDimensionFamilyInstance(lintel);
                if(allignElement != null) { 
                    if (allignElement.Category.Name == _revitRepository.GetCategory(BuiltInCategory.OST_Doors).Name ||
                        allignElement.Category.Name == _revitRepository.GetCategory(BuiltInCategory.OST_Windows).Name) {
                        var elementInWall = (FamilyInstance) _revitRepository.GetElementById(allignElement.Id);
                        var lintelViewModel = new LintelInfoViewModel() {
                            LintelId = lintel.Id,
                            ElementInWallId = elementInWall.Id,
                            WallTypeName = elementInWall.Host.Name,
                            ElementInWallName = elementInWall.Name
                        };
                        Lintels.LintelInfos.Add(lintelViewModel);
                    }

                }

            }
        }

    }
}
