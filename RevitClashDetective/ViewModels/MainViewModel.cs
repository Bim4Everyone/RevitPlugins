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
            _checksConfig = checksConfig;
            _filtersConfig = filtersConfig;
            _revitRepository = revitRepository;
            if(checksConfig.Checks.Count > 0) {
                InitializeChecks();
            } else {
                InitializeEmptyCheck();
            }
            AddCheckCommand = new RelayCommand(AddCheck);
            RemoveCheckCommand = new RelayCommand(RemoveCheck);
            FindClashesCommand = new RelayCommand(FindClashes, CanFindClashes);
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

        public ObservableCollection<CheckViewModel> Checks {
            get => _checks;
            set => this.RaiseAndSetIfChanged(ref _checks, value);
        }


        private void InitializeChecks() {
            Checks = new ObservableCollection<CheckViewModel>();
            foreach(var check in _checksConfig.Checks) {
                Checks.Add(new CheckViewModel(_revitRepository, _filtersConfig, check));
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
                Checks.RemoveAt(Checks.Count - 1);
            }
        }

        private async void FindClashes(object p) {
            foreach(var check in Checks.Where(item => item.IsSelected)) {
                check.SaveClashes();
            }
            SaveConfig();
            MessageText = "Проверка на коллизии прошла успешно";
            await Task.Delay(3000);
            MessageText = null;
            foreach(var check in Checks) {
                check.IsSelected = false;
            }
        }

        private void SaveConfig() {
            _checksConfig.Checks = new List<Check>();
            foreach(var check in Checks) {
                _checksConfig.Checks.Add(new Check() {
                    Name = check.Name,
                    MainFilters = check.MainDocumentProviders.Where(item => item.IsSelected).Select(item => item.Name).ToList(),
                    OtherFilters = check.OtherDocumentProviders.Where(item => item.IsSelected).Select(item => item.Name).ToList()
                });
            }
            _checksConfig.SaveProjectConfig();
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
            ErrorText = null;
            return true;
        }
    }
}