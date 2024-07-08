using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private const double _minOffsetMm = 0;
        private const double _maxOffsetMm = 1000;


        public MainViewModel(
            PluginConfig pluginConfig,
            RevitRepository revitRepository,
            ViewTemplatesViewModel viewTemplatesViewModel,
            ApartmentsViewModel apartmentsViewModel) {

            _pluginConfig = pluginConfig ?? throw new System.ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));

            ViewTemplatesViewModel = viewTemplatesViewModel
                ?? throw new System.ArgumentNullException(nameof(viewTemplatesViewModel));
            ApartmentsViewModel = apartmentsViewModel
                ?? throw new System.ArgumentNullException(nameof(apartmentsViewModel));
            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        }

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

            ErrorText = null;
            return true;
        }

        private void LoadConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

            OffsetMm = setting?.OffsetMm ?? 0;
        }

        private void SaveConfig() {
            RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                    ?? _pluginConfig.AddSettings(_revitRepository.Document);

            _pluginConfig.SaveProjectConfig();
        }
    }
}
