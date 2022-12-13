using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class ObservableCollectionExtension {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> elements) {
            foreach(var element in elements) {
                collection.Add(element);
            }
        }
    }
}
