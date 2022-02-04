using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly RulesSettigs _rulesSettings;

        //private RuleCollectionViewModel _rules;
        private LintelCollectionViewModel _lintels;
        private GroupedRuleCollectionViewModel _groupedRules;

        public MainViewModel() {

        }

        public MainViewModel(RevitRepository revitRepository, RulesSettigs rulesSettings) {
            this._revitRepository = revitRepository;
            this._rulesSettings = rulesSettings;
            Lintels = new LintelCollectionViewModel(_revitRepository);
            //Rules = new RuleCollectionViewModel(_revitRepository, rulesSettings);
            GroupedRules = new GroupedRuleCollectionViewModel(_revitRepository, rulesSettings);
            InitializeLintels();
            PlaceLintelCommand = new RelayCommand(PlaceLintels, p => true);
        }
        //public RuleCollectionViewModel Rules {
        //    get => _rules;
        //    set => this.RaiseAndSetIfChanged(ref _rules, value);
        //}

        public LintelCollectionViewModel Lintels {
            get => _lintels;
            set => this.RaiseAndSetIfChanged(ref _lintels, value);
        }

        public GroupedRuleCollectionViewModel GroupedRules {
            get => _groupedRules;
            set => this.RaiseAndSetIfChanged(ref _groupedRules, value);
        }

        public ICommand PlaceLintelCommand { get; set; }

        public void PlaceLintels(object p) {
            var elementInWallIds = _revitRepository.GetAllElementsInWall()
                .Select(e => e.Id)
                .ToList();

            foreach(var lintel in Lintels.LintelInfos) {
                if(elementInWallIds.Contains(lintel.ElementInWallId))
                    elementInWallIds.Remove(lintel.ElementInWallId);
            }

            LintelChecker lc = new LintelChecker(_revitRepository, _rulesSettings);
            var resultsForReport = lc.Check(Lintels.LintelInfos);

            var elevation = _revitRepository.GetElevation();
            var plan = _revitRepository.GetPlan();

            using(Transaction t = _revitRepository.StartTransaction("Расстановка перемычек")) {
                var view3D = _revitRepository.GetView3D();
                foreach(var elementId in elementInWallIds) {
                    var elementInWall = (FamilyInstance) _revitRepository.GetElementById(elementId);
                    var rule = GroupedRules.GetRule(elementInWall);
                    if(rule == null)
                        continue;
                    if(!_revitRepository.CheckUp(view3D, elementInWall))
                        continue;
                    var lintelType = _revitRepository.GetLintelType(rule.SelectedLintelType);
                    var lintel = _revitRepository.PlaceLintel(lintelType, elementId);
                    rule.SetParametersTo(lintel, elementInWall);
                    if(_revitRepository.CheckHorizontal(view3D, elementInWall, true)) {
                        lintel.SetParamValue("ОпираниеСправа", 0); //ToDo: параметр
                    }

                    if(_revitRepository.CheckHorizontal(view3D, elementInWall, false)) {
                        lintel.SetParamValue("ОпираниеСлева", 0);
                    }
                    _revitRepository.LockLintel(elevation, plan, lintel, elementInWall);
                    Lintels.LintelInfos.Add(new LintelInfoViewModel(_revitRepository, lintel, elementInWall));
                }
                t.Commit();
            }
            var message = new ReportMaker().MakeMessage(resultsForReport);
            if(!string.IsNullOrEmpty(message)) {
                TaskDialog.Show("Revit", message);
            }
        }


        //сопоставляются перемычки в группе + перемычки, закрепленные с элементом
        private void InitializeLintels() {
            var lintels = _revitRepository.GetLintels();
            var correlator = new LintelElementCorrelator(_revitRepository);
            var lintelInfos = lintels.Select(l =>
            new LintelInfoViewModel(_revitRepository, l, correlator.Correlate(l)));
            foreach(var lintelInfo in lintelInfos) {
                Lintels.LintelInfos.Add(lintelInfo);
            }
        }

    }
}
