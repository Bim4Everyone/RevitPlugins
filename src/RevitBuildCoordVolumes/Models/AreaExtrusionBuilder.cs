using System.Windows;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models;
internal class AreaExtrusionBuilder : IBuildAreaExtrusion {
    public void BuildAreaExtrusion(RevitArea area) {
        MessageBox.Show("Привет, это простой алгоритм");
    }
}
