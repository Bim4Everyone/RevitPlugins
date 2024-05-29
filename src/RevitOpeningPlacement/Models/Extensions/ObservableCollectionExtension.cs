using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class ObservableCollectionExtension {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> elements) {
            foreach(var element in elements) {
                collection.Add(element);
            }
        }
    }
}
