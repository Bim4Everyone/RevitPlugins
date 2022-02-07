using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels {
    internal class LintelCollectionViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private ElementInWallKind _selectedElementKind;
        private ViewOrientation3D _orientation; //вряд ли здесь нужно хранить
        private ObservableCollection<LintelInfoViewModel> _lintelInfos;


        public LintelCollectionViewModel(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
            LintelInfos = new ObservableCollection<LintelInfoViewModel>();
            LintelsViewSource = new CollectionViewSource {Source = LintelInfos};
            //LintelsViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(LintelInfoViewModel.WallTypeName)));
            LintelsViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(LintelInfoViewModel.Level)));
            LintelsViewSource.Filter += ElementInWallKindFilter;
            SelectAndShowElementCommand = new RelayCommand(SelectElement, p => true);
            SelectNextCommand = new RelayCommand(SelectNext, p => true);
            SelectPreviousCommand = new RelayCommand(SelectPrevious, p => true);
            SelectionElementKindChangedCommand = new RelayCommand(SelectionElementKindChanged, p => true);
        }

        public ElementInWallKind SelectedElementKind {
            get => _selectedElementKind;
            set => this.RaiseAndSetIfChanged(ref _selectedElementKind, value);
        }

        public ICommand SelectionElementKindChangedCommand { get; }
        public ICommand SelectAndShowElementCommand { get; }
        public ICommand SelectNextCommand { get; }
        public ICommand SelectPreviousCommand { get; }

        public CollectionViewSource LintelsViewSource { get; set; }

        public ObservableCollection<LintelInfoViewModel> LintelInfos {
            get => _lintelInfos;
            set => this.RaiseAndSetIfChanged(ref _lintelInfos, value);
        }

        private void ElementInWallKindFilter(object sender, FilterEventArgs e) {
            if(e.Item is LintelInfoViewModel lintel) { 
                e.Accepted = lintel.ElementInWallKind == SelectedElementKind;
            }
        }

        private void SelectElement(object p) {
            if(!(p is ElementId id) || p == null) {
                return;
            }

            if(_revitRepository.IsActiveView3D()) {
                if(_orientation == null) {
                    _orientation = _revitRepository.GetOrientation3D();
                }
                _revitRepository.SelectAndShowElement(id, _orientation);
            } else {
                TaskDialog.Show("Revit", "Перейдите на 3D вид");
            }
        }

        private void SelectNext(object p) {
            LintelsViewSource.View.MoveCurrentToNext();
            var lintelInfo = LintelsViewSource.View.CurrentItem;
            if (lintelInfo is LintelInfoViewModel lintel) {
                SelectElement(lintel.LintelId);
            }
        }

        private void SelectPrevious(object p) {
            LintelsViewSource.View.MoveCurrentToPrevious();
            var lintelInfo = LintelsViewSource.View.CurrentItem;
            if(lintelInfo is LintelInfoViewModel lintel) {
                SelectElement(lintel.LintelId);
            }
        }

        private void SelectionElementKindChanged(object p) {
            LintelsViewSource.View.Refresh();
        }
    }
}
