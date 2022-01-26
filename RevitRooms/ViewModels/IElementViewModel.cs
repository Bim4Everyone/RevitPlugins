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
        bool IsSelected { get; set; }

        TElement Element { get; }
        RevitRepository RevitRepository { get; }

        ElementId ElementId { get; }

        string Name { get; }
        string PhaseName { get; }
        string LevelName { get; }
        string CategoryName { get; }

        ICommand ShowElementCommand { get; }
        ICommand SelectElementCommand { get; }

        ICommand SelectElementsCommand { get; }
        ICommand UnselectElementsCommand { get; }
    }
}
