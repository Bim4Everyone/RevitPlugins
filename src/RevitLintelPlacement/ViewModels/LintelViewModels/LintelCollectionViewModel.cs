using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Comparers;
using RevitLintelPlacement.Models;
using RevitLintelPlacement.Models.ElementInWallProviders;
using RevitLintelPlacement.Models.LintelsProviders;
using RevitLintelPlacement.ViewModels.SampleModeViewModels;

namespace RevitLintelPlacement.ViewModels {
    internal class LintelCollectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly ElementInfosViewModel _elementInfos;
        private int _countLintelInView;
        private ElementInWallKind _selectedElementKind;
        private ViewOrientation3D _orientation; //вряд ли здесь нужно хранить
        private ObservableCollection<LintelInfoViewModel> _lintelInfos;
        private List<SampleModeViewModel> _sampleModes;
        private SampleModeViewModel _selectedSampleMode;

        public LintelCollectionViewModel(RevitRepository revitRepository, ElementInfosViewModel elementInfos) {
            _revitRepository = revitRepository;
            _elementInfos = elementInfos;

            InitializeSampleModes();
            ApplySettings(revitRepository);
            InitializeLintels();

            LintelsViewSource = new CollectionViewSource { Source = LintelInfos };
            LintelsViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(LintelInfoViewModel.Level)));
            LintelsViewSource.Filter += ElementInWallKindFilter;

            SelectAndShowElementCommand = new RelayCommand(SelectElement);
            SelectNextCommand = new RelayCommand(SelectNext);
            SelectPreviousCommand = new RelayCommand(SelectPrevious);
            SelectionElementKindChangedCommand = new RelayCommand(SelectionElementKindChanged);
            SampleModeChangedCommand = new RelayCommand(SampleModeChanged);
            CloseCommand = new RelayCommand(Close);
            CountLintelInView = LintelsViewSource.View.Cast<LintelInfoViewModel>().Count();
        }

        public int CountLintelInView {
            get => _countLintelInView;
            set => this.RaiseAndSetIfChanged(ref _countLintelInView, value);
        }

        public ElementInWallKind SelectedElementKind {
            get => _selectedElementKind;
            set => this.RaiseAndSetIfChanged(ref _selectedElementKind, value);
        }

        public SampleModeViewModel SelectedSampleMode {
            get => _selectedSampleMode;
            set => this.RaiseAndSetIfChanged(ref _selectedSampleMode, value);
        }

        public ICommand SelectionElementKindChangedCommand { get; }
        public ICommand SelectAndShowElementCommand { get; }
        public ICommand SelectNextCommand { get; }
        public ICommand SelectPreviousCommand { get; }
        public ICommand SampleModeChangedCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public CollectionViewSource LintelsViewSource { get; set; }

        public List<SampleModeViewModel> SampleModes {
            get => _sampleModes;
            set => this.RaiseAndSetIfChanged(ref _sampleModes, value);
        }

        public ObservableCollection<LintelInfoViewModel> LintelInfos {
            get => _lintelInfos;
            set => this.RaiseAndSetIfChanged(ref _lintelInfos, value);
        }

        private void InitializeSampleModes() {
            var view = _revitRepository.GetCurrentView();
            SampleModes = new List<SampleModeViewModel>() {
                new SampleModeViewModel("Выборка по всем элементам",
                    new AllLintelsProvider(_revitRepository),
                    new AllElementsInWallProvider(_revitRepository, _elementInfos)),
                new SampleModeViewModel("Выборка по выделенным элементам",
                    new SelectedLintelsProvider(_revitRepository),
                    new SelectedElementsInWallProvider(_revitRepository, _elementInfos)),
                new SampleModeViewModel("Выборка по текущему виду",
                    new CurrentViewLintelsProvider(_revitRepository),
                    new CurrentViewElementsInWallProvider(_revitRepository, _elementInfos, view)),
            };
        }

        private void ApplySettings(RevitRepository revitRepository) {
            var config = revitRepository.LintelsConfig.GetSettings(_revitRepository.GetDocumentName());
            if(config != null) {
                SelectedSampleMode = SampleModes.FirstOrDefault(item => item.Name.Equals(config.SelectedModeNameNavigator, StringComparison.CurrentCulture))
                    ?? SampleModes.FirstOrDefault();
            } else {
                SelectedSampleMode = SampleModes.FirstOrDefault();
            }
        }

        //сопоставляются перемычки в группе + перемычки, закрепленные с элементом
        private void InitializeLintels() {
            LintelInfos = new ObservableCollection<LintelInfoViewModel>();
            var lintels = SelectedSampleMode.LintelsProvider.GetLintels();
            var correlator = new LintelElementCorrelator(_revitRepository, new AllElementsInWallProvider(_revitRepository, _elementInfos));
            var lintelInfos = lintels
                .Select(l => new LintelInfoViewModel(_revitRepository, l, correlator.Correlate(l)))
                .OrderBy(l => l.Level, new AlphanumericComparer());
            foreach(var lintelInfo in lintelInfos) {
                LintelInfos.Add(lintelInfo);
            }
        }

        private void ElementInWallKindFilter(object sender, FilterEventArgs e) {
            if(e.Item is LintelInfoViewModel lintel) {
                e.Accepted = SelectedElementKind == ElementInWallKind.All ? true : lintel.ElementInWallKind == SelectedElementKind;
            }
        }

        private async void SelectElement(object p) {
            if(!(p is ElementId id) || p == null) {
                return;
            }
            _revitRepository.SetActiveView();
            if(_orientation == null) {
                _orientation = _revitRepository.GetOrientation3D();
            }
            await _revitRepository.SelectAndShowElement(id, _orientation);
        }

        private void SelectNext(object p) {
            LintelsViewSource.View.MoveCurrentToNext();
            if(LintelsViewSource.View.IsCurrentAfterLast) {
                LintelsViewSource.View.MoveCurrentToPrevious();
            } else {
                var lintelInfo = LintelsViewSource.View.CurrentItem;
                if(lintelInfo is LintelInfoViewModel lintel) {
                    SelectElement(lintel.LintelId);
                }
            }
        }

        private void SelectPrevious(object p) {
            LintelsViewSource.View.MoveCurrentToPrevious();
            if(LintelsViewSource.View.IsCurrentBeforeFirst) {
                LintelsViewSource.View.MoveCurrentToNext();
            } else {
                var lintelInfo = LintelsViewSource.View.CurrentItem;
                if(lintelInfo is LintelInfoViewModel lintel) {
                    SelectElement(lintel.LintelId);
                }
            }
        }

        private void SelectionElementKindChanged(object p) {
            LintelsViewSource.View.Refresh();
            CountLintelInView = LintelsViewSource.View.Cast<LintelInfoViewModel>().Count();
        }

        private void SampleModeChanged(object p) {
            InitializeLintels();
            LintelsViewSource.Source = LintelInfos;
            LintelsViewSource.View.Refresh();
            CountLintelInView = LintelsViewSource.View.Cast<LintelInfoViewModel>().Count();
        }

        private void Close(object p) {
            LintelsSettings settings;
            if(_revitRepository.LintelsConfig.GetSettings(_revitRepository.GetDocumentName()) == null) {
                settings = _revitRepository.LintelsConfig.AddSettings(_revitRepository.GetDocumentName());
            } else {
                settings = _revitRepository.LintelsConfig.GetSettings(_revitRepository.GetDocumentName());
            }
            settings.SelectedModeNameNavigator = SelectedSampleMode.Name;
            _revitRepository.LintelsConfig.SaveProjectConfig();
        }
    }

    internal enum SampleMode {
        [Display(Name = "Выборка по всем элементам")]
        AllElements,
        [Display(Name = "Выборка по выделенным элементам")]
        SelectedElements,
        [Display(Name = "Выборка по текущему виду")]
        CurrentView
    }
}