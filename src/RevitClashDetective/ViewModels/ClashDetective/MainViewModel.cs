using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

using dosymep.Bim4Everyone;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels.Services;

namespace RevitClashDetective.ViewModels.ClashDetective {
    internal class MainViewModel : BaseViewModel {
        private string _errorText;

        private readonly ChecksConfig _checksConfig;
        private readonly FiltersConfig _filtersConfig;
        private readonly RevitRepository _revitRepository;
        private bool _canCancel = true;
        private string _messageText;
        private ObservableCollection<CheckViewModel> _checks;

        public MainViewModel(ChecksConfig checksConfig, FiltersConfig filtersConfig, RevitRepository revitRepository) {
            _filtersConfig = filtersConfig;
            _revitRepository = revitRepository;
            _checksConfig = checksConfig;

            if(_checksConfig != null && _checksConfig.Checks.Count > 0) {
                InitializeChecks();
            } else {
                InitializeEmptyCheck();
            }
            AddCheckCommand = RelayCommand.Create(AddCheck);
            RemoveCheckCommand = RelayCommand.Create<CheckViewModel>(RemoveCheck, CanRemove);
            FindClashesCommand = RelayCommand.Create(FindClashes, CanFindClashes);

            SaveClashesCommand = RelayCommand.Create(SaveConfig);
            SaveAsClashesCommand = RelayCommand.Create(SaveAsConfig);
            LoadClashCommand = RelayCommand.Create(LoadConfig);
        }

        public bool CanCancel {
            get => _canCancel;
            set => this.RaiseAndSetIfChanged(ref _canCancel, value);
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

        private void AddCheck() {
            Checks.Add(new CheckViewModel(_revitRepository, _filtersConfig));
        }

        private void RemoveCheck(CheckViewModel p) {
            if(Checks.Count > 0) {
                Checks.Remove(p);
            }
        }

        private bool CanRemove(CheckViewModel p) {
            return p != null;
        }

        private void FindClashes() {
            CanCancel = false;
            RenewConfig();
            _checksConfig.SaveProjectConfig();

            foreach(var check in Checks.Where(item => item.IsSelected)) {
                check.SaveClashes();
            }

            CanCancel = true;
            MessageText = "Проверка на коллизии прошла успешно";
            Wait(() => {
                foreach(var check in Checks) {
                    check.IsSelected = false;
                }
                MessageText = null;
                CanFindClashes();
            });
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
            _checksConfig.RevitVersion = ModuleEnvironment.RevitVersion;
        }

        private void SaveConfig() {
            RenewConfig();
            _checksConfig.SaveProjectConfig();
            MessageText = "Файл проверок успешно сохранен";
            Wait(() => { MessageText = null; });
        }

        private void SaveAsConfig() {
            RenewConfig();
            ConfigSaverService s = new ConfigSaverService(_revitRepository);
            s.Save(_checksConfig);
            MessageText = "Файл проверок успешно сохранен";
            Wait(() => { MessageText = null; });

        }

        private void LoadConfig() {
            var cl = new ConfigLoaderService(_revitRepository);
            var config = cl.Load<ChecksConfig>();
            cl.CheckConfig(config);

            var newChecks = InitializeChecks(config).ToList();
            var nameResolver = new NameResolver<CheckViewModel>(Checks, newChecks);
            Checks = new ObservableCollection<CheckViewModel>(nameResolver.GetCollection());
            MessageText = "Файл проверок успешно загружен";
            Wait(() => { MessageText = null; });
        }

        private bool HasSameNames() {
            return Checks.Select(item => item.Name)
                .GroupBy(item => item)
                .Any(item => item.Count() > 1);
        }

        private bool CanFindClashes() {
            if(HasSameNames()) {
                ErrorText = $"У проверок должны быть разные имена.";
                return false;
            }
            var emptyCheck = Checks.FirstOrDefault(item => !item.IsFilterSelected);
            if(emptyCheck != null) {
                ErrorText = $"У проверки \"{emptyCheck.Name}\" необходимо выбрать хотя бы один поисковый набор.";
                return false;
            }

            emptyCheck = Checks.FirstOrDefault(item => !item.IsFilesSelected);
            if(emptyCheck != null) {
                ErrorText = $"У проверки \"{emptyCheck.Name}\" необходимо выбрать хотя бы один файл.";
                return false;
            }
            if(Checks.All(item => !item.IsSelected)) {
                ErrorText = $"Для поиска коллизий должна быть выбрана хотя бы одна проверка.";
                return false;
            }

            ErrorText = null;
            return true;
        }

        private void Wait(Action action) {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 3);
            timer.Tick += (s, a) => { action(); timer.Stop(); };
            timer.Start();
        }
    }
}
