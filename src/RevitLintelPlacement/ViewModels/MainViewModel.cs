using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;
using RevitLintelPlacement.Models.ElementInWallProviders;
using RevitLintelPlacement.Models.LintelsProviders;
using RevitLintelPlacement.ViewModels.SampleModeViewModels;
using RevitLintelPlacement.Views;

namespace RevitLintelPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private GroupedRuleCollectionViewModel _groupedRules;
        private ObservableCollection<LinkViewModel> _links;
        private ElementInfosViewModel _elementInfosViewModel;
        private string _errorText;
        private List<LinkViewModel> _selectedLinks = new List<LinkViewModel>();
        private List<SampleModeViewModel> _sampleModes;
        private SampleModeViewModel _selectedSampleMode;

        public MainViewModel() { }

        public MainViewModel(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
            ElementInfos = new ElementInfosViewModel(_revitRepository);

            InitializeLinks();
            InitializeSampleModes();
            ApplySettings();

            GroupedRules = new GroupedRuleCollectionViewModel(_revitRepository, ElementInfos);

            PlaceLintelCommand = new RelayCommand(PlaceLintels, CanPlace);
            ShowReportCommand = new RelayCommand(ShowReport);
            CloseCommand = new RelayCommand(Close);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public GroupedRuleCollectionViewModel GroupedRules {
            get => _groupedRules;
            set => this.RaiseAndSetIfChanged(ref _groupedRules, value);
        }

        public ObservableCollection<LinkViewModel> Links {
            get => _links;
            set => this.RaiseAndSetIfChanged(ref _links, value);
        }

        public ElementInfosViewModel ElementInfos {
            get => _elementInfosViewModel;
            set => this.RaiseAndSetIfChanged(ref _elementInfosViewModel, value);
        }

        public List<LinkViewModel> SelectedLinks {
            get => _selectedLinks;
            set => _selectedLinks = value;
        }

        public List<SampleModeViewModel> SampleModes {
            get => _sampleModes;
            set => _sampleModes = value;
        }

        public SampleModeViewModel SelectedSampleMode {
            get => _selectedSampleMode;
            set => this.RaiseAndSetIfChanged(ref _selectedSampleMode, value);
        }

        public ICommand PlaceLintelCommand { get; set; }
        public ICommand ShowReportCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        private void InitializeLinks() {
            var links = _revitRepository.GetLinkTypes().ToList();
            if(links.Count > 0) {
                Links = new ObservableCollection<LinkViewModel>(links.Select(l => new LinkViewModel() { Name = Path.GetFileNameWithoutExtension(l.Name) }));
            } else {
                Links = new ObservableCollection<LinkViewModel>();
            }
        }

        private void InitializeSampleModes() {
            var view = _revitRepository.GetCurrentView();
            SampleModes = new List<SampleModeViewModel>() {
                new SampleModeViewModel("Выборка по всем элементам",
                    new AllLintelsProvider(_revitRepository),
                    new AllElementsInWallProvider(_revitRepository, ElementInfos)),
                new SampleModeViewModel("Выборка по выделенным элементам",
                    new SelectedElementsInWallWithLintelsProvider(_revitRepository, new SelectedElementsInWallProvider(_revitRepository, ElementInfos)),
                    new SelectedElementsInWallProvider(_revitRepository, ElementInfos)),
                new SampleModeViewModel("Выборка по текущему виду",
                    new ViewLintelsProvider(_revitRepository, view),
                    new CurrentViewElementsInWallProvider(_revitRepository, ElementInfos, view)),
            };
        }

        private void ApplySettings() {
            SelectedLinks = new List<LinkViewModel>();

            var settings = _revitRepository.LintelsConfig.GetSettings(_revitRepository.GetDocumentName());
            if(settings != null) {
                SelectedSampleMode = SampleModes.FirstOrDefault(item => item.Name.Equals(settings.SelectedModeNameRules, StringComparison.CurrentCulture))
                    ?? SampleModes.FirstOrDefault();
                if(settings.SelectedLinks != null && settings.SelectedLinks.Count > 0) {
                    SelectedLinks = Links.Where(item => settings.SelectedLinks.Any(sl => sl.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase))).ToList();
                }
            } else {
                SelectedSampleMode = SampleModes.FirstOrDefault();
            }
        }

        private void PlaceLintels(object p) {
            if(!CheckConfig()) {
                return;
            }

            ICollection<LintelInfoViewModel> oldLintels = GetLintels();
            ICollection<FamilyInstance> elementsInWalls = GetElementsInWalls(oldLintels);

            //проверка старых перемычек
            View3D view3D = GetView();
            var links = SelectedLinks.Select(l => l.Name).ToList();
            CheckOldLintels(links, oldLintels);

            //расстановка перемычек
            var lintels = GetPlacedLintels(elementsInWalls, view3D, links);

            //фиксация перемычек
            var elevation = _revitRepository.GetElevation();
            if(!CheckView(elevation, "Фасад")) {
                return;
            }
            var plan = _revitRepository.GetPlan();
            if(!CheckView(elevation, "План")) {
                return;
            }
            LockPlacedLintels(view3D, elevation, plan, lintels);

            SaveRules();

            //отчет об ошибках
            if(ElementInfos.ElementInfos != null && ElementInfos.ElementInfos.Count > 0) {
                ShowReport();
            }
        }

        private bool CanPlace(object p) {
            var result = true;
            if(GroupedRules.SelectedRule == null) {
                return false;
            }
            foreach(var groupedRule in GroupedRules.SelectedRule.Rules) {
                if(!groupedRule.UpdateErrorText()) {
                    result = false;
                }
            }
            ErrorText = GroupedRules.SelectedRule.GetErrorText();
            if(!string.IsNullOrEmpty(ErrorText)) {
                result = false;
            }
            return result;
        }

        private bool CheckConfig() {
            ElementInfos.ElementInfos.Clear();

            if(!_revitRepository.CheckConfigParameters(ElementInfos)) {
                ShowReport();
                return false;
            }

            if(_revitRepository.GetLintelTypes().Any(item => !_revitRepository.CheckLintelType(item, ElementInfos))) {
                ShowReport();
                return false;
            }

            return true;
        }

        private void ShowReport() {
            ElementInfos.UpdateCollection();
            ElementInfos.UpdateGroupedMessage();
            ElementInfos.ElementInfosViewSource?.View?.Refresh();
            var view = new ReportView() { DataContext = ElementInfos };
            view.Show();
        }

        private List<LintelInfoViewModel> GetLintels() {
            var correlator = new LintelElementCorrelator(_revitRepository, SelectedSampleMode.ElementsInWallProvider);
            return SelectedSampleMode.LintelsProvider.GetLintels()
                .Select(item => new LintelInfoViewModel(_revitRepository, item, correlator.Correlate(item)))
                .ToList();
        }

        private ICollection<FamilyInstance> GetElementsInWalls(ICollection<LintelInfoViewModel> oldLintels) {
            var wallTypeNames = GroupedRules.SelectedRule.Rules.SelectMany(item => item.WallTypes.WallTypes.Where(w => w.IsChecked).Select(w => w.Name)).ToList();

            var elementsInWalls = SelectedSampleMode.ElementsInWallProvider.GetElementsInWall();

            foreach(var lintel in oldLintels) {
                var element = elementsInWalls.FirstOrDefault(e => e.Id == lintel.ElementInWallId);
                if(element != null) {
                    elementsInWalls.Remove(element);
                }
            }

            return elementsInWalls;
        }

        private View3D GetView() {
            var view3D = _revitRepository.GetView3D();
            using(Transaction t = _revitRepository.StartTransaction("Подготовка к расстановке перемычек")) {
                if(view3D.IsSectionBoxActive) {
                    view3D.IsSectionBoxActive = false;
                }
                t.Commit();
            }
            _revitRepository.ActivateView();
            return view3D;
        }

        private void CheckOldLintels(ICollection<string> links, ICollection<LintelInfoViewModel> oldLintels) {
            LintelChecker lc = new LintelChecker(_revitRepository, GroupedRules, links, ElementInfos);

            using(Transaction t = _revitRepository.StartTransaction("Проверка расставленных перемычек")) {
                lc.Check(oldLintels);
                t.Commit();
            }
        }

        private IEnumerable<LintelInfoViewModel> GetPlacedLintels(IEnumerable<FamilyInstance> elementInWalls, View3D view3D, IEnumerable<string> links) {
            List<LintelInfoViewModel> lintels;
            using(var pb = GetPlatformService<IProgressDialogService>()) {
                pb.StepValue = 10;
                pb.DisplayTitleFormat = "Идёт расчёт... [{0}\\{1}]";
                var progress = pb.CreateProgress();
                pb.MaxValue = elementInWalls.Count();
                var ct = pb.CreateCancellationToken();

                pb.Show();

                using(Transaction t = _revitRepository.StartTransaction("Расстановка перемычек")) {
                    lintels = PlaceLintels(elementInWalls, view3D, links, progress, ct).ToList();
                    t.Commit();
                }
            }

            return lintels;
        }

        private IEnumerable<LintelInfoViewModel> PlaceLintels(IEnumerable<FamilyInstance> elementInWalls, View3D view3D, IEnumerable<string> links, IProgress<int> progress, CancellationToken ct) {
            int count = 0;
            foreach(var elementInWall in elementInWalls) {
                progress.Report(count++);
                ct.ThrowIfCancellationRequested();
                var elementInWallFixation = elementInWall.GetParamValueOrDefault(_revitRepository.LintelsCommonConfig.OpeningFixation, 0);
                var value = (int) elementInWallFixation;
                if(value == 1) {
                    continue;
                }
                var rule = GroupedRules.GetRule(elementInWall);
                if(rule == null)
                    continue;
                if(!_revitRepository.CheckUp(view3D, elementInWall, links))
                    continue;
                if(string.IsNullOrEmpty(rule.SelectedLintelType)) {
                    TaskDialog.Show("Предупреждение!", "В проект не загружено семейство перемычки.");
                    yield break;
                }
                var lintelType = _revitRepository.GetLintelType(rule.SelectedLintelType);
                if(lintelType == null) {
                    TaskDialog.Show("Предупреждение!", $"В проект не загружено семейство с выбранным типоразмером \"{rule.SelectedLintelType}\".");
                    yield break;
                }
                var lintel = _revitRepository.PlaceLintel(lintelType, elementInWall);
                rule.SetParametersTo(lintel, elementInWall);

                var offset = rule.LintelRightOffsetParameter.RightOffsetInternal;

                if(_revitRepository.DoesLeftCornerNeeded(view3D, elementInWall, links, ElementInfos, offset, out double leftOffset)) {
                    lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelLeftOffset, leftOffset > 0 ? leftOffset : 0);
                    lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelLeftCorner, 1);
                } else {
                    lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelLeftOffset, leftOffset > 0 && leftOffset <= offset ? leftOffset : offset);
                    lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelLeftCorner, 0);
                }
                if(_revitRepository.DoesRightCornerNeeded(view3D, elementInWall, links, ElementInfos, offset, out double rightOffset)) {
                    lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightOffset, rightOffset > 0 ? rightOffset : 0);
                    lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightCorner, 1);
                } else {
                    lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightCorner, 0);
                    lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightOffset, rightOffset > 0 && rightOffset <= offset ? rightOffset : offset);

                }
                yield return new LintelInfoViewModel(_revitRepository, lintel, elementInWall);
            }
        }

        private bool CheckView(View elevation, string viewName) {
            if(elevation == null) {
                ElementInfos.ElementInfos.Add(new ElementInfoViewModel(ElementId.InvalidElementId, InfoElement.LackOfView.FormatMessage(viewName)));
                ShowReport();
                return false;
            }
            return true;
        }

        private void LockPlacedLintels(View3D view3D, View elevation, View plan, IEnumerable<LintelInfoViewModel> lintels) {
            using(Transaction t = _revitRepository.StartTransaction("Закрепление перемычек")) {
                foreach(var lintel in lintels) {
                    _revitRepository.LockLintel(view3D, elevation, plan, lintel.Lintel, lintel.ElementInWall);
                }
                t.Commit();
            }
        }

        private void SaveRules() {
            if(GroupedRules.SelectedRule.RuleConfigManager.CanSave) {
                GroupedRules.SelectedRule.SaveCommand.Execute(null);
            }
        }

        private void ShowReport(object p) {
            ShowReport();
        }

        private void Close(object p) {
            var settings = _revitRepository.LintelsConfig.GetSettings(_revitRepository.GetDocumentName());
            if(settings == null) {
                settings = _revitRepository.LintelsConfig.AddSettings(_revitRepository.GetDocumentName());
            }
            settings.SelectedPath = GroupedRules?.SelectedRule?.Name;
            settings.SelectedModeNameRules = SelectedSampleMode.Name;
            if(SelectedLinks != null) {
                settings.SelectedLinks = SelectedLinks.Select(l => l.Name).ToList();
            } else {
                settings.SelectedLinks = new List<string>();
            }

            _revitRepository.LintelsConfig.SaveProjectConfig();
        }
    }
}