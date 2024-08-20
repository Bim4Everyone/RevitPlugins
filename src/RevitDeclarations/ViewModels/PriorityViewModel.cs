using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Office.Interop.Excel;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    public class PriorityViewModel {
        private readonly RoomPriority _priority;
        private readonly int _ordinalNumber;
        private readonly string _name;
        private readonly string _areaCoefficient;
        private readonly string _isSummer;
        private readonly string _isLiving;

        public PriorityViewModel(RoomPriority priority) {
            _priority = priority;
            _ordinalNumber = priority.OrdinalNumber;
            _name = priority.Name;
            _areaCoefficient = priority.AreaCoefficient.ToString();

            if(priority.IsSummer) {
                _isSummer = "Да";
            } else {
                _isSummer = "Нет";
            }

            if(priority.IsLiving) {
                _isLiving = "Да";
            } else {
                _isLiving = "Нет";
            }
        }

        public RoomPriority Priority => _priority;
        public int OrdinalNumber => _ordinalNumber;
        public string Name => _name;
        public string AreaCoefficient => _areaCoefficient;
        public string IsSummer => _isSummer;
        public string IsLiving => _isLiving;
    }
}
