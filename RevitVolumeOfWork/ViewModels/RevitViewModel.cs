using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitVolumeOfWork.Models;

namespace RevitVolumeOfWork.ViewModels {
    internal abstract class RevitViewModel : BaseViewModel {
        protected readonly RevitRepository _revitRepository;

        public RevitViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            SetWallParametersCommand = new RelayCommand(SetWallParameters, CanSetWallParameters);

            Levels = new ObservableCollection<LevelViewModel>(GetLevelViewModels());
                //.OrderBy(item => item.Element.Elevation).Where(item => item.Rooms.Count > 0));
        }

        public ICommand SetWallParametersCommand { get; }

        public string Name { get; set; }

        public ObservableCollection<LevelViewModel> Levels { get; }

        protected abstract IEnumerable<LevelViewModel> GetLevelViewModels();

        private void SetWallParameters(object p) {
            var levels = Levels.Where(item => item.IsSelected);

            foreach(var level in levels) {
                foreach(var room in level.Rooms) {

                }
            }


        }

        private bool CanSetWallParameters(object p) { 
            

            return true; 
        }
    }
}
