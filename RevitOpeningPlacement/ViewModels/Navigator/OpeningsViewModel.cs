using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using DevExpress.Mvvm;
using DevExpress.Mvvm.Xpf;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    internal class OpeningsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ObservableCollection<OpeningViewModel> _openings;
        private WallOpeningGroupPlacerInitializer _wallOpeningGroupPlacerInitializer = new WallOpeningGroupPlacerInitializer();
        private FloorOpeningGroupPlacerInitializer _floorOpeningGroupPlacerInitializer = new FloorOpeningGroupPlacerInitializer();
        private OpeningViewModel _selectedItem;

        public OpeningsViewModel(Models.RevitRepository revitRepository, ICollection<OpeningsGroup> openingsGroups) {
            _revitRepository = revitRepository;

            InitializeOpenings(openingsGroups);

            SelectCommand = new RelayCommand(Select);
            SelectionChangedCommand = new RelayCommand(SelectionChanged, CanSelect);
            UniteCommand = new RelayCommand(Unite, CanUnite);
            RenewCommand = new RelayCommand(Renew);
        }

        public ObservableCollection<OpeningViewModel> Openings {
            get => _openings;
            set => this.RaiseAndSetIfChanged(ref _openings, value);
        }

        public CollectionViewSource OpeningsViewSource { get; set; }
        public OpeningViewModel SelectedItem {
            get => _selectedItem;
            set => this.RaiseAndSetIfChanged(ref _selectedItem, value);
        }

        public ICommand SelectCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand UniteCommand { get; }
        public ICommand RenewCommand { get; }

        private void InitializeOpenings(ICollection<OpeningsGroup> openingsGroups) {
            var openings = openingsGroups.SelectMany(item => OpeningViewModel.GetOpenings(_revitRepository, item)).ToList();
            var parentsIds = openings.Select(item => item.ParentId).Distinct().ToArray();

            Openings = new ObservableCollection<OpeningViewModel>(openings.OrderByDescending(item => parentsIds.FirstOrDefault(id => id == item.ParentId)));

            OpeningsViewSource = new CollectionViewSource() { Source = Openings };
        }

        private void Select(object p) {
            if(!(p is OpeningViewModel opening))
                return;
            var elements = new[] { _revitRepository.GetElement(new ElementId(opening.Id)) };
            _revitRepository.SelectAndShowElement(elements);
        }

        private void SelectElements(ICollection<OpeningViewModel> openings) {
            var elements = openings
                .Select(item => _revitRepository.GetElement(new ElementId(item.Id)))
                .ToArray();
            _revitRepository.SelectAndShowElement(elements);
        }

        private void SelectionChanged(object p) {
            if(OpeningsViewSource.View.CurrentPosition > -1
                && OpeningsViewSource.View.CurrentPosition < Openings.Count) {
                SelectElements((ObservableCollection<OpeningViewModel>) p);
            }
        }

        private bool CanSelect(object p) {
            return p is ObservableCollection<OpeningViewModel>;
        }

        private async void Unite(object p) {
            var elements = (ObservableCollection<OpeningViewModel>) p;
            FamilyInstance createdOpening = await PlaceUnitedOpenings(elements);

            RemoveOldOpenings(elements);

            var opening = new OpeningViewModel(_revitRepository, createdOpening);
            Openings.Add(opening);
            SelectedItem = opening;
        }

        private void RemoveOldOpenings(ObservableCollection<OpeningViewModel> elements) {
            var ids = elements.Select(item => item.Id).ToArray();
            var openings = Openings.ToList();

            var indexes = elements.Select(item => Openings.IndexOf(item)).OrderByDescending(item => item).ToArray();
            foreach(var index in indexes) {
                Openings.RemoveAt(index);
            }
        }

        private async Task<FamilyInstance> PlaceUnitedOpenings(ObservableCollection<OpeningViewModel> elements) {
            var familyName = elements.First().FamilyName;
            OpeningPlacer placer = null;
            var instances = elements
                .Select(item => _revitRepository.GetElement(new ElementId(item.Id)))
                .OfType<FamilyInstance>()
                .ToArray();
            if(RevitRepository.FloorFamilyNames.Any(item => item.Equals(familyName, StringComparison.CurrentCulture))) {
                placer = _floorOpeningGroupPlacerInitializer.GetPlacer(_revitRepository, new OpeningsGroup(instances));
            } else {
                placer = _wallOpeningGroupPlacerInitializer.GetPlacer(_revitRepository, new OpeningsGroup(instances));
            }

            return await _revitRepository.UniteOpenings(placer, instances);
        }

        private bool CanUnite(object p) {
            return p is ObservableCollection<OpeningViewModel> && ((ObservableCollection<OpeningViewModel>) p).Any();
        }

        private void Renew(object p) {
            Action action = () => {
                var command = new GetOpeningTaskCommand();
                command.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }
    }
}