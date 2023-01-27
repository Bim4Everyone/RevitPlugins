using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Models;

namespace RevitCheckingLevels.ViewModels {
    internal class LevelViewModel : BaseViewModel {
        private readonly Level _level;
        private readonly RevitRepository _revitRepository;

        public LevelViewModel(Level level, RevitRepository revitRepository) {
            _level = level;
            _revitRepository = revitRepository;
        }

        public string Name => _level.Name;

#if REVIT_2020_OR_LESS
        
        public double Elevation =>
            UnitUtils.ConvertFromInternalUnits(_level.Elevation, DisplayUnitType.DUT_MILLIMETERS);

#else
    
        public double Elevation =>
            UnitUtils.ConvertFromInternalUnits(_level.Elevation, UnitTypeId.Millimeters);

#endif

        public ObservableCollection<string> LevelErrors { get; }
    }
}
