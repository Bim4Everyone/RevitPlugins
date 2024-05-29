using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.TypeNamesProviders {
    internal class TypeNamesProvider : ITypeNamesProvider {
        private readonly bool _mepSystemIsRound;

        public TypeNamesProvider(bool mepSystemIsRound) {
            _mepSystemIsRound = mepSystemIsRound;
        }

        public IEnumerable<string> GetTypeNames() {
            if(_mepSystemIsRound) {
                yield return RevitRepository.OpeningTaskTypeName[OpeningType.WallRound];
            }
            yield return RevitRepository.OpeningTaskTypeName[OpeningType.WallRectangle];
        }
    }
}
