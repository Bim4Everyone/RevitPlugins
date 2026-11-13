using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;

internal sealed class BuilderFactory(
    BuildCoordVolumeSettings settings,
    BuildCoordVolumeServices services) {
    /// <summary>
    /// Метод создания билдеров ICoordVolumeBuilder.
    /// </summary>
    /// <remarks>
    /// В данном методе создаются билдеры, исходя из выбранного пользователем алгоритма.
    /// </remarks>
    /// <param name="algorithmType">Тип алгоритма.</param>
    /// <returns>
    /// ICoordVolumeBuilder.
    /// </returns>
    public ICoordVolumeBuilder Create(AlgorithmType algorithmType) {
        return algorithmType == AlgorithmType.SlabBasedAlgorithm
            ? new SlabBasedCoordVolumeBuilder(services, settings)
            : new ParamBasedCoordVolumeBuilder(services, settings);
    }
}
