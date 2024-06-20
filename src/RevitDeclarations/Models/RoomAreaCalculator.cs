using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal class RoomAreaCalculator {
        private readonly PrioritiesConfig _priorConfig;
        private readonly RoomElement _room;

        public RoomAreaCalculator(DeclarationSettings settings, RoomElement room) {
            _priorConfig = settings.PrioritiesConfig;
            _room = room;
        }

        public double CalculateAreaCoefRevit() {
            double areaCoefRevit;

            if(_priorConfig.Balcony.CheckName(_room.Name) || _priorConfig.Terrace.CheckName(_room.Name)) {
                areaCoefRevit = _room.AreaRevit * _priorConfig.Balcony.AreaCoefficient;
            } else if(_priorConfig.Loggia.CheckName(_room.Name)) {
                areaCoefRevit = _room.AreaRevit * _priorConfig.Loggia.AreaCoefficient;
            } else {
                areaCoefRevit = _room.AreaRevit;
            }

            return areaCoefRevit;
        }
    }
}
