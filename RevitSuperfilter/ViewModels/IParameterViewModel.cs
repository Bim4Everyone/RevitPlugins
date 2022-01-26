using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitSuperfilter.ViewModels {
    internal interface IParameterViewModel : ISelectableElement, INotifyPropertyChanged {
        int Count { get; }
        ObservableCollection<Element> Elements { get; }

        IEnumerable<Element> GetSelectedElements();
    }
}