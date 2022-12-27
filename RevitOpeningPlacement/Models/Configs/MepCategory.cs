using System.Collections.Generic;
using System.Linq;

namespace RevitOpeningPlacement.Models.Configs {
    internal class MepCategory {
        public bool IsRound { get; set; }
        public bool IsSelected { get; set; }
        public string Name { get; set; }
        public string ImageSource { get; set; }
        public SizeCollection MinSizes { get; set; } = new SizeCollection();
        public List<Offset> Offsets { get; set; } = new List<Offset>();
        public List<StructureCategory> Intersections { get; set; } = new List<StructureCategory>();
        public int Rounding { get; set; }

        public double GetOffset(double size) {
            return Offsets.Select(item => item.GetTransformedToInternalUnit())
                .FirstOrDefault(item => item.From <= size && item.To >= size)?.OffsetValue * 2 ?? 0;
        }
    }
}
