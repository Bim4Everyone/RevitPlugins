using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitRooms.ViewModels {
    internal class InfoElementViewModel : BaseViewModel {
        public IElementViewModel<Element> Element { get; set; }
        public ObservableCollection<string> Errors { get; set; }
    }
}
