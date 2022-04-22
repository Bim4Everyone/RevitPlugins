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

namespace RevitClashDetective.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private string _errorText;
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<CheckViewModel> _checks;

        public MainViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            InitializeEmptyCheck();
            AddCheckCommand = new RelayCommand(AddCheck);
            RemoveCheckCommand = new RelayCommand(RemoveCheck);
            FindClashesCommand = new RelayCommand(FindClashes, CanFindClashes);
        }
        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public ICommand AddCheckCommand { get; }
        public ICommand RemoveCheckCommand { get; }
        public ICommand FindClashesCommand { get; }

        public ObservableCollection<CheckViewModel> Checks {
            get => _checks;
            set => this.RaiseAndSetIfChanged(ref _checks, value);
        }


        private void InitializeEmptyCheck() {
            Checks = new ObservableCollection<CheckViewModel>() {
                new CheckViewModel(_revitRepository)
            };
        }

        private void AddCheck(object p) {
            Checks.Add(new CheckViewModel(_revitRepository));
        }

        private void RemoveCheck(object p) {
            if(Checks.Count > 0) {
                Checks.RemoveAt(Checks.Count - 1);
            }
        }

        private void FindClashes(object p) {
            Stopwatch sw = Stopwatch.StartNew();
            var clashes = Checks.SelectMany(item => item.GetClashes()).ToList();
            TaskDialog.Show("Revit", $"Время: " + sw.Elapsed.ToString() + $". Количество: { clashes.Count}");
            _revitRepository.SelectElements(clashes.Select(item => item.MainElement));
        }

        private bool CanFindClashes(object p) {
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