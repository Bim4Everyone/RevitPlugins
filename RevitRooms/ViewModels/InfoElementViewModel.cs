using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRooms.Views;

namespace RevitRooms.ViewModels {
    internal class InfoElementViewModel : BaseViewModel {
        public IElementViewModel<Element> Element { get; set; }
        public ObservableCollection<InfoMessage> Messages { get; set; }

        public string ImageSource { get; } = "pack://application:,,,/Resources/icons8-show-property-96.png";
    }

    internal class InfoMessage {
        public string Message { get; set; }
        public TypeInfo TypeInfo { get; set; }
    }
}
