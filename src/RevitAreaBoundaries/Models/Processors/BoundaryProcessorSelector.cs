using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Settings;

namespace RevitAreaBoundaries.Models.Processors;

internal class BoundaryProcessorSelector (OutBoundaryProcessor outerProcessor){
    public IBoundaryProcessor SelectProcessor(AreaBoundarySettings areaBoundarySettings) {
        return areaBoundarySettings.AlgorithmType switch {
            AlgorithmType.OuterBoundary => outerProcessor,
            _ => outerProcessor
        };
    }
    
}
