using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    public class PriorityViewModel {
        private readonly int _ordinalNumber;
        private readonly string _name;
        private readonly string _isSummer;

        public PriorityViewModel(RoomPriority priority) {
            _ordinalNumber = priority.OrdinalNumber;
            _name = priority.Name;

            if(priority.IsSummer) {
                _isSummer = "Да";
            } else {
                _isSummer = "Нет";
            }
        }

        public int OrdinalNumber => _ordinalNumber;
        public string Name => _name;
        public string IsSummer => _isSummer;
    }
}
