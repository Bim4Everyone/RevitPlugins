﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement.ViewModels.ReportViewModel {
    internal class ClashesViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        public ClashesViewModel(RevitRepository revitRepository, IEnumerable<UnplacedClashModel> clashes) {
            _revitRepository = revitRepository;
            Clashes = new List<ClashViewModel>(clashes.Select(item => new ClashViewModel(item)));
            ClashesViewSource = new CollectionViewSource() { Source = Clashes };

            SelectCommand = new RelayCommand(Select, CanSelect);
            SelectionDataChangedCommand = new RelayCommand(SelectionChanged);
        }

        public List<ClashViewModel> Clashes { get; set; }
        public CollectionViewSource ClashesViewSource { get; set; }
        public ICommand SelectCommand { get; }
        public ICommand SelectionDataChangedCommand { get; }

        private void SelectionChanged(object p) {
            if(ClashesViewSource.View.CurrentPosition > -1
                && ClashesViewSource.View.CurrentPosition < Clashes.Count) {
                Select(ClashesViewSource.View.CurrentItem);
            }
        }

        private void Select(object p) {
            var clash = (ClashViewModel) p;
            var elements = new[]{
                _revitRepository.GetElement(clash.Clash.MainElement.DocumentName, clash.Clash.MainElement.Id),
                _revitRepository.GetElement(clash.Clash.OtherElement.DocumentName, clash.Clash.OtherElement.Id)
            };
            _revitRepository.SelectAndShowElement(elements);
        }

        private bool CanSelect(object p) {
            return (p as ClashViewModel) != null;
        }
    }
}
