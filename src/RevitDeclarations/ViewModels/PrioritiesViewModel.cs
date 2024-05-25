using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    public class PrioritiesViewModel {
        private readonly List<PriorityViewModel> _priorities;

        public PrioritiesViewModel(List<RoomPriority> priorities) {
            _priorities = priorities
                .OrderBy(x => x.OrdinalNumber)
                .Select(x => new PriorityViewModel(x))
                .ToList();
        }

        public List<PriorityViewModel> Priorities => _priorities;
    }
}
