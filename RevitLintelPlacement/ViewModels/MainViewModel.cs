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
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private SampleMode _selectedSampleMode;

        private LintelCollectionViewModel _lintels;
        private GroupedRuleCollectionViewModel _groupedRules;
        private ObservableCollection<LinkViewModel> _links;
        private ElementInfosViewModel _elementInfosViewModel;

        public MainViewModel() {

        }

        public MainViewModel(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
            ElementInfos = new ElementInfosViewModel(_revitRepository);
            
            GroupedRules = new GroupedRuleCollectionViewModel(_revitRepository, ElementInfos);
            PlaceLintelCommand = new RelayCommand(PlaceLintels, p => true);
            ShowReportCommand = new RelayCommand(ShowReport, p=>true);
            var links = _revitRepository.GetLinkTypes().ToList();
            if(links.Count > 0) {
                Links = new ObservableCollection<LinkViewModel>(links.Select(l => new LinkViewModel() { Name = Path.GetFileNameWithoutExtension(l.Name) }));
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
        public ICommand ShowReportCommand { get; set; }

        public ObservableCollection<LinkViewModel> Links {
            get => _links;
            set => this.RaiseAndSetIfChanged(ref _links, value);
        }

        public ElementInfosViewModel ElementInfos { 
            get => _elementInfosViewModel; 
            set => this.RaiseAndSetIfChanged(ref _elementInfosViewModel, value); 
        }

        public void PlaceLintels(object p) {
            ElementInfos.ElementInfos.Clear();
            if(!_revitRepository.CheckConfigParameters(ElementInfos)) {
                ShowReport();
                return;
            }

            foreach(var type in _revitRepository.GetLintelTypes()) {
                if(!_revitRepository.CheckLintelType(type, ElementInfos)) {
                    ShowReport();
                    return;
                }
            }
            Lintels = new LintelCollectionViewModel(_revitRepository);
            
            

            var elementInWalls = _revitRepository.GetAllElementsInWall(SelectedSampleMode)
                .ToList();

            foreach(var lintel in Lintels.LintelInfos) {
                var element = elementInWalls.FirstOrDefault(e => e.Id == lintel.ElementInWallId);
                if(element!=null) {
                    elementInWalls.Remove(element);
                }
            }
            var view3D = _revitRepository.GetView3D();
            using(Transaction t = _revitRepository.StartTransaction("Подготовка к расстановке перемычек")) {
                if(view3D.IsSectionBoxActive) {
                    view3D.IsSectionBoxActive = false;
                }
                t.Commit();
            }
            LintelChecker lc = new LintelChecker(_revitRepository, GroupedRules, ElementInfos);
            lc.Check(Lintels.LintelInfos);

            var elevation = _revitRepository.GetElevation();
            var plan = _revitRepository.GetPlan();
          
            
            var links = Links.Where(l => l.IsChecked).Select(l => l.Name).ToList();
            using(Transaction t = _revitRepository.StartTransaction("Расстановка перемычек")) {

                foreach(var elementInWall in elementInWalls) {
                    var rule = GroupedRules.GetRule(elementInWall);
                    if(rule == null)
                        continue;
                    if(!_revitRepository.CheckUp(view3D, elementInWall, Links.Where(l => l.IsChecked).Select(l => l.Name)))
                        continue;
                    if(string.IsNullOrEmpty(rule.SelectedLintelType)) {
                        TaskDialog.Show("Предупреждение!", "В проект не загружено семейство перемычки.");
                        return;
                    }
                    var lintelType = _revitRepository.GetLintelType(rule.SelectedLintelType);
                    if(lintelType == null) {
                        TaskDialog.Show("Предупреждение!", $"В проект не загружено семейство с выбранным типоразмером \"{rule.SelectedLintelType}\".");
                        return;
                    }
                    var lintel = _revitRepository.PlaceLintel(lintelType, elementInWall);
                    rule.SetParametersTo(lintel, elementInWall);
                    if(_revitRepository.DoesCornerNeeded(view3D, elementInWall, true, links, ElementInfos, out double rightOffset)) {
                        lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightOffset, rightOffset > 0 ? rightOffset : 0);
                        lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightCorner, 1);
                    }

                    if(_revitRepository.DoesCornerNeeded(view3D, elementInWall, false, links, ElementInfos, out double leftOffset)) {
                        lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelLeftOffset, leftOffset > 0 ? leftOffset : 0);
                        lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelLeftCorner, 1);
                    }
                    _revitRepository.LockLintel(elevation, plan, lintel, elementInWall);
                    Lintels.LintelInfos.Add(new LintelInfoViewModel(_revitRepository, lintel, elementInWall));
                }
                
                t.Commit();
            }
            if(ElementInfos.ElementInfos != null && ElementInfos.ElementInfos.Count > 0) {
                ShowReport();
            }
        }

        private void ShowReport(object p) {
            ShowReport();
        }

        private void ShowReport() {
            ElementInfos.UpdateCollection();
            var view = new ReportView() { DataContext = ElementInfos };
            view.ShowDialog();
        }
    }
}