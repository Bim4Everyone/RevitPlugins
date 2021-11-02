using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using RevitRooms.Models;

namespace RevitRooms.ViewModels {
    internal interface IElementViewModel<out TElement> {
        TElement Element { get; }
        RevitRepository RevitRepository { get; }

        string Name { get; }
        ElementId ElementId { get; }
        

        ICommand ShowElementCommand { get; }
        ICommand SelectElementCommand { get; }
    }
}
