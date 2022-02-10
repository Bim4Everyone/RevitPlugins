using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        private readonly IEnumerable<GroupedRuleSettings> rules;
        private readonly LintelsConfig _lintelsConfig;
        private readonly LintelsCommonConfig lintelsCommonConfig;
        private readonly List<GroupedRuleSettings> _rulesSettings;
        private SampleMode _selectedSampleMode;

        private LintelCollectionViewModel _lintels;
        private GroupedRuleCollectionViewModel _groupedRules;
        private ObservableCollection<LinkViewModel> _links;

        public MainViewModel() {

        }

        public MainViewModel(RevitRepository revitRepository, IEnumerable<GroupedRuleSettings> rules, LintelsConfig lintelsConfig, LintelsCommonConfig lintelsCommonConfig) {
            this._revitRepository = revitRepository;
            this.rules = rules;
            this._lintelsConfig = lintelsConfig;
            this.lintelsCommonConfig = lintelsCommonConfig;
            this._rulesSettings = rules.ToList();
            Lintels = new LintelCollectionViewModel(_revitRepository);
            GroupedRules = new GroupedRuleCollectionViewModel(_revitRepository, lintelsConfig, rules);
            PlaceLintelCommand = new RelayCommand(PlaceLintels, p => true);
            var links = _revitRepository.GetLinkTypes().ToList();
            if(links.Count > 0) {
                Links = new ObservableCollection<LinkViewModel>(links.Select(l => new LinkViewModel() { Name =  Path.GetFileNameWithoutExtension(l.Name) }));
            } else {
                Links = new ObservableCollection<LinkViewModel>();
            }
        }

        public SampleMode SelectedSampleMode {
            get => _selectedSampleMode;
            set => this.RaiseAndSetIfChanged(ref _selectedSampleMode, value);
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

        public ObservableCollection<LinkViewModel> Links {
            get => _links;
            set => this.RaiseAndSetIfChanged(ref _links, value);
        }

        public void PlaceLintels(object p) {
            GroupedRules.Save(p);

            var elementInWallIds = _revitRepository.GetAllElementsInWall(SelectedSampleMode)
                .Select(e => e.Id)
                .ToList();

            foreach(var lintel in Lintels.LintelInfos) {
                if(elementInWallIds.Contains(lintel.ElementInWallId))
                    elementInWallIds.Remove(lintel.ElementInWallId);
            }

            LintelChecker lc = new LintelChecker(_revitRepository, _lintelsConfig, _rulesSettings);
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
                    if(_revitRepository.CheckHorizontal(view3D, elementInWall, true, Links.Where(l => l.IsChecked).Select(l => l.Name), out double rightOffset)) {
                        if(rightOffset > 0) {
                            lintel.SetParamValue("Смещение_справа", rightOffset);
                        }
                        lintel.SetParamValue("ОпираниеСправа", 0); //ToDo: параметр
                    }

                    if(_revitRepository.CheckHorizontal(view3D, elementInWall, false, Links.Where(l=>l.IsChecked).Select(l=>l.Name), out double leftOffset)) {
                        if(leftOffset > 0) {
                            lintel.SetParamValue("Смещение_слева", leftOffset);
                        }
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
