using System.Collections.Generic;
using System.Linq;

namespace RevitGenLookupTables.Extensions;

internal static class CombinationExtensions {
    public static IEnumerable<IEnumerable<T>> Combination<T>(this IEnumerable<IEnumerable<T>> listElements) {
        var array = listElements.ToArray();
        int countElements = array.Count();
        if(countElements > 1) {
            var combinations = Combination(array.Skip(1)).ToArray();
            foreach(var element in array.First()) {
                foreach(var combination in combinations) {
                    yield return new[] { element }.Concat(combination);
                }
            }
        } else if(countElements == 1) {
            foreach(var element in array.First().Select(x => new[] { x })) {
                yield return element;
            }
        }
    }
}
