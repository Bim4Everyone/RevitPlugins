using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RevitOpeningPlacement.Models.Configs {
    internal class MepCategoryCollection : IEnumerable<MepCategory> {
        public MepCategoryCollection() {
            Categories = new List<MepCategory>();
        }
        public MepCategoryCollection(IEnumerable<MepCategory> categories) {
            Categories = new List<MepCategory>(categories);
        }
        public List<MepCategory> Categories { get; set; } = new List<MepCategory>();
        public int Count => Categories.Count;
        public MepCategory this[CategoryEnum category] => Categories.FirstOrDefault(item => item.Name.Equals(RevitRepository.CategoryNames[category]));
        public IEnumerator<MepCategory> GetEnumerator() => Categories.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
