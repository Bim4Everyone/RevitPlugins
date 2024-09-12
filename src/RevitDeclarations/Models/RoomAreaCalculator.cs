namespace RevitDeclarations.Models {
    internal class RoomAreaCalculator {
        private readonly PrioritiesConfig _priorConfig;
        private readonly RoomElement _room;

        public RoomAreaCalculator(DeclarationSettings settings, RoomElement room) {
            _priorConfig = settings.PrioritiesConfig;
            _room = room;
        }

        public double CalculateAreaCoefRevit() {
            double coef = _priorConfig.GetPriorityByNameOrDefault(_room.Name).AreaCoefficient;
            return _room.AreaRevit * coef;
        }

        public double CalculateAreaLivingRevit() {
            bool isLiving = _priorConfig.GetPriorityByNameOrDefault(_room.Name).IsLiving;

            if (isLiving) {
                return _room.AreaRevit;
            }
            return 0;
        }

        public double CalculateAreaNonSummerRevit() {
            bool isSummer = _priorConfig.GetPriorityByNameOrDefault(_room.Name).IsSummer;

            if(!isSummer) {
                return _room.AreaRevit;
            }
            return 0;
        }
    }
}
