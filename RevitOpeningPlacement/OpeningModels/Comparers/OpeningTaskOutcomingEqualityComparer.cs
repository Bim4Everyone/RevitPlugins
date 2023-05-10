using System.Collections.Generic;

namespace RevitOpeningPlacement.OpeningModels.Comparers {
    internal class OpeningTaskOutcomingEqualityComparer : IEqualityComparer<OpeningTaskOutcoming> {
        public bool Equals(OpeningTaskOutcoming x, OpeningTaskOutcoming y) {
            if((x is null) || (y is null)) {
                return false;
            } else {
                return x.Equals(y);
            }
        }

        public int GetHashCode(OpeningTaskOutcoming obj) {
            return obj.GetHashCode();
        }
    }
}
