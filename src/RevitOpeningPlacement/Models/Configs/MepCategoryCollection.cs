using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RevitOpeningPlacement.Models.Configs {
    internal class MepCategoryCollection : IEnumerable<MepCategory> {
        public MepCategoryCollection() {
            Categories = new List<MepCategory>() {
                GetDefaultPipeCategory(),
                GetDefaultRectDuctCategory(),
                GetDefaultRoundDuctCategory(),
                GetDefaultCableTrayCategory(),
                GetDefaultConduitCategory()
            };
        }

        public MepCategoryCollection(IEnumerable<MepCategory> categories) {
            Categories = new List<MepCategory>(categories);
        }


        public List<MepCategory> Categories { get; set; }
        public int Count => Categories.Count;
        public MepCategory this[MepCategoryEnum category] => Categories
            .FirstOrDefault(item => item.Name.Equals(RevitRepository.MepCategoryNames[category]));


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


        public MepCategory GetDefaultPipeCategory() {
            return new MepCategory(
                    RevitRepository.MepCategoryNames[MepCategoryEnum.Pipe],
                    MepCategory.PipeImageSource,
                    true) {
                MinSizes = GetDefaultRoundSizes()
            };
        }

        public MepCategory GetDefaultRectDuctCategory() {
            return new MepCategory(
                    RevitRepository.MepCategoryNames[MepCategoryEnum.RectangleDuct],
                    MepCategory.RectDuctImageSource,
                    false) {
                MinSizes = GetDefaultRectSizes()
            };
        }

        public MepCategory GetDefaultRoundDuctCategory() {
            return new MepCategory(
                    RevitRepository.MepCategoryNames[MepCategoryEnum.RoundDuct],
                    MepCategory.RoundDuctImageSource,
                    true) {
                MinSizes = GetDefaultRoundSizes()
            };
        }

        public MepCategory GetDefaultConduitCategory() {
            return new MepCategory(
                    RevitRepository.MepCategoryNames[MepCategoryEnum.Conduit],
                    MepCategory.ConduitImageSource,
                    true) {
                MinSizes = GetDefaultRoundSizes()
            };
        }

        public MepCategory GetDefaultCableTrayCategory() {
            return new MepCategory(
                    RevitRepository.MepCategoryNames[MepCategoryEnum.CableTray],
                    MepCategory.CableTrayImageSource,
                    false) {
                MinSizes = GetDefaultRectSizes()
            };
        }

        private SizeCollection GetDefaultRectSizes() {
            return new SizeCollection(new List<Size>{
                new Size() {
                    Name = RevitRepository.ParameterNames[Parameters.Width],
                    Value = MepCategory.DefaultMinSizeMm
                },
                new Size() {
                    Name = RevitRepository.ParameterNames[Parameters.Height],
                    Value = MepCategory.DefaultMinSizeMm
                }
            });
        }

        private SizeCollection GetDefaultRoundSizes() {
            return new SizeCollection(new List<Size>{
                new Size() {
                    Name = RevitRepository.ParameterNames[Parameters.Diameter],
                    Value = MepCategory.DefaultMinSizeMm
                }
            });
        }
    }
}
