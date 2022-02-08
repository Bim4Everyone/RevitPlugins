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
        private readonly RulesSettings _rulesSettings;

        private LintelCollectionViewModel _lintels;
        private GroupedRuleCollectionViewModel _groupedRules;

        public MainViewModel() {

        }

        public MainViewModel(RevitRepository revitRepository, RulesSettings rulesSettings, LintelsConfig lintelsConfig, LintelsCommonConfig lintelsCommonConfig) {
            this._revitRepository = revitRepository;
            this._rulesSettings = rulesSettings;
            Lintels = new LintelCollectionViewModel(_revitRepository);
            GroupedRules = new GroupedRuleCollectionViewModel(_revitRepository, rulesSettings);
            PlaceLintelCommand = new RelayCommand(PlaceLintels, p => true);
        }

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
            GroupedRules.SaveConfig();

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
                    if(_revitRepository.CheckHorizontal(view3D, elementInWall, true, out double rightOffset)) {
                        lintel.SetParamValue("Смещение_справа", rightOffset);
                        lintel.SetParamValue("ОпираниеСправа", 0); //ToDo: параметр
                    }

                    if(_revitRepository.CheckHorizontal(view3D, elementInWall, false, out double leftOffset)) {
                        lintel.SetParamValue("Смещение_слева", leftOffset);
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




    }
}
