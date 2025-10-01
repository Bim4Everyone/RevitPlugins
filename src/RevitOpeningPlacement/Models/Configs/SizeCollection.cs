using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RevitOpeningPlacement.Models.Configs;
internal class SizeCollection : IEnumerable<Size> {
    public SizeCollection() {
        Sizes = [];
    }
    public SizeCollection(IEnumerable<Size> sizes) {
        Sizes = [.. sizes];
    }

    public Size this[Parameters index] { get => Sizes.FirstOrDefault(item => item.Name.Equals(RevitRepository.ParameterNames[index]));
    }

    private List<Size> Sizes { get; }

    public IEnumerator<Size> GetEnumerator() {
        return Sizes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
