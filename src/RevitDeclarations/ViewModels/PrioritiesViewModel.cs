using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitDeclarations.Models;

namespace RevitDeclarations.ViewModels {
    public class PrioritiesViewModel {
        private readonly List<PriorityViewModel> _priorities;

        public PrioritiesViewModel(ICollection<RoomPriority> priorities) {
            _priorities = priorities
                .OrderBy(x => x.OrdinalNumber)
                .Select(x => new PriorityViewModel(x))
                .ToList();
        }

        public IList<PriorityViewModel> Priorities => _priorities;
    }
}
