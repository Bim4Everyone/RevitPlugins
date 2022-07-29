using System.Collections.Generic;
using System.Linq;

namespace RevitOpeningPlacement.Models.Configs {
    internal class MepCategory {
        public string Name { get; set; }
        public string ImageSource { get; set; }
        public SizeCollection MinSizes { get; set; } = new SizeCollection();
        public List<Offset> Offsets { get; set; } = new List<Offset>();

        public double GetOffset(double size) {
            return Offsets.Select(item => item.GetTransformedToInternalUnit())
                .FirstOrDefault(item => item.From <= size && item.To >= size)?.OffsetValue ?? 0;
        }
    }
}
