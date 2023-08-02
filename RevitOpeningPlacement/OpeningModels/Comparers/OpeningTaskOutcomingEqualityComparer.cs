using System.Collections.Generic;

namespace RevitOpeningPlacement.OpeningModels.Comparers {
    internal class OpeningTaskOutcomingEqualityComparer : IEqualityComparer<OpeningMepTaskOutcoming> {
        public bool Equals(OpeningMepTaskOutcoming x, OpeningMepTaskOutcoming y) {
            if((x is null) || (y is null)) {
                return false;
            } else {
                return x.Equals(y);
            }
        }

        public int GetHashCode(OpeningMepTaskOutcoming obj) {
            return obj.GetHashCode();
        }
    }
}
