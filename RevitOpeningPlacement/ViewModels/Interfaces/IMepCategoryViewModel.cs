using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitOpeningPlacement.ViewModels.Interfaces {
    internal interface IMepCategoryViewModel {
        string Name { get; set; }
        ObservableCollection<ISizeViewModel> MinSizes { get; set; }
        ObservableCollection<IOffsetViewModel> Offsets { get; set; }
    }
}
