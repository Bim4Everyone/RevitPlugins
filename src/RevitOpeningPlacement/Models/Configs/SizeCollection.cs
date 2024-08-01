using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RevitOpeningPlacement.Models.Configs {
    internal class SizeCollection : IEnumerable<Size> {
        public SizeCollection() {
            Sizes = new List<Size>();
        }
        public SizeCollection(IEnumerable<Size> sizes) {
            Sizes = new List<Size>(sizes);
        }

        public Size this[Parameters index] => Sizes.FirstOrDefault(item => item.Name.Equals(RevitRepository.ParameterNames[index]));
        private List<Size> Sizes { get; }

        public IEnumerator<Size> GetEnumerator() => Sizes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
