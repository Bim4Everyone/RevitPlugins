using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.Interfaces {
    internal interface IMepCategoryViewModel {
        bool IsRound { get; set; }
        bool IsSelected { get; set; }
        string Name { get; set; }
        string ImageSource { get; }
        ObservableCollection<ISizeViewModel> MinSizes { get; set; }
        ObservableCollection<IOffsetViewModel> Offsets { get; set; }
        string GetErrorText();
        MepCategory GetMepCategory();
    }
}
