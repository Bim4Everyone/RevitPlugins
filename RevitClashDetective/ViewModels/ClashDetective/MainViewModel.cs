using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

namespace RevitClashDetective.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private string _errorText;

        private readonly ChecksConfig _checksConfig;
        private readonly FiltersConfig _filtersConfig;
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<CheckViewModel> _checks;
        private string _messageText;

        public MainViewModel(ChecksConfig checksConfig, FiltersConfig filtersConfig, RevitRepository revitRepository) {
            _filtersConfig = filtersConfig;
            _revitRepository = revitRepository;
            _checksConfig = checksConfig;

            if(_checksConfig != null && _checksConfig.Checks.Count > 0) {
                InitializeChecks();
            } else {
                InitializeEmptyCheck();
            }
            AddCheckCommand = new RelayCommand(AddCheck);
            RemoveCheckCommand = new RelayCommand(RemoveCheck, CanRemove);
            FindClashesCommand = new RelayCommand(FindClashes, CanFindClashes);

            SaveClashesCommand = new RelayCommand(SaveConfig);
            SaveAsClashesCommand = new RelayCommand(SaveAsConfig);
            LoadClashCommand = new RelayCommand(LoadConfig);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public string MessageText {
            get => _messageText;
            set => this.RaiseAndSetIfChanged(ref _messageText, value);
        }

        public ICommand AddCheckCommand { get; }
        public ICommand RemoveCheckCommand { get; }
        public ICommand FindClashesCommand { get; }
        public ICommand SaveAsClashesCommand { get; }
        public ICommand SaveClashesCommand { get; }
        public ICommand LoadClashCommand { get; }

        public ObservableCollection<CheckViewModel> Checks {
            get => _checks;
            set => this.RaiseAndSetIfChanged(ref _checks, value);
        }

        private void InitializeChecks() {
            Checks = new ObservableCollection<CheckViewModel>(InitializeChecks(_checksConfig));
        }

        private IEnumerable<CheckViewModel> InitializeChecks(ChecksConfig config) {
            foreach(var check in config.Checks) {
                yield return new CheckViewModel(_revitRepository, _filtersConfig, check);
            }
        }

        private void InitializeEmptyCheck() {
            Checks = new ObservableCollection<CheckViewModel>() {
                new CheckViewModel(_revitRepository, _filtersConfig)
            };
        }

        private void AddCheck(object p) {
            Checks.Add(new CheckViewModel(_revitRepository, _filtersConfig));
        }

        private void RemoveCheck(object p) {
            if(Checks.Count > 0) {
                Checks.Remove(p as CheckViewModel);
            }
        }

        private bool CanRemove(object p) {
            return (p as CheckViewModel) != null;
        }

        private async void FindClashes(object p) {
            foreach(var check in Checks.Where(item => item.IsSelected)) {
                check.SaveClashes();
            }
            RenewConfig();
            _checksConfig.SaveProjectConfig();
            MessageText = "Проверка на коллизии прошла успешно";
            foreach(var check in Checks) {
                check.IsSelected = false;
            }
            await Task.Delay(3000);
            MessageText = null;
        }

        private void RenewConfig() {
            _checksConfig.Checks = new List<Check>();
            foreach(var check in Checks) {
                _checksConfig.Checks.Add(new Check() {
                    Name = check.Name,
                    FirstSelection = check.FirstSelection.GetCheckSettings(),
                    SecondSelection = check.SecondSelection.GetCheckSettings()
                });
            }
        }

        private async void SaveConfig(object p) {
            RenewConfig();
            _checksConfig.SaveProjectConfig();
            MessageText = "Файл проверок успешно сохранен";
            await Task.Delay(3000);
            MessageText = null;
        }

        private async void SaveAsConfig(object p) {
            RenewConfig();
            ConfigSaver s = new ConfigSaver();
            if(s.Save(_checksConfig)) {
                MessageText = "Файл проверок успешно сохранен";
                await Task.Delay(3000);
                MessageText = null;
            }
        }

        private async void LoadConfig(object p) {
            var cl = new ConfigLoader();
            var config = cl.Load<ChecksConfig>();
            if(config == null) {
                return;
            }
            var newChecks = InitializeChecks(config).ToList();
            var nameResolver = new NameResolver<CheckViewModel>(Checks, newChecks);
            Checks = new ObservableCollection<CheckViewModel>(nameResolver.GetCollection());
            MessageText = "Файл проверок успешно загружен";
            await Task.Delay(3000);
            MessageText = null;
        }

        private bool HasSameNames() {
            return Checks.Select(item => item.Name)
                .GroupBy(item => item)
                .Any(item => item.Count() > 1);
        }

        private bool CanFindClashes(object p) {
            if(HasSameNames()) {
                ErrorText = $"У проверок должны быть разные имена.";
                return false;
            }
            var emptyCheck = Checks.FirstOrDefault(item => !item.IsFilterSelected);
            if(emptyCheck != null) {
                ErrorText = $"У проверки \"{emptyCheck.Name}\" необходимо выбрать хотя бы один фильтр.";
                return false;
            }

            emptyCheck = Checks.FirstOrDefault(item => !item.IsFilesSelected);
            if(emptyCheck != null) {
                ErrorText = $"У проверки \"{emptyCheck.Name}\" необходимо выбрать хотя бы один файл.";
                return false;
            }

            ErrorText = null;
            return true;
        }
    }
}