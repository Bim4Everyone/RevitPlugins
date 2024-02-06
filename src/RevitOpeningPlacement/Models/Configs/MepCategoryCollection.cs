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
        public MepCategory this[MepCategoryEnum category] => Categories.FirstOrDefault(item => item.Name.Equals(RevitRepository.MepCategoryNames[category]));
        public IEnumerator<MepCategory> GetEnumerator() => Categories.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public IEnumerable<MepCategory> GetCategories(FittingCategoryEnum fittingCategory) {
            switch(fittingCategory) {
                case FittingCategoryEnum.PipeFitting: {
                    yield return this[MepCategoryEnum.Pipe];
                    break;
                }
                case FittingCategoryEnum.CableTrayFitting: {
                    yield return this[MepCategoryEnum.CableTray];
                    break;
                }
                case FittingCategoryEnum.DuctFitting: {
                    yield return this[MepCategoryEnum.RoundDuct];
                    yield return this[MepCategoryEnum.RectangleDuct];
                    break;
                }
                case FittingCategoryEnum.ConduitFitting: {
                    yield return this[MepCategoryEnum.Conduit];
                    break;
                }
            }
        }
    }
}