using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoughFinishingDesign.Models;
using RevitRoughFinishingDesign.Services;

namespace RevitRoughFinishingDesign.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ILocalizationService _localizationService;
        private readonly ICurveLoopsSimplifier _curveLoopsSimplifier;
        private readonly WallDesignDataGetter _wallDesignDataGetter;
        private readonly CreatesLinesForFinishing _createsLinesForFinishing;
        private string _errorText;
        private string _saveProperty;

        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ILocalizationService localizationService,
            ICurveLoopsSimplifier curveLoopsSimplifier,
            WallDesignDataGetter wallDesignDataGetter,
            CreatesLinesForFinishing createsLinesForFinishing) {

            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _localizationService = localizationService;
            _curveLoopsSimplifier = curveLoopsSimplifier;
            _wallDesignDataGetter = wallDesignDataGetter;
            _createsLinesForFinishing = createsLinesForFinishing;
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }
        private string _lineOffset;
        public string LineOffset {
            get => _lineOffset;
            set {
                RaiseAndSetIfChanged(ref _lineOffset, value);
                OnPropertyChanged(nameof(ErrorText));
            }
        }
        public bool IsEditable => !IsAutomated;
        private bool _isAutomated;
        public bool IsAutomated {
            get => _isAutomated;
            set {
                RaiseAndSetIfChanged(ref _isAutomated, value);
                OnPropertyChanged(nameof(ErrorText));
                OnPropertyChanged(nameof(IsEditable));
            }
        }
        private ObservableCollection<WallTypeViewModel> _wallTypes = new ObservableCollection<WallTypeViewModel>();
        public ObservableCollection<WallTypeViewModel> WallTypes {
            get => _wallTypes;
            set => this.RaiseAndSetIfChanged(ref _wallTypes, value);
        }

        private ObservableCollection<LineStyleViewModel> _lineStyles = new ObservableCollection<LineStyleViewModel>();
        public ObservableCollection<LineStyleViewModel> LineStyles {
            get => _lineStyles;
            set => this.RaiseAndSetIfChanged(ref _lineStyles, value);
        }

        private ObservableCollection<PairViewModel> _items = new ObservableCollection<PairViewModel>();
        public ObservableCollection<PairViewModel> Items {
            get => _items;
            set => this.RaiseAndSetIfChanged(ref _items, value);
        }

        private WallTypeViewModel _selectedWallType;
        public WallTypeViewModel SelectedWallType {
            get => _selectedWallType;
            set {
                RaiseAndSetIfChanged(ref _selectedWallType, value);
                OnPropertyChanged(nameof(ErrorText));
            }
        }

        private LineStyleViewModel _selectedLineStyle;
        public LineStyleViewModel SelectedLineStyle {
            get => _selectedLineStyle;
            set {
                RaiseAndSetIfChanged(ref _selectedLineStyle, value);
                OnPropertyChanged(nameof(ErrorText));
            }
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string SaveProperty {
            get => _saveProperty;
            set => this.RaiseAndSetIfChanged(ref _saveProperty, value);
        }

        private void LoadView() {
            LoadConfig();
        }

        private void AcceptView() {
            SaveConfig();
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            using(var transaction = _revitRepository.Document.StartTransaction("Оформление черновой отделки")) {
                _createsLinesForFinishing.DrawLines(setting);
                transaction.Commit();
            }
        }

        private bool CanAcceptView() {
            if(!double.TryParse(LineOffset, out double result)) {
                ErrorText = "Смещение не должно содержать символов или букв";
                return false;
            }
            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
            LineOffset = setting?.LineOffset.GetValueOrDefault(0).ToString() ?? "0";
            var pairModels = setting?.PairModels ?? new List<PairModel>();
            IsAutomated = setting?.IsAutomated ?? false;
            WallTypes = new ObservableCollection<WallTypeViewModel>(
                _revitRepository.GetWallTypesInsideRoomsOnActiveView()
                .Select(fs => new WallTypeViewModel(fs))
                .OrderBy(fs => fs.Name));

            LineStyles = new ObservableCollection<LineStyleViewModel>(
                _revitRepository.GetAllLineStyles()
                .Select(fs => new LineStyleViewModel(fs))
                .OrderBy(fs => fs.Name));
            LineStyles.Insert(0, LineStyleViewModel.None);
            Items = GetRevitTypesData(WallTypes, LineStyles, pairModels);

            OnPropertyChanged(nameof(ErrorText));
        }

        private ObservableCollection<PairViewModel> GetRevitTypesData(
                ICollection<WallTypeViewModel> allWallTypes,
                ICollection<LineStyleViewModel> allLineStyles,
                ICollection<PairModel> pairModels) {
            var noneStyle = allLineStyles.FirstOrDefault(ls => ls.IsNone) ?? LineStyleViewModel.None;

            ObservableCollection<PairViewModel> items = new ObservableCollection<PairViewModel>(
                allWallTypes.Select(wt => new PairViewModel(wt, noneStyle)));

            foreach(PairViewModel item in items) {
                PairModel pairModel = pairModels.FirstOrDefault(pm => pm.WallTypeId == item.WallTypeVM.WallTypeId);
                if(pairModel != null) {
                    LineStyleViewModel lineStyle = allLineStyles
                        .FirstOrDefault(ls => ls.GraphicStyleId == pairModel.LineStyleId);

                    if(lineStyle != null && lineStyle.GraphicStyleId != item.LineStyleVM.GraphicStyleId) {
                        item.LineStyleVM = lineStyle;
                    }
                }
            }

            return items;
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.SaveProperty = SaveProperty;
            setting.LineOffset = double.Parse(LineOffset);
            setting.PairModels = Items
                .Select(p => new PairModel(p.WallTypeVM.WallTypeId, p.LineStyleVM.GraphicStyleId))
                .ToList();
            setting.IsAutomated = IsAutomated;
            _pluginConfig.SaveProjectConfig();
        }
    }
}
