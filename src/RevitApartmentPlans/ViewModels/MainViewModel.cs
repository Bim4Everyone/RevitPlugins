using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitApartmentPlans.Models;
using RevitApartmentPlans.Services;

namespace RevitApartmentPlans.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IViewPlanCreationService _viewPlanCreationService;
        private readonly ILengthConverter _lengthConverter;
        private readonly IProgressDialogFactory _progressDialogFactory;
        private const double _minOffsetMm = 0;
        private const double _maxOffsetMm = 2000;


        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ViewTemplatesViewModel viewTemplatesViewModel,
            ApartmentsViewModel apartmentsViewModel,
            IMessageBoxService messageBoxService,
            IViewPlanCreationService viewPlanCreationService,
            ILengthConverter lengthConverter,
            IProgressDialogFactory progressDialogFactory
            ) {

            _pluginConfig = pluginConfig
                ?? throw new System.ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository
                ?? throw new System.ArgumentNullException(nameof(revitRepository));
            ViewTemplatesViewModel = viewTemplatesViewModel
                ?? throw new System.ArgumentNullException(nameof(viewTemplatesViewModel));
            ApartmentsViewModel = apartmentsViewModel
                ?? throw new System.ArgumentNullException(nameof(apartmentsViewModel));
            _messageBoxService = messageBoxService
                ?? throw new System.ArgumentNullException(nameof(messageBoxService));
            _viewPlanCreationService = viewPlanCreationService
                ?? throw new System.ArgumentNullException(nameof(viewPlanCreationService));
            _lengthConverter = lengthConverter
                ?? throw new System.ArgumentNullException(nameof(lengthConverter));
            _progressDialogFactory = progressDialogFactory
                ?? throw new System.ArgumentNullException(nameof(progressDialogFactory));
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

        public IProgressDialogFactory ProgressDialogFactory => _progressDialogFactory;
        public IMessageBoxService MessageBoxService => _messageBoxService;
        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        private string _errorText;
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }


        public ViewTemplatesViewModel ViewTemplatesViewModel { get; }

        public ApartmentsViewModel ApartmentsViewModel { get; }

        private double _offsetMm;
        public double OffsetMm {
            get => _offsetMm;
            set => RaiseAndSetIfChanged(ref _offsetMm, value);
        }


        private void LoadView() {
            LoadConfig();
        }

        private void AcceptView() {
            SaveConfig();
            var selectedApartments = ApartmentsViewModel.Apartments
                    .Where(a => a.IsSelected)
                    .Select(vm => vm.GetApartment())
                    .ToArray();
            var selectedTemplates = ViewTemplatesViewModel.ViewTemplates
                    .Select(t => t.GetTemplate())
                    .ToArray();
            using(var progressDialogService = _progressDialogFactory.CreateDialog()) {
                progressDialogService.StepValue = 1;
                progressDialogService.DisplayTitleFormat = "Обработка квартир... [{0}]\\[{1}]";
                var progress = progressDialogService.CreateProgress();
                progressDialogService.MaxValue = selectedApartments.Length;
                var ct = progressDialogService.CreateCancellationToken();
                progressDialogService.Show();

                _viewPlanCreationService.CreateViews(
                    selectedApartments,
                    selectedTemplates,
                    _lengthConverter.ConvertToInternal(OffsetMm),
                    progress,
                    ct);
            }

        }

        private bool CanAcceptView() {
            if(OffsetMm < _minOffsetMm) {
                ErrorText = $"Отступ не должен быть меньше {_minOffsetMm} мм.";
                return false;
            }
            if(OffsetMm > _maxOffsetMm) {
                ErrorText = $"Отступ не может быть больше {_maxOffsetMm} мм.";
                return false;
            }
            if(ApartmentsViewModel?.SelectedParam is null) {
                ErrorText = "Выберите параметр для группировки.";
                return false;
            }
            if(ViewTemplatesViewModel?.ViewTemplates?.Count() == 0) {
                ErrorText = "Добавьте шаблоны видов.";
                return false;
            }
            if(ApartmentsViewModel?.Apartments?.Where(a => a.IsSelected).Count() == 0) {
                ErrorText = "Выберите квартиры.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            OffsetMm = setting?.OffsetMm ?? 200;
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);
            setting.OffsetMm = OffsetMm;
            setting.ParamName = ApartmentsViewModel?.SelectedParam?.Name ?? string.Empty;
            setting.ViewTemplates = ViewTemplatesViewModel?.ViewTemplates?.Select(t => t.GetTemplate().Id).ToArray();
            _pluginConfig.SaveProjectConfig();
        }
    }
}
