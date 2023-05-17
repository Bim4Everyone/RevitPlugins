using System.Collections.Generic;
using System.Collections.ObjectModel;

using RevitDeleteUnused.Models;

namespace RevitDeleteUnused.Commands {
    internal class CheckBoxCommands {
        public static void SetAll(List<ElementToDelete> AllLinks, bool value) {
            foreach(var element in AllLinks) { element.IsChecked = value; }
        }

        public static void InvertAll(List<ElementToDelete> AllLinks) {
            foreach(var element in AllLinks) { element.IsChecked = !element.IsChecked; }
        }
    }
}
