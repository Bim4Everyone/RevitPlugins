using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders {
    internal class ZeroAngleFinder : IAngleFinder {
        public Rotates GetAngle() {
            return new Rotates(0, 0, 0);
        }
    }
}
