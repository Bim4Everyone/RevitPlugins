using System.Drawing;
using System.Windows.Media.Imaging;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Extensions;

namespace RevitMarkPlacement.ViewModels;

internal class SpotDimensionTypeViewModel : BaseViewModel {
    private readonly SpotDimensionType _spotDimensionType;

    public SpotDimensionTypeViewModel(SpotDimensionType spotDimensionType) {
        _spotDimensionType = spotDimensionType;
    }

    public string Name => _spotDimensionType.Name;
    public BitmapSource PreviewImage => _spotDimensionType.GetPreviewImage(new Size(24, 24)).ConvertToBitmapSource();
}
