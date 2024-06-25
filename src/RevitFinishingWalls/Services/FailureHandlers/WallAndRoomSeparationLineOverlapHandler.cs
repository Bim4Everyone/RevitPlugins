using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitFinishingWalls.Services.FailureHandlers {
    /// <summary>
    /// Подавляет предупреждение <see cref="BuiltInFailures.OverlapFailures.WallRoomSeparationOverlap"/>
    /// </summary>
    internal class WallAndRoomSeparationLineOverlapHandler : IFailuresPreprocessor {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor) {
            //https://thebuildingcoder.typepad.com/blog/2010/04/failure-api.html

            IList<FailureMessageAccessor> failures = failuresAccessor.GetFailureMessages();
            foreach(FailureMessageAccessor fma in failures) {
                FailureDefinitionId failureId = fma.GetFailureDefinitionId();

                if(BuiltInFailures.OverlapFailures.WallRoomSeparationOverlap == failureId) {
                    failuresAccessor.DeleteWarning(fma);
                }
            }
            return FailureProcessingResult.Continue;
        }
    }
}
